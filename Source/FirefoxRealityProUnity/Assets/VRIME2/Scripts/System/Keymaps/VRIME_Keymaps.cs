// ========================================================================== //
//  Created: 2019-01-14
// ========================================================================== //
//  Copy From VRIME Ver 1
//  Copyright 2018 HTC Xindian HTC1
// ========================================================================== //
namespace VRIME2
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public interface IKeymaps
    {
        List<Words> GetDisplays(string key);
        List<Words> GetCandidates(string key);
        string GetDisplayCode(string key);
        List<string> GetNearKeys(string key);
    }


    public abstract class VRIME_Keymaps : IKeymaps
    {
		#region protected field
		protected Dictionary<string, VRIME_Keymap> keymapDict { get; set; }
		#endregion
		#region Practice interface function
        public virtual List<Words> GetDisplays(string key)
        {
			if (string.IsNullOrEmpty(key))
            {
                VRIME_Debugger.LogError("Keymaps", "Key is null or empty!");
                return null;
            }
            if (key.Replace(" ", string.Empty) == string.Empty)
            {
                VRIME_Debugger.LogError("Keymaps", "Key is whitespace!");
                return null;
            }

            return keymapDict.ContainsKey(key) ? keymapDict[key].Displays : null;
        }
        public virtual List<Words> GetCandidates(string key) { throw new NotImplementedException(); }
        public virtual string GetDisplayCode(string key) { throw new NotImplementedException(); }
        public virtual List<string> GetNearKeys(string key) { throw new NotImplementedException(); }
		#endregion
		#region public function
		public bool HasKeymap(string key)
        {
            if (keymapDict == null || string.IsNullOrEmpty(key))
            {
                return false;
            }
            if (key.Replace(" ", string.Empty) == string.Empty)
            {
                return false;
            }

            return keymapDict.ContainsKey(key);
        }
		public bool HasDisplays(string key)
        {
            if (!HasKeymap(key))
            {
                return false;
            }
            if (keymapDict[key].Displays == null)
            {
                return false;
            }
            if (keymapDict[key].Displays.Count == 0)
            {
                return false;
            }
            return true;
        }

        public bool HasCandidates(string key)
        {
            if (!HasKeymap(key))
            {
                return false;
            }
            if (keymapDict[key].Candidates == null)
            {
                return false;
            }
            if (keymapDict[key].Candidates.Count == 0)
            {
                return false;
            }
            return true;
        }
		public int CalculateSyllables(string displayCode)
        {
            if (string.IsNullOrEmpty(displayCode))
            {
                return 0;
            }
            if (displayCode.Replace(" ", string.Empty) == string.Empty)
            {
                return 0;
            }

            return displayCode.Split(' ').Length;
        }
        /// <summary>
        /// Change accent letter keymapDict.
        /// </summary>
        /// <param name="iSys"></param>
        /// <returns></returns>
        public bool ChangeAccentKeymapDict(VRIME_LanguageSys iSys)
        {
            if(iSys == null)
                return false;
            if(iSys.accentShow == false)
                return false;
            
            Dictionary<string, VRIME_Keymap> aNewKeymap =  new Dictionary<string, VRIME_Keymap>();
            // Language
            VRIME_KeypadConfig aConfig = iSys.GetConfigData(VRIME_KeyboardSetting.IMELanguage);
            for(int i = 0; i < aConfig.rowDatas.Length; i++)
            {
                VRIME_KeypadConfigRowData aRowData = aConfig.rowDatas[i];
                for(int k = 0; k < aRowData.objDatas.Length; k++)
                {
                    VRIME_KeypadConfigData aConfigData = aRowData.objDatas[k];
                    // Lower
                    AddNewKeymapDictItem(aConfigData.word.ToLower(), aConfigData.lowerAccentWords, ref aNewKeymap);
                    // Upper
                    AddNewKeymapDictItem(aConfigData.word.ToUpper(), aConfigData.upperAccentWords, ref aNewKeymap);
                }
            }
            // override using keymap
            if(aNewKeymap.Count > 0) {
                keymapDict = aNewKeymap;
            }
            else {
                return false;
            }
            return true;
        }
        private void AddNewKeymapDictItem(string iKey, string[] iValues, ref Dictionary<string, VRIME_Keymap> iNewKeymap)
        {
            // Add New List
            List<Words> aWordList = new List<Words>();
            for(int i = 0; i < iValues.Length; i++) {
                aWordList.Add(new Words { Syllable = 1, Code = iKey, Value = iValues[i] });
            }
            // add keymap by ref
            if(aWordList.Count > 0 && iNewKeymap.ContainsKey(iKey) == false) {
                iNewKeymap.Add(iKey, new VRIME_Keymap { Displays = aWordList });
            }
        }
        #endregion
    }

    public class VRIME_Keymap
    {
        public List<Words> Displays { get; set; }
        public List<Words> Candidates { get; set; }
    }

    public class Words
    {
        public int Syllable { get; set; }           // The "a di da si" has 4 syllables.
        public string Code { get; set; }            // "a di da si"
        public string Value { get; set; }           // "阿迪达斯"

        public Words Clone() {
            return new Words()
            {
                            Syllable = this.Syllable,
                            Code = this.Code,
                            Value = this.Value
            };
        }
    }
}