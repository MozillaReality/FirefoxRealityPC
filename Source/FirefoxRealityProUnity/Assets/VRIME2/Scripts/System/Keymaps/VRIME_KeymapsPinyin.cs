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

	public class VRIME_KeymapsPinyin : VRIME_Keymaps
	{
        public VRIME_KeymapsPinyin()
        {
            LoadDataFromKeymapsTable();
            LoadDataFromAutoCorrectTable();
            AddDataNotInDatabase();
        }
		private static VRIME_KeymapsPinyin _instance;
        public static VRIME_KeymapsPinyin Instance {
            get {
                if (_instance == null) {
                    _instance = new VRIME_KeymapsPinyin();
                }
                return _instance;
            }
        }

        #region private field
        private const string debugTag = "keymaps_pinyin";
        private const string DatabasePath = "/google_pinyin.db";
        private const int MaxNearKeys = 4;
        private readonly char[] Vowels = { 'a', 'e', 'i', 'o', 'u' };
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
                            
                            if (HasDisplays(nearKey) && nearKeys.Count() < MaxNearKeys)
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

            var code = GetDisplayCode(key);
            var displayList = code.Split(' ').ToList();
            var candidateList = new List<Words>();
            var candidate = string.Empty;
            var syllable = 0;

            // Get the 1st candidate.
            syllable = displayList.Count();
            var tempKey = key;
            var remainKey = string.Empty;
            while (tempKey.Length > 0)
            {
                if (HasDisplays(tempKey))
                {
                    candidate += keymapDict[tempKey].Displays[0].Value;
                    tempKey = remainKey;
                    remainKey = string.Empty;
                }
                else
                {
                    remainKey = tempKey.Last() + remainKey;
                    tempKey = tempKey.Remove(tempKey.Length - 1);
                }
            }
            candidateList.Add(new Words
            {
                Syllable = syllable,
                Code = code,
                Value = candidate
            });

            // Get the other candidates
            tempKey = key;
            while (tempKey.Length > 0)
            {
                if (HasDisplays(tempKey))
                {
                    candidateList.AddRange(keymapDict[tempKey].Displays);
                }
                if (HasCandidates(tempKey))
                {
                    candidateList.AddRange(keymapDict[tempKey].Candidates);
                }
                tempKey = tempKey.Remove(tempKey.Length - 1);
            }

            if(candidateList.Count > 1) 
            {
                if(candidateList[0].Value.Equals(candidateList[1].Value))
                {
                    candidateList.RemoveAt(1);
                }
            }

            return candidateList;
        }
        public override List<Words> GetDisplays(string key)
        {
            return base.GetDisplays(key);
        }
        #endregion
        #region private function
        private void LoadDataFromKeymapsTable()
        {
            connectionStr = "URI=File:" + Application.streamingAssetsPath + DatabasePath;
            databaseConnection = new SqliteConnection(connectionStr);
            databaseConnection.Open();
            databaseCommand = databaseConnection.CreateCommand();
            databaseCommand.CommandText = "SELECT * FROM keymaps ORDER BY _id ASC";
            dataReader = databaseCommand.ExecuteReader();

            while (dataReader.Read())
            {
                var key = dataReader.IsDBNull(1) ? null : dataReader.GetString(1);

                AddWordsIntoKeymapDict(
                    key,
                    key,
                    dataReader.IsDBNull(2) ? null : dataReader.GetString(2),
                    dataReader.IsDBNull(3) ? null : dataReader.GetString(3));
            }

            databaseConnection.Close();
        }

        private void LoadDataFromAutoCorrectTable()
        {
            connectionStr = "URI=File:" + Application.streamingAssetsPath + DatabasePath;
            databaseConnection = new SqliteConnection(connectionStr);
            databaseConnection.Open();
            databaseCommand = databaseConnection.CreateCommand();
            databaseCommand.CommandText = "SELECT * FROM autocorrect ORDER BY _id ASC";
            dataReader = databaseCommand.ExecuteReader();

            while (dataReader.Read())
            {
                var key = dataReader.IsDBNull(1) ? null : dataReader.GetString(1);

                AddWordsIntoKeymapDict(
                    key,
                    dataReader.IsDBNull(2) ? key : dataReader.GetString(2),
                    dataReader.IsDBNull(3) ? null : dataReader.GetString(3));
            }

            databaseConnection.Close();
        }

        private void AddDataNotInDatabase()
        {
            AddWordsIntoKeymapDict("a", "a", "a|A");
            AddWordsIntoKeymapDict("b", "b", "b|B");
            AddWordsIntoKeymapDict("c", "c", "c|C");
            AddWordsIntoKeymapDict("d", "d", "d|D");
            AddWordsIntoKeymapDict("e", "e", "e|E");
            AddWordsIntoKeymapDict("f", "f", "f|F");
            AddWordsIntoKeymapDict("g", "g", "g|G");
            AddWordsIntoKeymapDict("h", "h", "h|H");
            AddWordsIntoKeymapDict("i", "i", "i|I", "喔|哦|噢");
            AddWordsIntoKeymapDict("j", "j", "j|J");
            AddWordsIntoKeymapDict("k", "k", "k|K");
            AddWordsIntoKeymapDict("l", "l", "l|L");
            AddWordsIntoKeymapDict("m", "m", "m|M");
            AddWordsIntoKeymapDict("n", "n", "n|N");
            AddWordsIntoKeymapDict("o", "o", "o|O");
            AddWordsIntoKeymapDict("p", "p", "p|P");
            AddWordsIntoKeymapDict("q", "q", "q|Q");
            AddWordsIntoKeymapDict("r", "r", "r|R");
            AddWordsIntoKeymapDict("s", "s", "s|S");
            AddWordsIntoKeymapDict("t", "t", "t|T");
            AddWordsIntoKeymapDict("u", "u", "u|U", "有|要");
            AddWordsIntoKeymapDict("v", "v", "v|V", "吧|被");
            AddWordsIntoKeymapDict("w", "w", "w|W");
            AddWordsIntoKeymapDict("x", "x", "x|X");
            AddWordsIntoKeymapDict("y", "y", "y|Y");
            AddWordsIntoKeymapDict("z", "z", "z|Z");

            if (nearAlphabetDict == null)
            {
                nearAlphabetDict = new Dictionary<char, char[]>();
            }
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
            nearAlphabetDict.Add('q', new char[] { 'w', 'a'           });
            nearAlphabetDict.Add('w', new char[] { 'e', 's', 'a', 'q' });
            nearAlphabetDict.Add('e', new char[] { 'r', 'd', 's', 'w' });
            nearAlphabetDict.Add('r', new char[] { 't', 'f', 'd', 'e' });
            nearAlphabetDict.Add('t', new char[] { 'y', 'g', 'f', 'r' });
            nearAlphabetDict.Add('y', new char[] { 'u', 'h', 'g', 't' });
            nearAlphabetDict.Add('u', new char[] { 'i', 'j', 'h', 'y' });
            nearAlphabetDict.Add('i', new char[] { 'o', 'k', 'j', 'u' });
            nearAlphabetDict.Add('o', new char[] { 'p', 'l', 'k', 'i' });
            nearAlphabetDict.Add('p', new char[] {           'l', 'o' });

            // second line
            nearAlphabetDict.Add('a', new char[] { 's', 'z',           'q', 'w' });
            nearAlphabetDict.Add('s', new char[] { 'd', 'x', 'z', 'a', 'w', 'e' });
            nearAlphabetDict.Add('d', new char[] { 'f', 'c', 'x', 's', 'e', 'r' });
            nearAlphabetDict.Add('f', new char[] { 'g', 'v', 'c', 'd', 'r', 't' });
            nearAlphabetDict.Add('g', new char[] { 'h', 'b', 'v', 'f', 't', 'y' });
            nearAlphabetDict.Add('h', new char[] { 'j', 'n', 'b', 'g', 'y', 'u' });
            nearAlphabetDict.Add('j', new char[] { 'k', 'm', 'n', 'h', 'u', 'i' });
            nearAlphabetDict.Add('k', new char[] { 'l',      'm', 'j', 'i', 'o' });
            nearAlphabetDict.Add('l', new char[] {                'k', 'o', 'p' });

            // third line
            nearAlphabetDict.Add('z', new char[] { 'x',      'a', 's' });
            nearAlphabetDict.Add('x', new char[] { 'c', 'z', 's', 'd' });
            nearAlphabetDict.Add('c', new char[] { 'v', 'x', 'd', 'f' });
            nearAlphabetDict.Add('v', new char[] { 'b', 'c', 'f', 'g' });
            nearAlphabetDict.Add('b', new char[] { 'n', 'v', 'g', 'h' });
            nearAlphabetDict.Add('n', new char[] { 'm', 'b', 'h', 'j' });
            nearAlphabetDict.Add('m', new char[] {      'n', 'j', 'k' });
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
	}
}