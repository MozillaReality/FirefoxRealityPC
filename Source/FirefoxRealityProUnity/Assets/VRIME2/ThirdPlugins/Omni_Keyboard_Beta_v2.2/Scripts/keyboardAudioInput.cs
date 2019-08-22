// ========================================================================== //
//
//  class keyboardAudioInput
//  -----
//  Purpose: Listens for HMD mic input and drives animation from 0 to 1 based on
//           amplitude of that input.
//
//  Created: 2019-01-15
//  Updated: 2018-01-18
//
//  Created by Andrew Champlin, sound designer (andrew_champlin@htc.com)
// 
// ========================================================================== //

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Htc.Omni;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using VRIME2;

[RequireComponent(typeof(AudioSource))]
public class keyboardAudioInput : MonoBehaviour 
{
    private static keyboardAudioInput mInstance;
    public static keyboardAudioInput Ins 
    {
        get { return mInstance; }        
    }

    #region notShow
	private AudioClip voice { get; set; }
	private AudioSource voiceContainer { get; set; }
    #endregion

    #region basicField
    public InputField inputField;
    public Text HintText;

    #endregion

    #region show in unity editor

	// [Range(0f, 50f)]
    // [Tooltip("Loudness sensitivy; lower value is more sensitive.")]
	// public float loudnessCeiling = 1.5f; //Adjust based on input.
    [HideInInspector]
    [Tooltip("Component attempts to find VIVE mic automatically; use 'Debug' to find system mic name.")]
	public string microphone;

    public GameObject processPanel;    

    [Tooltip("Logs microphone device names for manual entry in 'Microphone' string.")]
    public bool debug = false;



    #endregion

    #region advanced setting


    #endregion

	// float[] clipSampleData;
	
    // [HideInInspector]
    // float clipLoudness;

    
    #region public event
    public delegate void resolveEngineResult(VoiceEngineBase engineBase, SupportLanguage language);

    public event resolveEngineResult VoiceEngineReadyEvent;

    
    #endregion

    private bool record = true;


    #region private members
    private bool isIOProcessing = false;
    
    private bool isRecoding = false;

    private AutoResetEvent waitHandler = null;
    
    private Queue<ProcessObject> processQueue = new Queue<ProcessObject>();

    private Thread processThread;

    private static VoiceEngineBase voiceEngine = null;

    private string filePath;

    private bool initialized = false;
    private bool isFirst = true;
    private bool isStop = true;
    private bool isPleaseSaySomething = true;

    private bool haveEfftiveAudioData = false;

    private Visualizer visualizer;

    #endregion


    [Space(35)]
    [Header("Advanced setting, if you don't understand, don't change")]
    [Header("After user stop speech")]
    [Range(0.2f, 1.2f)]
    public float autoDelaySend = 0.5f;

    [Header("Auto close voice if user donesn't speech")]
    [Range(3.0f, 10.0f)]
    public float autoDelayClose = 3f;
    [Range(5.0f, 12.0f)]
    public float autoDelayCloseWaitData = 8f;

    [Header("Max voice record time")]
    [Range(7, 30)]
    public int maxRecordTime = 15;

    [Header("Don't check this volume when voice starting")]
    [Range(0, 0.5f)]
    public float startTimeInVoice = 0.2f;

    [Header("For check volume, check how many blocks every time")]
    [Range(1,16)]
    public int voiceSampleBlock = 16;

    [Header("If volume change large than it, we think user have speak")]
    [Range(20,60)]
    public int effectSoundLevel = 40;

    [Header("If volume change less than it, we think the speak status not changed")]
    [Range(10,20)]
    public int shakeSoundLevel = 15;

    [Header("If volume less then it, we think user doesn't speak")]
    [Range(-200,-100)]
    public int tooSmallVolume = -160;

    [Header("Max Wave level db")]
    [Range(-50,0)]
    public int maxWaveLevelDb = 0;

