using System.Text;
using UnityEngine;

namespace VRIME2
{    
    public partial class VRIME_BISender
	{
        private const string CATEGORY_VOICE_INPUT = "voice_input";
        private const string KEY_VOICE_SESSION_ID = "flow_session_id";
        private const string EVENT_VOICE_START = "start";
        private const string EVENT_VOICE_RESULT = "result";
        private const string EVENT_VOICE_ERROR = "error";
        private const string EVENT_VOICE_SKIP = "skip";
        private static Encoder encoder = Encoding.Unicode.GetEncoder();

        private const string CATEGORY_VOICE_USER_ACTION = "user_action";
        private const string EVENT_VOICE_OPEN = "voice_open";
        private const string EVENT_VOICE_CLOSE = "voice_close";
        private const string EVENT_VOICE_CANCEL = "voice_cancel";
        private const string EVENT_VOICE_AUTO_CLOSE = "voice_auto_close";

        public const string VOICE_ERROR_TYPE_HTTP = "http";
        public const string VOICE_ERROR_TYPE_JSON = "json";

        private string mVoiceSessionId;
        

        public void CallVoiceStart() 
        {
            BILogInit();
			using(new BILogScope(CATEGORY_VOICE_INPUT)) {
				BILogScope.BI.AddData(KEY_EVENT, EVENT_VOICE_START);				
                AddDataVoiceCommon();
				AddDataCommon();
			}
			VRIME_Debugger.Log("BI CallVoiceStart Logged. voice SessionID :" + mVoiceSessionId);
        }

        /// <summary>
        /// Send BI when voice input result
        /// </summary>
        /// <param name="reulst">voice result string</param>
        /// <param name="duration">voice length (ms)</param>
        /// <param name="serverResponseTime">voice server response time (ms)</param>
        public void CallVoiceResult(string result, int duration, int serverResponseTime) 
        {
            BILogInit();
			using(new BILogScope(CATEGORY_VOICE_INPUT)) {
				BILogScope.BI.AddData(KEY_EVENT, EVENT_VOICE_RESULT);
                BILogScope.BI.AddData(KEY_A2, duration);
                BILogScope.BI.AddData(KEY_A3, result == null ? 0 : result.Replace(" ", "").Length);                
                BILogScope.BI.AddData(KEY_A4, serverResponseTime);
                AddDataVoiceCommon();
				AddDataCommon();
			}
			VRIME_Debugger.Log("BI CallVoiceResult Logged. voice SessionID :" + mVoiceSessionId);            
        }        

        public void CallVoiceError(int duration, int serverResponseTime, string errorType, string errorMessage) 
        {
                        
            BILogInit();
			using(new BILogScope(CATEGORY_VOICE_INPUT)) {
				BILogScope.BI.AddData(KEY_EVENT, EVENT_VOICE_ERROR);
                BILogScope.BI.AddData(KEY_A2, duration);
                BILogScope.BI.AddData(KEY_A4, serverResponseTime);
                BILogScope.BI.AddData(KEY_A5, errorType);
                BILogScope.BI.AddData(KEY_A6, errorMessage);
                AddDataVoiceCommon();
				AddDataCommon();
			}
			VRIME_Debugger.Log("BI CallVoiceError Logged. voice SessionID :" + mVoiceSessionId);            
        }        

        public void CallVoiceSkip(int duration) 
        {
            BILogInit();
			using(new BILogScope(CATEGORY_VOICE_INPUT)) {
				BILogScope.BI.AddData(KEY_EVENT, EVENT_VOICE_SKIP);
                BILogScope.BI.AddData(KEY_A2, duration);
                AddDataVoiceCommon();
				AddDataCommon();
			}
			VRIME_Debugger.Log("BI CallVoiceSkip Logged. voice SessionID :" + mVoiceSessionId);                        
        }

        public void CallVoiceOpen()
        {
            mVoiceSessionId = System.Guid.NewGuid().ToString();
#if VOICEINPUT_LOGUI
            VRIME_VoiceInputDebug.BISessionID = mVoiceSessionId;
#endif
            BILogInit();
			using(new BILogScope(CATEGORY_VOICE_USER_ACTION)) {
				BILogScope.BI.AddData(KEY_A0, EVENT_VOICE_OPEN);
                AddDataVoiceUserActionCommon();
				AddDataCommon();
			}
			VRIME_Debugger.Log("BI CallVoiceOpen Logged. voice SessionID :" + mVoiceSessionId);                  
        }

        public void CallVoiceClose()
        {            
            BILogInit();
			using(new BILogScope(CATEGORY_VOICE_USER_ACTION)) {
				BILogScope.BI.AddData(KEY_A0, EVENT_VOICE_CLOSE);
                AddDataVoiceUserActionCommon();
				AddDataCommon();
			}
			VRIME_Debugger.Log("BI CallVoiceClose Logged. voice SessionID :" + mVoiceSessionId);                  
        }   

        public void CallVoiceAutoClose()
        {            
            BILogInit();
			using(new BILogScope(CATEGORY_VOICE_USER_ACTION)) {
				BILogScope.BI.AddData(KEY_A0, EVENT_VOICE_AUTO_CLOSE);
                AddDataVoiceUserActionCommon();
				AddDataCommon();
			}
			VRIME_Debugger.Log("BI CallVoiceAutoClose Logged. voice SessionID :" + mVoiceSessionId);                  
        }   

        public void CallVoiceCancel()
        {            
            BILogInit();
			using(new BILogScope(CATEGORY_VOICE_USER_ACTION)) {
				BILogScope.BI.AddData(KEY_A0, EVENT_VOICE_CANCEL);
                AddDataVoiceUserActionCommon();
				AddDataCommon();
			}
			VRIME_Debugger.Log("BI CallVoiceCancel Logged. voice SessionID :" + mVoiceSessionId);                  
        }         

        public static void calucateCharCount(string str, out int englishCount, out int nonEnglishCount)
        {
            englishCount = 0;
            nonEnglishCount = 0;            
            VRIME_Debugger.Log("encoder name: " + Encoding.Default.EncodingName);
            
            char[] array = str.Replace(" ", "").ToCharArray();
            for(int i = 0; i < array.Length; i++) 
            {
                int bytesCount = encoder.GetByteCount(array, i, 1, true);
                VRIME_Debugger.Log("char: " + array[i] + ", bytesCount: " + bytesCount);
                if(bytesCount > 1) 
                {
                    nonEnglishCount++;
                } else {
                    englishCount++;
                }
            }
        }

        private void AddDataVoiceCommon()
        {
            BILogScope.BI.AddData(KEY_A0, keyboardAudioInput.VoiceEngineName);
            BILogScope.BI.AddData(KEY_A1, keyboardAudioInput.SpeechLanguage.ToString());
            BILogScope.BI.AddData(KEY_VOICE_SESSION_ID, mVoiceSessionId);
        }

        private void AddDataVoiceUserActionCommon()
        {
            BILogScope.BI.AddData(KEY_EVENT, ACTION_EV_EVENT);
            BILogScope.BI.AddData(KEY_VOICE_SESSION_ID, mVoiceSessionId);
        }
    }
}    