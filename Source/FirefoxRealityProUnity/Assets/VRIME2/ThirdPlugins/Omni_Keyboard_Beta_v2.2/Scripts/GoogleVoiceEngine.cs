using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using UnityEngine;

namespace Htc.Omni
{
    public class GoogleVoiceEngineBase : VoiceEngineBase
    {
        private string apiKey;

        public GoogleVoiceEngineBase(string googleApiKey)
        {
            apiKey = googleApiKey;
        }

        public override string VoiceEngineName() {  return "google"; }

        public override string GetVoiceStringByFile(string filePath, out string error, SupportLanguage language, out bool httpError)
        {
            Debug.Log("Saving @ " + filePath);
            //Insert your API KEY here.
            string apiURL = "https://speech.googleapis.com/v1/speech:recognize?&key=" + apiKey;
            string Response;

            Debug.Log("Uploading " + filePath);            
            Response = HttpUploadFile(apiURL, filePath, "file", language, out httpError);            
            
                                
            Debug.Log("Response String: " + Response);

            var jsonresponse = SimpleJSON.JSON.Parse(Response);

            string transcripts = "";
            error = "";

            if (jsonresponse != null)
            {
                // string resultString = jsonresponse ["result"] [0].Value;
                // Debug.Log ("resultString: " + resultString );
                // var jsonResults = SimpleJSON.JSON.Parse (resultString);

                var jsonResults = jsonresponse["results"][0];

                transcripts = jsonResults["alternatives"][0]["transcript"].Value;

                Debug.Log("transcript string: " + transcripts);
            }

            if(String.IsNullOrEmpty(transcripts))
            {
                error = Response;
            }

            
            return transcripts;
        }
    

        public string HttpUploadFile(string url, string file, string contentType, SupportLanguage language, out bool httpError) 
        {
            httpError = false;
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
            Debug.Log(string.Format("Uploading {0} to {1}", file, url));

            Byte[] bytes = File.ReadAllBytes(file);
            String file64 = Convert.ToBase64String(bytes,
                                            Base64FormattingOptions.None);
            
            

            try
            {

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add("Content-Encoding", "gzip");
                httpWebRequest.Method = "POST";

                string locale = "zh-CN";
                if(language == SupportLanguage.English) 
                {
                    locale = "en-US";
                } else if(language == SupportLanguage.TraditionalChinese)  {
                    locale = "zh-TW";
                }

                using (var gzipStream = new GZipStream(httpWebRequest.GetRequestStream(), CompressionMode.Compress))
                {
                    
                    using (var streamWriter = new StreamWriter(gzipStream))
                    {
                        string json = "{ \"config\": { \"languageCode\" : \""+ locale + "\""
                        //+ ", \"maxAlternatives\":3 "
                         + ", \"profanityFilter\":true "
                        + "}, \"audio\" : { \"content\" : \"" + file64 + "\"}}";

                        Debug.Log(json);
                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }
                /**
                Response sample:
    {
    "results": [
        {
        "alternatives": [
            {
            "transcript": "你今天好嗎",
            "confidence": 0.97500247
            }
        ]
        }
    ]
    }			
                */

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Debug.Log(httpResponse);
                
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Debug.Log("Response:" + result);
                    return result;
                }
            
            } catch (WebException ex) {
                httpError = true;
                var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                Debug.Log(resp);
    
            }

            return "empty";		
        }    
    }
}