    [Header("Max Wave level db")]
    [Range(-120,-50)]
    public int minWaveLevelDb = -85;

    private DateTime startTime;



    private void threadProc() 
    {
        while(Thread.CurrentThread.Equals(processThread)) // if thread not processThread, this thread should be gone
        {
            waitHandler.WaitOne();
            while(processQueue.Count > 0)
            {                
                ProcessObject po = processQueue.Peek();
                string processFilePath = po.filePath;
                if(!isStop) 
                {
                    // VRIME2.VRIME_Debugger.Log("process audio file: " + processFilePath);
                    string error="";    
                    bool httpError;
                    DateTime serverStartTime = DateTime.Now;                
                    string translate = voiceEngine.GetVoiceStringByFile(processFilePath, out error, language, out httpError);                                    
                    int serverResponseTime = (int)(DateTime.Now-serverStartTime).TotalMilliseconds;
                    if(processQueue.Peek().Equals(po)){
                        if(!string.IsNullOrEmpty(translate)) 
                        {
                            resultQueue.Enqueue(translate);
                            VRIME_BISender.Ins.CallVoiceResult(translate, po.duration, serverResponseTime);
                        }                         
                        else 
                        {
                            VRIME2.VRIME_Debugger.Log("response error: " + error);
                            VRIME_BISender.Ins.CallVoiceError(po.duration, serverResponseTime, httpError ? VRIME_BISender.VOICE_ERROR_TYPE_HTTP : VRIME_BISender.VOICE_ERROR_TYPE_JSON, error);
                        }
#if VOICEINPUT_LOGUI
                        VRIME_VoiceInputDebug.DataResponse(!string.IsNullOrEmpty(translate), translate, serverResponseTime, error, processFilePath);
#endif
                    }
                } else {
                    VRIME_BISender.Ins.CallVoiceSkip(po.duration);
#if VOICEINPUT_LOGUI
                    VRIME_VoiceInputDebug.SkipResponse(processFilePath);
#endif
                }  
                if(processQueue.Peek().Equals(po)){
                    processQueue.Dequeue();
                }
                File.Delete(processFilePath);
            }
            isIOProcessing = false;
        }        
    }

    private const string LANGUAGE_PREFIX = "language_";
    
    
    private void Awake() {
        mInstance = this;
    }

    private static SupportLanguage language;

    public static SupportLanguage SpeechLanguage 
    {
        get 
        {
            return language;
        }
        set
        {
            language = value;
            string lauguageStr = Enum.GetName(typeof(SupportLanguage), language);
            if(voiceEngine != null)
            {
                string currentEngine = voiceEngine.VoiceEngineName();

                PlayerPrefs.SetString(LANGUAGE_PREFIX + currentEngine, lauguageStr);
                PlayerPrefs.Save();
            }
        }
    }

    


    public enum VoiceEngineType { AutoSelect, Google, Baidu, None}



    public static string VoiceEngineName {
        get {return voiceEngine.VoiceEngineName(); }
    }



    private Queue<string> resultQueue = new Queue<string>();
    private Queue<string> baiduTokenQueue = new Queue<string>();

    private void OnEnable() 
    {
        VRIME2.VRIME_Debugger.Log("keyboardAudioInput OnEnable");

        // create new thread when onEnable
        waitHandler = new AutoResetEvent(false);
        processThread = new Thread(threadProc);
        processThread.Start();

        //isRecoding = true;
        //startRecord();
        isStop = false;
        isFirst = true;
        setHintText(HintText);
        isPleaseSaySomething = true;        
        resultQueue.Clear();
        recentPos = inputField.caretPosition;
        //VRIME2.VRIME_Debugger.Log("isRecording: " + isRecoding + ", isProcessing: " + isProcessing);
    }

    private void setHintText(Text hintText)
    {
        hintText.text = VRIME2.VRIME_InternationalWord.VoiceInpupHintText(SpeechLanguage);
    }

