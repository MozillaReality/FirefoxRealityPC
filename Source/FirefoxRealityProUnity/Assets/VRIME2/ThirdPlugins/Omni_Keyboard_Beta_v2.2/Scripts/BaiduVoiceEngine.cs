using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Htc.Omni
{
    public class BaiduVoiceEngineBase : VoiceEngineBase
    {
        private string apiKey;
        private string secret;

        private string baidu_token;

        private const string TOKEN_KEY = "baidu_token";
        private const string KEY_SERECT_HASH = "baidu_key_serect_hash";

        public override string VoiceEngineName() {  return "baidu"; }

        private keyboardAudioInput keyboardAudioInput;

        public BaiduVoiceEngineBase(string baiduApiKey, string baiduSecret, keyboardAudioInput keyboardAudioInput)
        {
            this.keyboardAudioInput = keyboardAudioInput;
            if(string.IsNullOrEmpty(baiduApiKey) || string.IsNullOrEmpty(baiduSecret))
            {
                VRIME2.VRIME_Debugger.LogError("not correct baidu api key or secret");
                return;
            }

            apiKey = baiduApiKey;
            secret = baiduSecret;

            string currentKeyHash = PlayerPrefs.GetString(KEY_SERECT_HASH, "");
            baidu_token = PlayerPrefs.GetString(TOKEN_KEY, "");
            
            string keyHash = getMD5(apiKey + secret);
		    // VRIME2.VRIME_Debugger.Log("get token from prefs: " + baidu_token);

            if(String.IsNullOrEmpty(baidu_token) || string.IsNullOrEmpty(currentKeyHash) || !currentKeyHash.Equals(keyHash))
            {
                new Thread( () => {
                    baidu_token = getBaiduToken();
                    // VRIME2.VRIME_Debugger.Log("get token from server: " + baidu_token);
                }).Start();
            }
        }

        private string getBaiduToken()
        {
            string token = "";
            string baidu_token_url = "https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials&client_id=" + apiKey + "&client_secret=" + secret;
            // VRIME2.VRIME_Debugger.Log("get baidu token: " + baidu_token_url);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(baidu_token_url);
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    /**
                    example:
    {
        "access_token": "24.4bb4ff5309c5a1720aeeaec912e011a6.2592000.1549684134.282335-15382757",
        "session_key": "9mzdD0bcXIchm2BocWoKOCaNaDIxRCjuoN1pmT9EDVTI3PHPSKoo299iRiF89JS2LAX8CcYb6P2xuJJdVdihnO/OTCzY+Q==",
        "scope": "audio_voice_assistant_get audio_tts_post public brain_all_scope wise_adapt lebo_resource_base lightservice_public hetu_basic lightcms_map_poi kaidian_kaidian ApsMisTest_Test权限 vis-classify_flower lpq_开放 cop_helloScope ApsMis_fangdi_permission smartapp_snsapi_base iop_autocar oauth_tp_app smartapp_smart_game_openapi oauth_sessionkey smartapp_swanid_verify smartapp_opensource_openapi",        
        "refresh_token": "25.ac4675df45a5e7af444cba6851dee6ab.315360000.1862452134.282335-15382757",
        "session_secret": "8b2ed8cdf2b91c8bc2c08c173ea68f43",
        "expires_in": 2592000
    }				
                    */

                    var result = streamReader.ReadToEnd();
                    // VRIME2.VRIME_Debugger.Log("Response:" + result);
                    var jsonresponse = SimpleJSON.JSON.Parse(result);
                    if (jsonresponse != null)
                    {
                        token = jsonresponse["access_token"].Value;
                    }
                    if(!String.IsNullOrEmpty(token))
                    {                         
                        keyboardAudioInput.queueBaiduTokenToSave(token);
                        // PlayerPrefs.SetString(TOKEN_KEY, baidu_token);
                        // string keyHash = getMD5(apiKey + secret);
                        // PlayerPrefs.SetString(KEY_SERECT_HASH, keyHash);
                        // PlayerPrefs.Save();
                    }

                    return token;
                }
        }

        public void saveBaiduToken(string token) 
        {
            PlayerPrefs.SetString(TOKEN_KEY, baidu_token);
            string keyHash = getMD5(apiKey + secret);
            PlayerPrefs.SetString(KEY_SERECT_HASH, keyHash);
            PlayerPrefs.Save();            
        }

        private string getMD5(string str)
        {
            using (var cryptoMD5 = System.Security.Cryptography.MD5.Create())
            {
                //將字串編碼成 UTF8 位元組陣列
                var bytes = Encoding.UTF8.GetBytes(str);

                //取得雜湊值位元組陣列
                var hash = cryptoMD5.ComputeHash(bytes);

                //取得 MD5
                var md5 = BitConverter.ToString(hash)
                .Replace("-", String.Empty)
                .ToUpper();

                return md5;
            }
        }

        public override string GetVoiceStringByFile(string filePath, out string error, SupportLanguage language, out bool httpError)
        {
            httpError = false;
            VRIME2.VRIME_Debugger.Log("baidu GetVoiceStringByFile");
            int retryCount = 0;
            const int max_retry = 2;
            bool retry = true;
            // VRIME2.VRIME_Debugger.Log("Saving @ " + filePath);
            string transcripts = "";
            error = "";
            while(retry && retryCount < max_retry)
            {
                if(String.IsNullOrEmpty(baidu_token))
                {
                    baidu_token = getBaiduToken();
                }

                int devPid = 1537; // chinese					
                if(language == SupportLanguage.English) 
                {
                    devPid = 1737; // english
                } 

                // // test for random wrong token
                // if(DateTime.Now.Second %2 == 1)
                // {
                //     VRIME2.VRIME_Debugger.Log("try to build fail token");
                //     baidu_token += "a";
                // }

                string apiURL = "https://vop.baidu.com/server_api?dev_pid=" + devPid + "&token=" + baidu_token + "&cuid=1";
                string Response;


                // VRIME2.VRIME_Debugger.Log("Uploading " + filePath);            
                Response = HttpUploadFile(apiURL, filePath, "file", "audio/wav; rate=16000", language, out httpError);            
                
                                    
                // VRIME2.VRIME_Debugger.Log("Response String: " + Response);

                var jsonresponse = SimpleJSON.JSON.Parse(Response);


                transcripts = "";
                if (jsonresponse != null)
                {
                    // string resultString = jsonresponse ["result"] [0].Value;
                    // VRIME2.VRIME_Debugger.Log ("resultString: " + resultString );
                    // var jsonResults = SimpleJSON.JSON.Parse (resultString);

                    transcripts = jsonresponse["result"][0].Value;            
                    // VRIME2.VRIME_Debugger.Log("transcript string: " + transcripts);

                    string errorCode = jsonresponse["err_no"].Value;
                    if("3302".Equals(errorCode))
                    {
                        VRIME2.VRIME_Debugger.Log("baidu token is not valid, try renew");
                        baidu_token = null;
                    }                    
                }

                if(String.IsNullOrEmpty(transcripts))
                {
                    error = Response;
                } else 
                {
                    break;
                }
                retryCount++;
            }

            
            return transcripts;
        }
    

        public string HttpUploadFile(string url, string file, string paramName, string contentType, SupportLanguage language, out bool httpError) 
        {
            httpError = false;
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
            // VRIME2.VRIME_Debugger.Log(string.Format("Uploading {0} to {1}", file, url));

            Byte[] bytes = File.ReadAllBytes(file);
            

            try
            {

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = contentType;                
                httpWebRequest.Method = "POST";
                Stream dataStream = httpWebRequest.GetRequestStream();
                dataStream.Write(bytes, 0, bytes.Length);
                dataStream.Close();

                /**
                Response sample:
{
    "corpus_no": "6644710189556046247",
    "err_msg": "success.",
    "err_no": 0,
    "result": [
        "北京科技馆"
    ],
    "sn": "444076691981547092150"
}		
                */

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                // VRIME2.VRIME_Debugger.Log(httpResponse);
                
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    // VRIME2.VRIME_Debugger.Log("Response:" + result);
                    return result;
                }
            
            } catch (WebException ex) {
                httpError = true;
                var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                VRIME2.VRIME_Debugger.Log(resp);    
            }

            return "empty";		
        }    
    }
}