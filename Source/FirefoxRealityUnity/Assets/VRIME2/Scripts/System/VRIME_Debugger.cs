// ========================================================================== //
//  Created: 2019-01-09
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class VRIME_Debugger 
	{
		#region value Field
		private const string ProjectName = "VRIME2";
		private const string DefaultLog = "VRIME2 LOG";
		private const string DefaultWarning = "VRIME2 WARNING";
		private const string DefaultError = "VRIME2 ERROR";
		private static ILogger logger = Debug.unityLogger;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		private static bool DebugShow = true;
#else
		private static bool DebugShow = false;
#endif
		#endregion

		#region log function
		public static void Log(GameObject iWhoCall, object iMessage) { Log(iWhoCall.name, iMessage); }
		public static void Log(object iMessage) { Log(DefaultLog, iMessage); }
		public static void Log(string iCallName, object iMessage)
		{
			if(DebugShow == false)
				return;
			
			string aMessageTag = iCallName;
			if(iCallName != DefaultLog)
				aMessageTag = GetTagName(iCallName);
			
			logger.Log(aMessageTag, iMessage);
		}
		#endregion
		#region log warning function
		public static void LogWarning(GameObject iWhoCall, object iMessage) { LogWarning(iWhoCall.name, iMessage); }
		public static void LogWarning(object iMessage) { LogWarning(DefaultWarning, iMessage); }
		public static void LogWarning(string iCallName, object iMessage)
		{
			if(DebugShow == false)
				return;
				
			string aMessageTag = iCallName;
			if(iCallName != DefaultWarning)
				aMessageTag = GetTagName(iCallName);

			logger.LogWarning(aMessageTag, iMessage);
		}
		public static void LogWarningFormat(string iFormat, params object[] iArgs)
		{
			string aResult = string.Format(iFormat, iArgs);
			LogWarning(aResult);
		}
		#endregion

		#region log error function
		public static void LogError(GameObject iWhoCall, object iMessage) { LogError(iWhoCall.name, iMessage); }
		public static void LogError(object iMessage) { LogError(DefaultError, iMessage); }
		public static void LogError(string iCallName, object iMessage)
        {
			if(DebugShow == false)
				return;
			
			string aMessageTag = iCallName;
			if(iCallName != DefaultError)
				aMessageTag = GetTagName(iCallName);

			logger.LogError(aMessageTag, iMessage);
        }
		#endregion
		#region common function
		private static string GetTagName(string iObjName)
        {
            return ProjectName + "_" + iObjName;
        }
		#endregion
	}
}