    private void OnDisable() 
    {
        VRIME2.VRIME_Debugger.Log("keyboardAudioInput OnDisable");
        isStop = true;
        resultQueue.Clear();
        processQueue.Clear();
        
		CancelInvoke("dealyStopForAuto");
		CancelInvoke("delayStopForListen");       
        CancelInvoke("delayClose"); 

        if(isRecoding) 
        {
            VRIME2.VRIME_Debugger.Log("keyboardAudioInput OnDisable, stopRecord");
#if VOICEINPUT_LOGUI
            VRIME_VoiceInputDebug.EndRecored((int)(DateTime.Now - startRecordTime).TotalMilliseconds);
#endif
            //StartCoroutine(stopRecord());
            Microphone.End(microphone); 
            isRecoding = false;
            waitHandler.Set();            
        }

        if(Microphone.IsRecording(microphone))
        {
            Microphone.End(microphone); 
            isRecoding = false;
            
        }

        // clear thread information when onDisable
        processThread = null;
        waitHandler.Set();
        waitHandler = null;        
    }

    public void queueBaiduTokenToSave(string token)
    {
        baiduTokenQueue.Enqueue(token);
    }
    
    private int recentPos = 0;

    private void Update() {
        if(baiduTokenQueue.Count > 0 && voiceEngine is BaiduVoiceEngineBase)
        {
            string baidu_token = baiduTokenQueue.Dequeue();
            ((BaiduVoiceEngineBase)voiceEngine).saveBaiduToken(baidu_token);
        }

        if(!initialized)
            return;

        if(isStop)
            return;

        int currentPos = inputField.caretPosition;
        if(currentPos != recentPos) 
        {
            // cursor moved, close voice
            recentPos = currentPos;
            StartCoroutine("closeVoice");
            return;
        }

        recentPos = currentPos;

        while(resultQueue.Count > 0)
        {
            if(inputField != null) 
            {
                string result = resultQueue.Dequeue();
                StartCoroutine(insertText(result));
                //microphoneText.caretPosition = microphoneText.text.Length - 1;
            }
            //resultQueue.Clear();                       
        }

        if(!isRecoding && !string.IsNullOrEmpty(microphone) && !isIOProcessing)
        {
            startRecord();
        }
    }


    private IEnumerator insertText(string result)
    {
        string text = inputField.text;
        int currentPos = inputField.caretPosition;

        
		int aAnchorPos = inputField.selectionAnchorPosition;
		int aFocusPos = inputField.selectionFocusPosition;

        // remove selection
        if(aAnchorPos != aFocusPos)
        {
            int start = Mathf.Min(aAnchorPos, aFocusPos);
            int end = Mathf.Max(aAnchorPos, aFocusPos);
                        try
            {
                text = text.Remove(start, end - start);
            }
            catch
            {
                VRIME_Debugger.LogError(inputField.name, "Remove Selection Error");
            }
        }

        
        inputField.ActivateInputField();

        if(!string.IsNullOrEmpty(result))
        {
            if(isFirst) 
            {
                isFirst = false;
            } else {
                result = " " + result;
            }
        }
        
        text = text.Insert(currentPos, result);
        inputField.text = text;

        inputField.caretPosition = currentPos + result.Length;
        recentPos = inputField.caretPosition;
        inputField.ForceLabelUpdate();
        // BI Logger
        VRIME_BISender.Ins.LabelInsert(result, currentPos);
        yield return null;
    }

