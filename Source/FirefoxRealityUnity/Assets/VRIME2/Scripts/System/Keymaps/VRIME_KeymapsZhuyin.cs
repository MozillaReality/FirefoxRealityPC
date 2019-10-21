// ========================================================================== //
//  Created: 2019-01-14
// ========================================================================== //
//  Copy From VRIME Ver 1
//  Copyright 2018 HTC Xindian HTC1
// ========================================================================== //
namespace VRIME2
{
	using Mono.Data.SqliteClient;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEngine;

	public class VRIME_KeymapsZhuyin : VRIME_Keymaps
	{
        public VRIME_KeymapsZhuyin()
        {
            LoadDataFromBaseTable();
            LoadDataFromMappingTable();            
            
            AddDataNotInDatabase();
        }
		private static VRIME_KeymapsZhuyin _instance;
        public static VRIME_KeymapsZhuyin Instance {
            get {
                if (_instance == null) {
                    _instance = new VRIME_KeymapsZhuyin();
                }
                return _instance;
            }
        }

        #region private field
        private const string debugTag = "keymaps_zhuyin";
        private const string DatabasePath = "/BPMF.db";
        private const int MaxNearKeys = 4;

        private const string toneChars = " ˊˇˋ˙"; // 代表聲調
        private const string bpmfChars = "ㄅㄆㄇㄈㄉㄊㄋㄌㄍㄎㄏㄐㄑㄒㄓㄔㄕㄖㄗㄘㄙㄧㄨㄩㄚㄛㄜㄝㄞㄟㄠㄡㄢㄣㄤㄥㄦ";

        private Dictionary<string, List<Words>> cacheResult = new Dictionary<string, List<Words>>();
        private Queue<string> cacheCode = new Queue<string>();
        private const int MAX_CACHE = 20;
        
        private Dictionary<char, char[]> nearAlphabetDict;

        private string connectionStr = string.Empty;
        private IDbConnection databaseConnection;
        private IDbCommand databaseCommand;
        private IDataReader dataReader;
        #endregion
        #region public function
        public override List<string> GetNearKeys(string key)
        {
            var nearKeys = new List<string>();
            if (nearAlphabetDict == null)
            {
                VRIME_Debugger.LogError(debugTag, "Plz call 'AddDataNotInDatabase' first!");
                return null;
            }

            var tempKey = string.Empty;
            try
            {                               
                tempKey = key; // use all key to check near key                
                var firstChar = tempKey.First();

                if (nearAlphabetDict.ContainsKey(firstChar))
                {
                    List<char> firstNearKeys = new List<char>(nearAlphabetDict[firstChar]);
                    List<char> includedKeys = new List<char>();
                    while(nearKeys.Count() < MaxNearKeys && tempKey.Length > 0) {
                        for (var i = 0; i < firstNearKeys.Count; i++)
                        {
                            char newKey = firstNearKeys[i];
                            if(includedKeys.Contains(newKey)) {                                
                                continue; // this key have found words
                            }
                                
                            // replace the 1st char
                            var nearKey = new Regex(Regex.Escape(tempKey.First().ToString()))
                                .Replace(tempKey, newKey.ToString(), 1);
                            
                            if ((HasDisplays(nearKey) || HasCandidates(nearKey)) && nearKeys.Count() < MaxNearKeys)
                            {                                
                                nearKeys.Add(nearKey);
                                includedKeys.Add(firstNearKeys[i]);
                            }
                        }
                        tempKey = tempKey.Remove(tempKey.Length - 1);
                    }
                }
            }
            catch (Exception e)
            {
                VRIME_Debugger.LogError(debugTag, e.ToString());
                return null;
            }

            return nearKeys;
        }
        #endregion
        #region override function
        public override string GetDisplayCode(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                VRIME_Debugger.LogError(debugTag, "Key is null or empty!");
                return null;
            }

            if (!Regex.IsMatch(key, @"^[a-z]+$"))
            {
                VRIME_Debugger.LogError(debugTag, "'a' to 'z' only!");
                return null;
            }

            var remainKey = string.Empty;
            var displays = new List<string>();

