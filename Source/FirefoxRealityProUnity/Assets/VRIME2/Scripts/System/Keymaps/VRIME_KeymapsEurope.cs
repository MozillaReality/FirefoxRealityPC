// ========================================================================== //
//  Created: 2019-01-14
// ========================================================================== //
//  Copy From VRIME Ver 1
//  Copyright 2018 HTC Xindian HTC1
// ========================================================================== //
namespace VRIME2
{
	using System.Collections.Generic;
	using System.Linq;

	public class VRIME_KeymapsEurope : VRIME_Keymaps
	{
		// public VRIME_KeymapsEurope()
		// {
		// 	SetupDict();
		// }
        private static VRIME_KeymapsEurope _instance;
        public static VRIME_KeymapsEurope Instance {
            get {
                if (_instance == null) {
                    _instance = new VRIME_KeymapsEurope();
                }
                return _instance;
            }
        }
		#region private function
		// private void SetupDict()
        // {
        //     if (keymapDict == null)
        //     {
        //         keymapDict = new Dictionary<string, VRIME_Keymap>();
        //     }

        //     AddAlphabetIntoKeymapDict("a", "á|à|â|ã|ä|å|ā|æ");
        //     AddAlphabetIntoKeymapDict("A", "Á|À|Â|Ã|Ä|Å|Ā|Æ");
        //     AddAlphabetIntoKeymapDict("c", "ć|č|ç");
        //     AddAlphabetIntoKeymapDict("C", "Ć|Č|Ç");
        //     AddAlphabetIntoKeymapDict("d", "ď");
        //     AddAlphabetIntoKeymapDict("D", "Ď");
        //     AddAlphabetIntoKeymapDict("e", "é|è|ê|ė|ë|ē|ę|ě");
        //     AddAlphabetIntoKeymapDict("E", "É|È|Ê|Ė|Ë|Ē|Ę|Ě");
        //     AddAlphabetIntoKeymapDict("i", "í|ì|î|ï|į|ī|ǐ");
        //     AddAlphabetIntoKeymapDict("I", "Í|Ì|Î|Ï|Į|Ī|Ǐ");
        //     AddAlphabetIntoKeymapDict("n", "ń|ñ|ň");
        //     AddAlphabetIntoKeymapDict("N", "Ń|Ñ|Ň");
        //     AddAlphabetIntoKeymapDict("o", "ó|ò|ô|ö|õ|ø|ō|œ");
        //     AddAlphabetIntoKeymapDict("O", "Ó|Ò|Ô|Ö|Õ|Ø|Ō|Œ");
        //     AddAlphabetIntoKeymapDict("r", "ř");
        //     AddAlphabetIntoKeymapDict("R", "Ř");
        //     AddAlphabetIntoKeymapDict("s", "ß|š");
        //     AddAlphabetIntoKeymapDict("S", "ẞ|Š");
        //     AddAlphabetIntoKeymapDict("t", "ť");
        //     AddAlphabetIntoKeymapDict("T", "Ť");
        //     AddAlphabetIntoKeymapDict("u", "ú|ù|û|ü|ū|ů");
        //     AddAlphabetIntoKeymapDict("U", "Ú|Ù|Û|Ü|Ū|Ů");
        //     AddAlphabetIntoKeymapDict("y", "ÿ");
        //     AddAlphabetIntoKeymapDict("Y", "Ÿ");
        //     AddAlphabetIntoKeymapDict("z", "ž");
        //     AddAlphabetIntoKeymapDict("Z", "Ž");
        //     AddAlphabetIntoKeymapDict("?", "¿");
        //     AddAlphabetIntoKeymapDict("!", "¡");
        // }

		// private void AddAlphabetIntoKeymapDict(string key, string alphabets)
        // {
        //     var alphabetList = alphabets.Split('|').ToList();
        //     var wordsList = new List<Words>();

        //     for (var i = 0; i < alphabetList.Count; i++)
        //     {
        //         wordsList.Add(new Words { Syllable = 1, Code = key, Value = alphabetList[i] });
        //     }

        //     keymapDict.Add(key, new VRIME_Keymap { Displays = wordsList });
        // }

        
		#endregion
	}
}