    private IEnumerator getVocieEngine(VoiceEngineType type)
    {
        if(VRIME_Manager.Ins.voiceEngineType == VoiceEngineType.None)
        {
            VRIME2.VRIME_Debugger.Log("no voice engine");
            yield break;
        }

        switch(type)
        {
            case VoiceEngineType.Baidu:
                string baiduApiKey = VRIME_Manager.Ins.baiduApiKey;
                string baiduApiSecert = VRIME_Manager.Ins.baiduApiSecert;
                if(!string.IsNullOrEmpty(baiduApiKey) && !string.IsNullOrEmpty(baiduApiSecert)) 
                {
                    voiceEngine = new BaiduVoiceEngineBase(baiduApiKey, baiduApiSecert, this);
                    language = (SupportLanguage)Enum.Parse(typeof(SupportLanguage), PlayerPrefs.GetString(LANGUAGE_PREFIX + voiceEngine.VoiceEngineName(), VRIME_Manager.Ins.defaultVoiceInputLanguage.ToString()));
                    if(VoiceEngineReadyEvent != null)
                        VoiceEngineReadyEvent(voiceEngine, language);
                }
                yield break;
            case VoiceEngineType.Google:
            string googleApiKey = VRIME_Manager.Ins.googleApiKey;
                if(!string.IsNullOrEmpty(googleApiKey))
                {                    
                    voiceEngine = new GoogleVoiceEngineBase(googleApiKey);
                    language = (SupportLanguage)Enum.Parse(typeof(SupportLanguage), PlayerPrefs.GetString(LANGUAGE_PREFIX + voiceEngine.VoiceEngineName(), VRIME_Manager.Ins.defaultVoiceInputLanguage.ToString()));
                    if(VoiceEngineReadyEvent != null)
                        VoiceEngineReadyEvent(voiceEngine, language);
                }
                yield break;
            case VoiceEngineType.AutoSelect:
                yield return getEngineTypeByNetwork();
                yield break;            
        }           
    }

    private IEnumerator getEngineTypeByNetwork()
    {
        UnityWebRequest request = UnityWebRequest.Get("https://store.viveport.com/api/whichcountry/v1/plain");
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            VRIME2.VRIME_Debugger.Log(request.error);
            yield break;
        }

        string result = request.downloadHandler.text;
        VoiceEngineType type = VoiceEngineType.Google;

        if("CN".Equals(result))
        {
            type = VoiceEngineType.Baidu;
        } else {
            type = VoiceEngineType.Google;
        } 

        yield return getVocieEngine(type);