            while (key.Length > 0)
            {
                if (HasDisplays(key))
                {
                    displays.Add(keymapDict[key].Displays[0].Code);
                    key = remainKey;
                    remainKey = string.Empty;
                }
                else
                {
                    remainKey = key.Last() + remainKey;
                    key = key.Remove(key.Length - 1);
                }
            }

            return displays.Count == 0 ? null : string.Join(" ", displays.ToArray());
        }
        public override List<Words> GetCandidates(string key)
        {
            Debug.Log("GetCandidates : " + key);
            if (string.IsNullOrEmpty(key))
            {
                VRIME_Debugger.LogError(debugTag, "Key is null or empty!");
                return null;
            }
            if (key.Replace(" ", string.Empty) == string.Empty)
            {
                VRIME_Debugger.LogError(debugTag, "Key is whitespace!");
                return null;
            }

            var code = key;
            var displayList = code.Split(' ').ToList();
            var candidateList = new List<Words>();
            var candidate = string.Empty;
            var syllable = 0;

            // Get the 1st candidate.
            syllable = displayList.Count();
            var tempKey = key;
            var remainKey = string.Empty;
            var tempList = new List<string>();            

            // while (tempKey.Length > 0)
            // {         
            //     string fuzzyKey = Regex.Replace(tempKey, " |ˊ|ˇ|ˋ|˙", "");
            //     if (HasCandidates(tempKey))
            //     {                                        
            //         candidate += keymapDict[tempKey].Candidates[0].Value;
            //         tempKey = remainKey;
            //         remainKey = string.Empty;
            //     } else if (HasCandidates(fuzzyKey)) {
            //         candidate += keymapDict[fuzzyKey].Candidates[0].Value;
            //         tempKey = remainKey;
            //         remainKey = string.Empty;
            //     } else if(HasDisplays(tempKey)) {
            //         candidate += keymapDict[tempKey].Displays[0].Value;
            //         tempKey = remainKey;
            //         remainKey = string.Empty;                    
            //     } else if(HasDisplays(fuzzyKey)) {
            //         candidate += keymapDict[fuzzyKey].Displays[0].Value;
            //         tempKey = remainKey;
            //         remainKey = string.Empty;                    
            //     } 
            //     else
            //     {
            //         if(tempKey.Length > 1 && toneChars.Contains(tempKey.Last()) && bpmfChars.Contains(tempKey[tempKey.Length-2]) ) {
            //             remainKey = tempKey.Substring(tempKey.Length - 2, 2) + remainKey;
            //             tempKey = tempKey.Remove(tempKey.Length - 2);
            //         } else {
            //             remainKey = tempKey.Last() + remainKey;                    
            //             tempKey = tempKey.Remove(tempKey.Length - 1);
            //         }
            //     }
            // }
            // candidateList.Add(new Words
            // {
            //     Syllable = syllable,
            //     Code = code,
            //     Value = candidate
            // });

            tempList.Add(candidate);

            // Get the other candidates
            tempKey = key;
            _getCandidate(tempKey, candidateList, tempList);
            // while (tempKey.Length > 0)
            // {
            //     if (HasDisplays(tempKey))
            //     {                    
            //         foreach(Words w in keymapDict[tempKey].Displays) {
            //             if(!tempList.Contains(w.Value)) {
            //                 candidateList.Add(w);
            //                 tempList.Add(w.Value);
            //             }
            //         }
            //         //candidateList.AddRange(keymapDict[tempKey].Displays);
            //     }
            //     if (HasCandidates(tempKey))
            //     {
            //         foreach(Words w in keymapDict[tempKey].Candidates) {
            //             if(!tempList.Contains(w.Value)) {
            //                 candidateList.Add(w);
            //                 tempList.Add(w.Value);
            //             }
            //         }
            //         //candidateList.AddRange(keymapDict[tempKey].Candidates);
            //     }
            //     tempKey = tempKey.Remove(tempKey.Length - 1);
            // }

            // remove ˊˇˋ˙
            // tempKey = Regex.Replace(key, "ˊ|ˇ|ˋ|˙| ", "");
            // if(!tempKey.Equals(key)) {
            //     _getCandidate(tempKey, candidateList, tempList);
            // }


            if(candidateList.Count > 1) 
            {
                if(candidateList[0].Value.Equals(candidateList[1].Value))
                {
                    candidateList.RemoveAt(1);
                }
            }


            return candidateList;
        }

