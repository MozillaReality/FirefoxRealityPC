using UnityEngine;
using System.Collections;

public class FxRController : MonoBehaviour
{
    public enum FXR_LOG_LEVEL
    {
        FXR_LOG_LEVEL_DEBUG = 0,
        FXR_LOG_LEVEL_INFO,
        FXR_LOG_LEVEL_WARN,
        FXR_LOG_LEVEL_ERROR,
        FXR_LOG_LEVEL_REL_INFO
    }

    [SerializeField]
    private FXR_LOG_LEVEL currentLogLevel = FXR_LOG_LEVEL.FXR_LOG_LEVEL_INFO;

    // Main reference to the plugin functions. Created in OnEnable(), destroyed in OnDestroy().
    private FxRPlugin fxr_plugin = null;

    //
    // MonoBehavior methods.
    //

    void Awake()
    {
        Debug.Log("FxRController.Awake())");
    }

    void OnEnable()
    {
        Debug.Log("FxRController.OnEnable()");

        fxr_plugin = new FxRPlugin();

        Application.runInBackground = true;

        // Register the log callback.
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:                        // Unity Editor on OS X.
            case RuntimePlatform.OSXPlayer:                        // Unity Player on OS X.
            case RuntimePlatform.WindowsEditor:                    // Unity Editor on Windows.
            case RuntimePlatform.WindowsPlayer:                    // Unity Player on Windows.
            case RuntimePlatform.LinuxEditor:
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.WSAPlayerX86:                     // Unity Player on Windows Store X86.
            case RuntimePlatform.WSAPlayerX64:                     // Unity Player on Windows Store X64.
            case RuntimePlatform.WSAPlayerARM:                     // Unity Player on Windows Store ARM.
                fxr_plugin.fxrRegisterLogCallback(Debug.Log);
                break;
            case RuntimePlatform.Android:                          // Unity Player on Android.
            case RuntimePlatform.IPhonePlayer:                     // Unity Player on iOS.
                break;
            default:
                break;
        }

        // Set the reference to the plugin in any other objects in the scene that need it.
        FxRWindow[] fxrwindows = FindObjectsOfType<FxRWindow>();
        foreach (FxRWindow w in fxrwindows) {
            w.fxr_plugin = fxr_plugin;
        }
    }

    void OnDisable()
    {
        Debug.Log("FxRController.OnDisable()");

        // Clear the references to the plugin in any other objects in the scene that have it.
        FxRWindow[] fxrwindows = FindObjectsOfType<FxRWindow>();
        foreach (FxRWindow w in fxrwindows)
        {
            w.fxr_plugin = null;
        }

        // Since we might be going away, tell users of our Log function
        // to stop calling it.
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                goto case RuntimePlatform.WindowsPlayer;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
            //case RuntimePlatform.LinuxEditor:
            case RuntimePlatform.LinuxPlayer:
                fxr_plugin.fxrRegisterLogCallback(null);
                break;
            case RuntimePlatform.Android:
                break;
            case RuntimePlatform.IPhonePlayer:
                break;
            case RuntimePlatform.WSAPlayerX86:
            case RuntimePlatform.WSAPlayerX64:
            case RuntimePlatform.WSAPlayerARM:
                fxr_plugin.fxrRegisterLogCallback(null);
                break;
            default:
                break;
        }
        fxr_plugin = null;


    }

    void Start()
    {
        Debug.Log("FxRController.Start()");

        Debug.Log("Fx version " + fxr_plugin.fxrGetFxVersion());
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("FxRController.Update()");
    }

    public FXR_LOG_LEVEL LogLevel
    {
        get
        {
            return currentLogLevel;
        }

        set
        {
            currentLogLevel = value;
            fxr_plugin.fxrSetLogLevel((int)currentLogLevel);
        }
    }


}