        yield return null;
   
    }
    
    private VoiceEngineType getEngineByNetwork()
    {        
        VRIME2.VRIME_Debugger.Log("getEngineByNetwork");
        string url = "https://store.viveport.com/api/whichcountry/v1/plain";
        //string url = "https://www.google.com/robots.txt";

        try {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        httpWebRequest.Timeout = 3;
        //httpWebRequest.Method = "GET";    
        //httpWebRequest.GetRequestStream().Close();    
        //httpWebRequest.Proxy = new WebProxy("127.0.0.1", 8888);
                
        VRIME2.VRIME_Debugger.Log("before get response");
        
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            
            VRIME2.VRIME_Debugger.Log("after get response");
                    
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                // VRIME2.VRIME_Debugger.Log(result);
                if("CN".Equals(result))
                {
                    return VoiceEngineType.Baidu;
                } else {
                    return VoiceEngineType.Google;
                }            
            }    
        } catch (WebException ex) {
            using (var streamReader = new StreamReader(ex.Response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                VRIME2.VRIME_Debugger.Log(result);
            }    
            return VoiceEngineType.Google;
        }
        // VRIME2.VRIME_Debugger.Log("end getEngineByNetwork"); // Unreachable code detected
    }

    private bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
        VRIME2.VRIME_Debugger.Log("MyRemoteCertificateValidationCallback enter");
        bool isOk = true;
        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None) {
            for (int i=0; i<chain.ChainStatus.Length; i++) {
                if (chain.ChainStatus [i].Status != X509ChainStatusFlags.RevocationStatusUnknown) {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build ((X509Certificate2)certificate);
                    if (!chainIsValid) {
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }

    public void InitVoiceEngine(resolveEngineResult resolveEvent, MonoBehaviour monoBehaviour)    
    {
        if(resolveEvent != null) 
        {
            VoiceEngineReadyEvent += resolveEvent;
        }
        VoiceEngineType type = VRIME_Manager.Ins.voiceEngineType;
        monoBehaviour.StartCoroutine(getVocieEngine(type));
    }

    void Start()
    {
        VRIME2.VRIME_Debugger.Log("keyboardAudioInput start");
    	ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;

        //voiceEngine = new GoogleVoiceEngineBase(googleApiKey);
        VoiceEngineType type = VRIME_Manager.Ins.voiceEngineType;

        if(voiceEngine == null)
        {
            StartCoroutine(getVocieEngine(type));            
        }
        
        // processThread = new Thread(threadProc);
        // processThread.Start();

        if (debug && microphone == "")
        {
            for (int i = 0; i < Microphone.devices.Length; i++)
            {
                if (Microphone.devices[i].Contains("VIVE") || Microphone.devices[i].Contains("Advanced Audio Device")) //These are common names for VIVE HMD microphones on Windows.
                    VRIME2.VRIME_Debugger.Log("VIVE Microphone " + i + " device name: " + Microphone.devices[i] + " (Enter this string into the 'Microphone' field on the 'keybaordAudioInput' component in the inspector.)");
                else VRIME2.VRIME_Debugger.Log("Microphone " + i + " device name: " + Microphone.devices[i]);
            }
        }
        

        if (visualizer == null)
            visualizer = FindObjectOfType<Visualizer>();
        else if (FindObjectOfType<Visualizer>() == null) { VRIME2.VRIME_Debugger.LogWarning("No WCL visualizer in scene."); return; }
        
        if(Microphone.devices.Length > 0)
        {
            microphone = Microphone.devices[0];            
            //StartCoroutine("setMicrophoneText", microphone);
//            resultQueue.Enqueue(microphone);
        }

        // for (int i = 0; i < Microphone.devices.Length; i++)
        // {
        //     if (Microphone.devices[i] == "Microphone (VIVE Pro Mutimedia Audio)" || Microphone.devices[i] == "Microphone (USB Advanced Audio Device)" || Microphone.devices[i].Contains("VIVE") && microphone == null)
        //     {
        //         if (debug) VRIME2.VRIME_Debugger.Log("VIVE HMD mic found! Device name: " + Microphone.devices[i]);
        //         microphone = Microphone.devices[i];
        //     }
        // }

        if (microphone == "") { VRIME2.VRIME_Debugger.LogWarning("No VIVE HMD microphones found. Visualizer animation running at 100% for demonstration purposes."); visualizer.micIn = null; visualizer.voiceInput = 1f; return; }
        if (record)
		{
			voiceContainer = gameObject.GetComponent<AudioSource>();
			//voiceContainer.mute = true;
			voiceContainer.loop = true;
            //isProcessing = false;
            //startRecord();
		}

        initialized = true;
	}

    public void startRecord()
    {
        if(microphone != null && !isRecoding ) { 
            resetDb();
            startRecordTime = DateTime.Now;
            isRecoding = true;
            //disabledPanel.SetActive(false);
            VRIME2.VRIME_Debugger.Log("microphone test start");
            haveEfftiveAudioData = false;
            filePath = generateFilePath();
            startTime = DateTime.Now;
            VRIME_BISender.Ins.CallVoiceStart();
#if VOICEINPUT_LOGUI
            VRIME_VoiceInputDebug.StartRecored(startRecordTime, microphone);
#endif
            voice = Microphone.Start(microphone, true, maxRecordTime, 16000); voiceContainer.clip = voice; if(debug) VRIME2.VRIME_Debugger.Log("Listening on Vive HMD mic: " + microphone);             
            voiceContainer.mute = true;
            Invoke("dealyStopForAuto", maxRecordTime);

            if(processQueue.Count > 0) {
                Invoke("delayClose", autoDelayCloseWaitData);
            } else {
                Invoke("delayClose", autoDelayClose);
            }
            
        }
    }

    public IEnumerator stopRecord()
    {        
        if(!isRecoding)
            yield break;

        //disabledPanel.SetActive(true);
        //yield return null;
        
        isRecoding = false;

        int position = Microphone.GetPosition(microphone); // end position
        VRIME2.VRIME_Debugger.Log("stopRecord ,  micrephone.End");
        float max = maxDb.Value;
        float min = minDb.Value;
        Microphone.End(microphone); //Stop the audio recording
        
		
        // if(isStop) {
        //     waitHandler.Set();                        
        //     yield break;
        // }

        VRIME2.VRIME_Debugger.Log("Recording Stopped, start process");
        isIOProcessing = true;

        string saveFilePath = filePath;
        string processFilePath = null;
        int duration = (int)(DateTime.Now - startRecordTime).TotalMilliseconds;
        if(haveEfftiveAudioData) 
        {
            SavWav.Save(saveFilePath, voice, position); //Save a temporary Wav File		
            processFilePath = saveFilePath;                        
        } else {
            VRIME_BISender.Ins.CallVoiceSkip(duration);
        }
        filePath = null;

        isIOProcessing = false;   

        if(!string.IsNullOrEmpty(processFilePath))
        {
            processQueue.Enqueue(new ProcessObject(max, min, processFilePath, duration));
#if VOICEINPUT_LOGUI
            VRIME_VoiceInputDebug.GetProcessingData(max, min, processFilePath, duration);
#endif
            waitHandler.Set();
        } 


        startRecord();        
        yield return null;
    }

    private IEnumerator disableVoice()
    {
        processPanel.SetActive(true);
        yield return null;
    }

    private void dealyStopForAuto() 
	{		
		CancelInvoke("dealyStopForAuto");
		CancelInvoke("delayStopForListen");        
        CancelInvoke("delayClose");
		//stopRecord();
        StartCoroutine("stopRecord", null);
	} 

	public void delayStopForListen()
	{		
		CancelInvoke("dealyStopForAuto");		
		CancelInvoke("delayStopForListen");
        CancelInvoke("delayClose");
        StartCoroutine("stopRecord", null);		
		//stopRecord();
	} 

    public void delayClose()
    {
		CancelInvoke("dealyStopForAuto");		
		CancelInvoke("delayStopForListen");     
        CancelInvoke("delayClose");   
        StartCoroutine("closeVoice", null);
    }      

    public IEnumerator closeVoice()
    {
        VRIME2.VRIME_Debugger.Log("closeVoice");
        VRIME_BISender.Ins.CallVoiceAutoClose();
        VRIME_VoiceOversee.Ins.CallVoiceUI(false);        
        yield return null;
    }

    private string generateFilePath()
    {
        string filePath = "";
        float filenameRand = UnityEngine.Random.Range(0.0f, 10.0f);
        string filename = "testing" + filenameRand;
        if (!filename.ToLower().EndsWith(".wav"))
        {
            filename += ".wav";
        }

        filePath = Path.Combine("testing/", filename);
        filePath = Path.Combine(Application.persistentDataPath, filePath);

        return filePath;
    }

    
    private float? maxDb = null;
    private float? minDb = null;

    private DateTime startRecordTime;

    private void resetDb()
    {
        minDb = null;
        maxDb = null;
    }

	public float GetSampleAmplitude()
	{	
		// clipSampleData = new float[1024];
		// voice.GetData(clipSampleData, voiceContainer.timeSamples);

		// foreach (var sample in clipSampleData)		
		// 	clipLoudness += Mathf.Abs(sample);
        

		// var normalizedVol = Mathf.Clamp((clipLoudness / loudnessCeiling), 0f, 1f);
		// clipLoudness = 0f;

        if((DateTime.Now - startRecordTime).TotalMilliseconds < (startTimeInVoice*1000))
        {
            // not effitive sample
            return 0;
        }



        float levelMax = MicrophoneLevelMax();
		float db = MicrophoneLevelMaxDecibels(levelMax);
        
        
        if(maxDb == null && db > tooSmallVolume)
        {
            maxDb = db;
        }

        if(minDb == null && db > tooSmallVolume)
        {
            minDb = db;
        }

        if(maxDb < db)
        {
            maxDb = db;
        }

        if(minDb > db)
        {
            minDb = db;
        }

        //VRIME2.VRIME_Debugger.Log("db: " + db + ", maxDb: " + maxDb + ", minDb: " + minDb);

        if(isStop)
            return 0;

        if(minDb == null || maxDb == null)
            return 0;

        if(Mathf.Abs(maxDb.Value - minDb.Value) > effectSoundLevel)
        {
            if(Mathf.Abs(maxDb.Value - db) < shakeSoundLevel)
            {
                // still mean max
                if(!haveEfftiveAudioData) 
                {
                    VRIME2.VRIME_Debugger.Log("get Efftive audio data");
                    haveEfftiveAudioData = true;
                    if(isPleaseSaySomething)
                    {
                        VRIME2.VRIME_Debugger.Log("close please say something");
                        VRIME_VoiceOversee.Ins.AnimeToVoiceOpend();
                        isPleaseSaySomething = false;
                    }
                    if(IsInvoking("delayClose"))
                    {
                        CancelInvoke("delayClose");
                    }
                }
            }

            if(Mathf.Abs(minDb.Value - db) < shakeSoundLevel)
            {
                // still mean min
                if(haveEfftiveAudioData)
                {         
                    if(!IsInvoking("delayStopForListen"))
                    {
                        VRIME2.VRIME_Debugger.Log("invoke delay stop for listen");
                        Invoke("delayStopForListen", autoDelaySend);                
                    }
                }            
            }
        }


        //db range : 0~-120
        // if(db > -10)
        // {
        //     CancelInvoke("delayStopForListen");
        //     if(!haveEfftiveAudioData) 
        //     {
        //         VRIME2.VRIME_Debugger.Log("get Efftive audio data");
        //         haveEfftiveAudioData = true;
        //     }
        // }
        
        // if(db < -65)
        // {
        //     db=-65f;

        //     if(haveEfftiveAudioData)
        //     {         
        //         if(!IsInvoking("delayStopForListen"))
        //         {
        //             VRIME2.VRIME_Debugger.Log("invoke delay stop for listen");
        //             Invoke("delayStopForListen", 1);                
        //         }
        //     }
        // } 
        if(db< minWaveLevelDb)
        {
            db= minWaveLevelDb;
        }

        int waveRange = maxWaveLevelDb - minWaveLevelDb;
        
        var normalizedVol = Mathf.Clamp(1 - Mathf.Abs((db-maxWaveLevelDb)/waveRange), 0f, 1f);

		return normalizedVol;
	}    


    
    float MicrophoneLevelMax()
    {
        const int _sampleWindow = 128*16;

        float levelMax = 0;
        float[] waveData = new float[_sampleWindow];
        int micPosition = Microphone.GetPosition(microphone) - (_sampleWindow + 1); // null means the first microphone        
        if (micPosition < 0) return 0;

        //VRIME2.VRIME_Debugger.Log("mic position: " + micPosition);
				
        voice.GetData(waveData, micPosition);
				
        // Getting a peak on the last 128 samples
        for (int i = 0; i < _sampleWindow; i++)
        {
            float wavePeak = waveData[i] * waveData[i];
            if (levelMax < wavePeak)
            {
                levelMax = wavePeak;
            }
        }
        return levelMax;
    }

    float MicrophoneLevelMaxDecibels(float levelMax)
    {
        float db = 20 * Mathf.Log10(Mathf.Abs(levelMax));
        return db;
    }

    private struct ProcessObject
    {
        public float maxDb;
        public float minDb;
        public string filePath;
        public int duration;

        public ProcessObject(float maxDb, float minDb, string filePath, int duration)
        {
            this.maxDb = maxDb;
            this.minDb = minDb;
            this.filePath = filePath;
            this.duration = duration;
        }
    }
}