        private void _getCandidate(string tempKey, List<Words> candidateList, List<string> tempList) {            
            string srcKey = tempKey;
            List<Words> displays = new List<Words>();
            List<Words> candidates = new List<Words>();
            while (tempKey.Length > 0)
            {                
                List<Words> cache = findInCache(tempKey);
                if(cache != null) {
                    // found cache
                    foreach(Words w in cache) {
                        if(!tempList.Contains(w.Value)) {
                            tempList.Add(w.Value);
                            candidateList.Add(w);
                        }
                    }
                    addInToCache(srcKey, new List<Words>(candidateList));
                    return;
                }
                displays.Clear();
                candidates.Clear();
                if (HasDisplays(tempKey))
                {                                        
                    foreach(Words w in keymapDict[tempKey].Displays) {
                        if(!tempList.Contains(w.Value)) {                            
                            //candidateList.Add(w);
                            displays.Add(w);
                            tempList.Add(w.Value);
                        }
                    }
                    //candidateList.AddRange(keymapDict[tempKey].Displays);
                }
                if (HasCandidates(tempKey))
                {                    
                    foreach(Words w in keymapDict[tempKey].Candidates) {
                        if(!tempList.Contains(w.Value)) {                                                        
                            //candidateList.Add(w);
                            candidates.Add(w);
                            tempList.Add(w.Value);
                        }
                    }
                    //candidateList.AddRange(keymapDict[tempKey].Candidates);
                }

                bool isFullMatched = (displays.Count > 0 || candidates.Count > 0);

                
                _getCandidateFuzzy(tempKey, displays, candidates, tempList, isFullMatched);

                    // if(displays.Count > 1 && candidates.Count > 1) {
                    //     displays.Insert(1, candidates.First());
                    //     candidates.RemoveAt(0);
                    // }
                

                candidateList.AddRange(displays);
                candidateList.AddRange(candidates);
                
                
                if(tempKey.Length > 1 && toneChars.Contains(tempKey.Last()) && bpmfChars.Contains(tempKey[tempKey.Length-2]) ) {
                    tempKey = tempKey.Remove(tempKey.Length - 2);
                } else {
                    tempKey = tempKey.Remove(tempKey.Length - 1);
                }                
            }
            addInToCache(srcKey, new List<Words>(candidateList));
        }

        private void _getCandidateFuzzy(string tempKey, List<Words> displays, List<Words> candidates, List<string> tempList, bool isFullMatched) {
            string newKey = Regex.Replace(tempKey, " |ˊ|ˇ|ˋ|˙", "");
            Debug.Log("_getCandidateFuzzy, tempKey: " + tempKey + ", newKey: " + newKey);
            if(tempKey.Equals(newKey)) {
                return;
            }
            

            if (!isFullMatched && HasDisplays(newKey))
            {                                        
                foreach(Words w in keymapDict[newKey].Displays) {
                    if(!tempList.Contains(w.Value)) {                            
                        //candidateList.Add(w);
                        Words newW = w.Clone();
                        newW.Code = tempKey;
                        displays.Add(newW);
                        tempList.Add(w.Value);
                    }
                }
                //candidateList.AddRange(keymapDict[tempKey].Displays);
            }
            if (HasCandidates(newKey))
            {                    
                foreach(Words w in keymapDict[newKey].Candidates) {
                    if(!tempList.Contains(w.Value)) {                                                        
                        //candidateList.Add(w);
                        Words newW = w.Clone();
                        newW.Code = tempKey;
                        candidates.Add(newW);
                        tempList.Add(w.Value);
                    }
                }
                //candidateList.AddRange(keymapDict[tempKey].Candidates);
            }
        }

        public override List<Words> GetDisplays(string key)
        {
            return base.GetDisplays(key);
        }
        #endregion
        #region private function
        private List<Words> findInCache(string key) {
            if(cacheCode.Contains(key)) {
                return cacheResult[key];
            }
            return null;
        }

        private void addInToCache(string key, List<Words> words) {
            if(!cacheCode.Contains(key)) {
                if(cacheCode.Count >= MAX_CACHE) {
                    string removeKey = cacheCode.Dequeue();
                    cacheResult.Remove(removeKey);
                }
                cacheCode.Enqueue(key);                
                cacheResult.Add(key, words);
            }
        }

