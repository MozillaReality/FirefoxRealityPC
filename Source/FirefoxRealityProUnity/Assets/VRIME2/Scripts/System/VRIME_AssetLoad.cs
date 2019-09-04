// ========================================================================== //
//  Created: 2019-01-11
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

	public class VRIME_AssetLoad
	{

		public static GameObject GetGameObjectResource(string iResPath)
		{
			GameObject aResult = null;
			var aGamePrefab = Resources.Load(iResPath) as GameObject;
			if(aGamePrefab != null) {
				aResult = GameObject.Instantiate(aGamePrefab);
			}
			return aResult;
		}

		public static AudioClip GetAudioClipResource(string iResPath)
		{
			AudioClip aResult = Resources.Load(iResPath, typeof(AudioClip)) as AudioClip;
			return aResult;
		}
		/// <summary>
		/// Json file load string and change to KeypadConfig
		/// </summary>
		/// <param name="iFileName"></param>
		/// <returns></returns>
		public static string LoadKeypadConfig(string iJsonFileName, eConfigFolder iFolderType)
		{
			string aSavePath = Path.Combine(Application.streamingAssetsPath, iFolderType.ToString());
			string aFilePath = Path.Combine(aSavePath, iJsonFileName + ".json");
			// Load File
			FileInfo aFI = new FileInfo(aFilePath);
			if(!aFI.Exists)
			{
				Debug.Log("File Not Found, Path :" + aFilePath);
				return null;
			}

			FileStream aFS = new FileStream(aFilePath, FileMode.Open, FileAccess.Read);
			StreamReader aSR = new StreamReader(aFS, Encoding.GetEncoding("UTF-8"));
			
			string aJsonText = aSR.ReadToEnd();
			aSR.Close();
			aFS.Close();
			// Json text to config class
			return aJsonText;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="iFolderType"></param>
		/// <returns></returns>
		public static string[] LoadKeypadFileNameList(eConfigFolder iFolderType)
		{
			string aSavePath = Path.Combine(Application.streamingAssetsPath, iFolderType.ToString());
			string[] aFileList =  Directory.GetFiles(aSavePath);
			List<string> aResult = new List<string>();
			for(int i = 0; i < aFileList.Length; i++)
			{
				string aFileFullName = aFileList[i].Remove(0, (aSavePath.Length + 1));
				string[] aSplitFullName = aFileFullName.Split('.');
				if(aSplitFullName.Length >= 3)
					continue;
				aResult.Add(aSplitFullName[0]);
			}

			return aResult.ToArray();
		}
	}
}