        private void addInToCacheWithoutFirst(string key, List<Words> words) {
            if(!cacheCode.Contains(key)) {
                if(cacheCode.Count >= MAX_CACHE) {
                    string removeKey = cacheCode.Dequeue();
                    cacheResult.Remove(removeKey);
                }
                cacheCode.Enqueue(key);
                words.RemoveAt(0);
                cacheResult.Add(key, words);
            }
        }

        private void LoadDataFromBaseTable()
        {
            // _id: 1500, display: 斬, code: ㄓㄢˇ , code2: ㄓㄢ
            connectionStr = "URI=File:" + Application.streamingAssetsPath + DatabasePath;
            databaseConnection = new SqliteConnection(connectionStr);
            databaseConnection.Open();
            databaseCommand = databaseConnection.CreateCommand();
            databaseCommand.CommandText = "SELECT * FROM BPMFBase ORDER BY code,cnt DESC";
            dataReader = databaseCommand.ExecuteReader();

            while (dataReader.Read())
            {
                var display = dataReader.IsDBNull(1) ? null : dataReader.GetString(1);
                var code = dataReader.IsDBNull(2) ? null : dataReader.GetString(2);
                var code2 = dataReader.IsDBNull(3) ? null : dataReader.GetString(3);

                AddWordsIntoKeymapDict(
                    code,
                    code,
                    display,
                    null);

                if(code2 != null && !code2.Equals(code)) {

                AddWordsIntoKeymapDict(
                    code2,
                    code2,
                    display,
                    null);                    
                }
            }

            databaseConnection.Close();
        }

        private void LoadDataFromMappingTable()
        {
            // _id: 1495, display: 一池, code: ㄧ ㄔˊ, code2: ㄧㄔ
            connectionStr = "URI=File:" + Application.streamingAssetsPath + DatabasePath;
            databaseConnection = new SqliteConnection(connectionStr);
            databaseConnection.Open();
            databaseCommand = databaseConnection.CreateCommand();
            databaseCommand.CommandText = "SELECT * FROM BPMFMapping ORDER BY code,cnt DESC";
            dataReader = databaseCommand.ExecuteReader();

            while (dataReader.Read())
            {
                var display = dataReader.IsDBNull(1) ? null : dataReader.GetString(1);
                var code = dataReader.IsDBNull(2) ? null : dataReader.GetString(2);
                var code2 = dataReader.IsDBNull(3) ? null : dataReader.GetString(3);

                AddWordsIntoKeymapDict(
                    code,
                    code,
                    null,
                    display);

                if(!code2.Equals(code)) {
                AddWordsIntoKeymapDict(
                    code2,
                    code2,
                    null,
                    display
                    );                    
                }                    
            }

            databaseConnection.Close();
        }

        private void AddDataNotInDatabase()
        {
            // AddWordsIntoKeymapDict("a", "a", "a|A");
            // AddWordsIntoKeymapDict("b", "b", "b|B");
            // AddWordsIntoKeymapDict("c", "c", "c|C");
            // AddWordsIntoKeymapDict("d", "d", "d|D");
            // AddWordsIntoKeymapDict("e", "e", "e|E");
            // AddWordsIntoKeymapDict("f", "f", "f|F");
            // AddWordsIntoKeymapDict("g", "g", "g|G");
            // AddWordsIntoKeymapDict("h", "h", "h|H");
            // AddWordsIntoKeymapDict("i", "i", "i|I", "喔|哦|噢");
            // AddWordsIntoKeymapDict("j", "j", "j|J");
            // AddWordsIntoKeymapDict("k", "k", "k|K");
            // AddWordsIntoKeymapDict("l", "l", "l|L");
            // AddWordsIntoKeymapDict("m", "m", "m|M");
            // AddWordsIntoKeymapDict("n", "n", "n|N");
            // AddWordsIntoKeymapDict("o", "o", "o|O");
            // AddWordsIntoKeymapDict("p", "p", "p|P");
            // AddWordsIntoKeymapDict("q", "q", "q|Q");
            // AddWordsIntoKeymapDict("r", "r", "r|R");
            // AddWordsIntoKeymapDict("s", "s", "s|S");
            // AddWordsIntoKeymapDict("t", "t", "t|T");
            // AddWordsIntoKeymapDict("u", "u", "u|U", "有|要");
            // AddWordsIntoKeymapDict("v", "v", "v|V", "吧|被");
            // AddWordsIntoKeymapDict("w", "w", "w|W");
            // AddWordsIntoKeymapDict("x", "x", "x|X");
            // AddWordsIntoKeymapDict("y", "y", "y|Y");
            // AddWordsIntoKeymapDict("z", "z", "z|Z");

            if (nearAlphabetDict == null)
            {
                nearAlphabetDict = new Dictionary<char, char[]>();
            }

            nearAlphabetDict.Add('ㄅ', new char[] {       'ㄉ', 'ㄆ' });
            nearAlphabetDict.Add('ㄆ', new char[] {       'ㄊ', 'ㄇ', 'ㄅ', 'ㄉ' });
            nearAlphabetDict.Add('ㄇ', new char[] {       'ㄋ', 'ㄈ', 'ㄆ', 'ㄊ' });
            nearAlphabetDict.Add('ㄈ', new char[] {       'ㄌ',       'ㄇ', 'ㄋ' });
                  
            nearAlphabetDict.Add('ㄉ', new char[] {             'ㄊ', 'ㄆ', 'ㄅ' });
            nearAlphabetDict.Add('ㄊ', new char[] {       'ㄍ', 'ㄋ', 'ㄇ', 'ㄆ', 'ㄉ' });
            nearAlphabetDict.Add('ㄋ', new char[] { 'ㄌ', 'ㄎ',       'ㄈ', 'ㄇ', 'ㄊ', 'ㄍ' });
            nearAlphabetDict.Add('ㄌ', new char[] { 'ㄖ', 'ㄋ', 'ㄏ',       'ㄈ',       'ㄎ' });
      
            nearAlphabetDict.Add('ㄍ', new char[] {       'ㄐ', 'ㄎ', 'ㄋ', 'ㄊ' });
            nearAlphabetDict.Add('ㄎ', new char[] {       'ㄑ', 'ㄏ', 'ㄌ', 'ㄋ', 'ㄍ', 'ㄐ' });
            nearAlphabetDict.Add('ㄏ', new char[] {       'ㄒ',             'ㄌ', 'ㄎ', 'ㄑ' });
      
      
            nearAlphabetDict.Add('ㄐ', new char[] {       'ㄔ', 'ㄑ', 'ㄎ', 'ㄍ',       'ㄓ' });
            nearAlphabetDict.Add('ㄑ', new char[] {       'ㄕ', 'ㄒ', 'ㄏ', 'ㄎ', 'ㄐ', 'ㄔ' });
            nearAlphabetDict.Add('ㄒ', new char[] {       'ㄖ',             'ㄏ', 'ㄑ', 'ㄕ' });
      
            nearAlphabetDict.Add('ㄓ', new char[] { 'ㄗ',       'ㄔ', 'ㄐ' });
            nearAlphabetDict.Add('ㄔ', new char[] { 'ㄘ', 'ㄗ', 'ㄕ', 'ㄑ', 'ㄐ', 'ㄓ' });
            nearAlphabetDict.Add('ㄕ', new char[] { 'ㄙ', 'ㄘ', 'ㄖ', 'ㄒ', 'ㄑ', 'ㄔ', 'ㄗ' });
            nearAlphabetDict.Add('ㄖ', new char[] { 'ㄌ', 'ㄙ',             'ㄒ', 'ㄕ', 'ㄘ' });
      
            nearAlphabetDict.Add('ㄗ', new char[] { 'ㄓ', 'ㄧ', 'ㄘ', 'ㄕ', 'ㄔ' });
            nearAlphabetDict.Add('ㄘ', new char[] { 'ㄔ', 'ㄨ', 'ㄙ', 'ㄖ', 'ㄕ', 'ㄗ', 'ㄧ' });
            nearAlphabetDict.Add('ㄙ', new char[] { 'ㄕ', 'ㄩ',             'ㄖ', 'ㄘ', 'ㄨ' });
      
            nearAlphabetDict.Add('ㄧ', new char[] {       'ㄛ', 'ㄨ', 'ㄘ', 'ㄗ',       'ㄚ' });
            nearAlphabetDict.Add('ㄨ', new char[] {       'ㄜ', 'ㄩ', 'ㄙ', 'ㄘ', 'ㄧ', 'ㄛ' });
            nearAlphabetDict.Add('ㄩ', new char[] {       'ㄝ',             'ㄙ', 'ㄨ', 'ㄜ' });
      
            nearAlphabetDict.Add('ㄚ', new char[] {       'ㄞ', 'ㄛ', 'ㄧ',                 });
            nearAlphabetDict.Add('ㄛ', new char[] { 'ㄡ', 'ㄟ', 'ㄜ', 'ㄨ', 'ㄧ', 'ㄚ', 'ㄞ' });
            nearAlphabetDict.Add('ㄜ', new char[] { 'ㄦ', 'ㄠ', 'ㄝ', 'ㄩ', 'ㄨ', 'ㄛ', 'ㄟ' });
            nearAlphabetDict.Add('ㄝ', new char[] { 'ㄟ', 'ㄡ',             'ㄩ', 'ㄜ', 'ㄠ' });
      
            nearAlphabetDict.Add('ㄞ', new char[] {       'ㄢ', 'ㄟ', 'ㄛ', 'ㄚ' });
            nearAlphabetDict.Add('ㄟ', new char[] { 'ㄝ', 'ㄣ', 'ㄠ', 'ㄜ', 'ㄛ', 'ㄞ', 'ㄢ' });
            nearAlphabetDict.Add('ㄠ', new char[] {       'ㄤ', 'ㄡ', 'ㄝ', 'ㄜ', 'ㄟ', 'ㄣ' });
            nearAlphabetDict.Add('ㄡ', new char[] { 'ㄛ', 'ㄥ',             'ㄝ', 'ㄠ', 'ㄤ' });
      
            nearAlphabetDict.Add('ㄢ', new char[] { 'ㄤ', 'ㄦ', 'ㄣ', 'ㄟ', 'ㄞ' });
            nearAlphabetDict.Add('ㄣ', new char[] { 'ㄥ',       'ㄤ', 'ㄠ', 'ㄟ', 'ㄢ', 'ㄦ' });
            nearAlphabetDict.Add('ㄤ', new char[] { 'ㄢ',       'ㄥ', 'ㄡ', 'ㄠ', 'ㄣ'       });
            nearAlphabetDict.Add('ㄥ', new char[] { 'ㄣ',                   'ㄡ', 'ㄤ'       });
      
            nearAlphabetDict.Add('ㄦ', new char[] { 'ㄜ',             'ㄣ', 'ㄢ'             });

            // nearAlphabetDict.Add('q', new char[] { 'w', 's' });
            // nearAlphabetDict.Add('a', new char[] { 'e' });
            // nearAlphabetDict.Add('z', new char[] { 's', 'x' });
            // nearAlphabetDict.Add('w', new char[] { 'q', 's', 'd' });
            // nearAlphabetDict.Add('s', new char[] { 'q', 'z', 'w', 'x', 'd', 'c' });
            // nearAlphabetDict.Add('x', new char[] { 'z', 's', 'd', 'c' });
            // nearAlphabetDict.Add('e', new char[] { 'a' });
            // nearAlphabetDict.Add('d', new char[] { 'w', 's', 'x', 'c', 'r', 'f', 'v' });
            // nearAlphabetDict.Add('c', new char[] { 's', 'x', 'd', 'f', 'v' });
            // nearAlphabetDict.Add('r', new char[] { 'd', 'f', 't', 'g' });
            // nearAlphabetDict.Add('f', new char[] { 'd', 'c', 'r', 'v', 't', 'g', 'b' });
            // nearAlphabetDict.Add('v', new char[] { 'd', 'c', 'f', 'g', 'b' });
            // nearAlphabetDict.Add('t', new char[] { 'r', 'f', 'g', 'y', 'h', 'j' });
            // nearAlphabetDict.Add('g', new char[] { 'r', 'f', 'v', 't', 'b', 'y', 'h', 'n' });
            // nearAlphabetDict.Add('b', new char[] { 'f', 'v', 'g', 'h', 'n' });
            // nearAlphabetDict.Add('y', new char[] { 't', 'g', 'h', 'j' });
            // nearAlphabetDict.Add('h', new char[] { 't', 'g', 'b', 'y', 'n', 'j', 'm' });
            // nearAlphabetDict.Add('n', new char[] { 'g', 'b', 'h', 'j', 'm' });
            // nearAlphabetDict.Add('u', new char[] { 'i' });
            // nearAlphabetDict.Add('j', new char[] { 'y', 'h', 'n', 'm', 'k' });
            // nearAlphabetDict.Add('m', new char[] { 'h', 'n', 'j', 'k' });
            // nearAlphabetDict.Add('i', new char[] { 'u', 'o' });
            // nearAlphabetDict.Add('k', new char[] { 'j', 'm', 'l' });
            // nearAlphabetDict.Add('o', new char[] { 'i' });
            // nearAlphabetDict.Add('l', new char[] { 'k', 'p' });
            // nearAlphabetDict.Add('p', new char[] { 'l' });

            // new rule
            // first line
            // nearAlphabetDict.Add('q', new char[] { 'w', 'a'           });
            // nearAlphabetDict.Add('w', new char[] { 'e', 's', 'a', 'q' });
            // nearAlphabetDict.Add('e', new char[] { 'r', 'd', 's', 'w' });
            // nearAlphabetDict.Add('r', new char[] { 't', 'f', 'd', 'e' });
            // nearAlphabetDict.Add('t', new char[] { 'y', 'g', 'f', 'r' });
            // nearAlphabetDict.Add('y', new char[] { 'u', 'h', 'g', 't' });
            // nearAlphabetDict.Add('u', new char[] { 'i', 'j', 'h', 'y' });
            // nearAlphabetDict.Add('i', new char[] { 'o', 'k', 'j', 'u' });
            // nearAlphabetDict.Add('o', new char[] { 'p', 'l', 'k', 'i' });
            // nearAlphabetDict.Add('p', new char[] {           'l', 'o' });

            // // second line
            // nearAlphabetDict.Add('a', new char[] { 's', 'z',           'q', 'w' });
            // nearAlphabetDict.Add('s', new char[] { 'd', 'x', 'z', 'a', 'w', 'e' });
            // nearAlphabetDict.Add('d', new char[] { 'f', 'c', 'x', 's', 'e', 'r' });
            // nearAlphabetDict.Add('f', new char[] { 'g', 'v', 'c', 'd', 'r', 't' });
            // nearAlphabetDict.Add('g', new char[] { 'h', 'b', 'v', 'f', 't', 'y' });
            // nearAlphabetDict.Add('h', new char[] { 'j', 'n', 'b', 'g', 'y', 'u' });
            // nearAlphabetDict.Add('j', new char[] { 'k', 'm', 'n', 'h', 'u', 'i' });
            // nearAlphabetDict.Add('k', new char[] { 'l',      'm', 'j', 'i', 'o' });
            // nearAlphabetDict.Add('l', new char[] {                'k', 'o', 'p' });

            // // third line
            // nearAlphabetDict.Add('z', new char[] { 'x',      'a', 's' });
            // nearAlphabetDict.Add('x', new char[] { 'c', 'z', 's', 'd' });
            // nearAlphabetDict.Add('c', new char[] { 'v', 'x', 'd', 'f' });
            // nearAlphabetDict.Add('v', new char[] { 'b', 'c', 'f', 'g' });
            // nearAlphabetDict.Add('b', new char[] { 'n', 'v', 'g', 'h' });
            // nearAlphabetDict.Add('n', new char[] { 'm', 'b', 'h', 'j' });
            // nearAlphabetDict.Add('m', new char[] {      'n', 'j', 'k' });
        }

        private void AddWordsIntoKeymapDict(string key, string code, string displays = null, string candidates = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                VRIME_Debugger.LogError(debugTag, "Key is null or empty!");
                return;
            }
            if (key.Replace(" ", string.Empty) == string.Empty)
            {
                VRIME_Debugger.LogError(debugTag, "Key is whitespace!");
                return;
            }
            if (string.IsNullOrEmpty(code))
            {
                VRIME_Debugger.LogError(debugTag, "Code is null or empty!");
                return;
            }
            if (string.IsNullOrEmpty(displays) && string.IsNullOrEmpty(candidates))
            {
                VRIME_Debugger.LogError(debugTag, "display or candidate is null or empty!");
                return;
            }
            if (keymapDict == null)
            {
                keymapDict = new Dictionary<string, VRIME_Keymap>();
            }

            if (keymapDict.ContainsKey(key))
            {
                if (displays != null)
                {
                    var displayList = displays.Split('|').ToList();
                    var wordList = new List<Words>();

                    for (var i = 0; i < displayList.Count; i++)
                    {
                        wordList.Add(new Words
                        {
                            Syllable = CalculateSyllables(code),
                            Code = code,
                            Value = displayList[i]
                        });
                    }

                    if (keymapDict[key].Displays == null)
                    {
                        keymapDict[key].Displays = wordList;
                    }
                    else
                    {
                        keymapDict[key].Displays.AddRange(wordList);
                    }
                }

                if (candidates != null)
                {
                    var candidateList = candidates.Split('|').ToList();
                    var wordList = new List<Words>();

                    for (var i = 0; i < candidateList.Count; i++)
                    {
                        wordList.Add(new Words
                        {
                            Syllable = CalculateSyllables(code),
                            Code = code,
                            Value = candidateList[i]
                        });
                    }

                    if (keymapDict[key].Candidates == null)
                    {
                        keymapDict[key].Candidates = wordList;
                    }
                    else
                    {
                        keymapDict[key].Candidates.AddRange(wordList);
                    }
                }
            }
            else
            {
                VRIME_Keymap map = new VRIME_Keymap();
                if (displays != null)
                {
                    var displayList = displays.Split('|').ToList();
                    var wordList = new List<Words>();

                    for (var i = 0; i < displayList.Count; i++)
                    {
                        wordList.Add(new Words
                        {
                            Syllable = CalculateSyllables(code),
                            Code = code,
                            Value = displayList[i]
                        });
                    }

                    map.Displays = wordList;
                }
                if (candidates != null)
                {
                    var candidateList = candidates.Split('|').ToList();
                    var wordList = new List<Words>();

                    for (var i = 0; i < candidateList.Count; i++)
                    {
                        wordList.Add(new Words
                        {
                            Syllable = CalculateSyllables(code),
                            Code = code,
                            Value = candidateList[i]
                        });
                    }

                    map.Candidates = wordList;
                }

                keymapDict.Add(key, map);
            }
        }
        #endregion

        // for test
        private string convertAsciiToBPMF(string key) {
            key = key.Replace('1','ㄅ');
            key = key.Replace('q','ㄆ');
            key = key.Replace('a','ㄇ');
            key = key.Replace('z','ㄈ');
            key = key.Replace('2','ㄉ');
            key = key.Replace('w','ㄊ');
            key = key.Replace('s','ㄋ');
            key = key.Replace('x','ㄌ');
            key = key.Replace('3','ˇ');
            key = key.Replace('e','ㄍ');
            key = key.Replace('d','ㄎ');
            key = key.Replace('c','ㄏ');
            key = key.Replace('4','ˋ');
            key = key.Replace('r','ㄐ');
            key = key.Replace('f','ㄑ');
            key = key.Replace('v','ㄒ');
            key = key.Replace('5','ㄓ');
            key = key.Replace('t','ㄔ');
            key = key.Replace('g','ㄕ');
            key = key.Replace('b','ㄖ');
            key = key.Replace('6','ˊ');
            key = key.Replace('y','ㄗ');
            key = key.Replace('h','ㄘ');
            key = key.Replace('n','ㄙ');
            key = key.Replace('7','˙');
            key = key.Replace('u','ㄧ');
            key = key.Replace('j','ㄨ');
            key = key.Replace('m','ㄩ');
            key = key.Replace('8','ㄚ');
            key = key.Replace('i','ㄛ');
            key = key.Replace('k','ㄜ');
            key = key.Replace(',','ㄝ');
            key = key.Replace('9','ㄞ');
            key = key.Replace('o','ㄟ');
            key = key.Replace('l','ㄠ');
            key = key.Replace('.','ㄡ');
            key = key.Replace('0','ㄢ');
            key = key.Replace('p','ㄣ');
            key = key.Replace(';','ㄤ');
            key = key.Replace('/','ㄥ');
            key = key.Replace('-','ㄦ');
            return key;
        }
	}
}