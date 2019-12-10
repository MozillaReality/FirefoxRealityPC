// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019, Mozilla.

#define USE_EDITOR_HARDCODED_FIREFOX_PATH // Comment this out to not use a hardcoded path in editor, but instead use StreamingAssets

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
using Valve.VR.InteractionSystem;
using VRIME2;
using Hand = Valve.VR.InteractionSystem.Hand;

public class FxRController : MonoBehaviour
{
#if UNITY_EDITOR
    // Set the following to the location of your local desktop firefox build to use in editor
    private const string HardcodedFirefoxPath = "d:\\patri\\dev\\Mozilla\\gecko_build_release";
#endif
    public enum FXR_LOG_LEVEL
    {
        FXR_LOG_LEVEL_DEBUG = 0,
        FXR_LOG_LEVEL_INFO,
        FXR_LOG_LEVEL_WARN,
        FXR_LOG_LEVEL_ERROR,
        FXR_LOG_LEVEL_REL_INFO
    }

    [SerializeField] private FXR_LOG_LEVEL currentLogLevel = FXR_LOG_LEVEL.FXR_LOG_LEVEL_INFO;

    [SerializeField] private FxRVideoController VideoController;

    [SerializeField] private Transform EnvironmentOrigin;

    public enum FXR_BROWSING_MODE
    {
        FXR_BROWSER_MODE_DESKTOP_INSTALL,
        FXR_BROWSER_MODE_WEB_BROWSING,
        FXR_BROWSER_MODE_FULLSCREEN_VIDEO,
        FXR_BROWSER_MODE_WEBXR
    }

    public delegate void BrowsingModeChanged(FXR_BROWSING_MODE browsingMode);

    public static BrowsingModeChanged OnBrowsingModeChanged;

    public static FXR_BROWSING_MODE CurrentBrowsingMode
    {
        get => currentBrowsingMode;
        private set
        {
            if (currentBrowsingMode != value)
            {
                OnBrowsingModeChanged?.Invoke(value);
            }

            currentBrowsingMode = value;
            if (currentBrowsingMode != FXR_BROWSING_MODE.FXR_BROWSER_MODE_WEB_BROWSING
                && VRIME_Manager.Ins.ShowState)
            {
                VRIME_Manager.Ins.HideIME();
            }

            FxRWindow[] fxrwindows = FindObjectsOfType<FxRWindow>();
            foreach (FxRWindow w in fxrwindows)
            {
                w.Visible = currentBrowsingMode == FXR_BROWSING_MODE.FXR_BROWSER_MODE_WEB_BROWSING;
            }
        }
    }

    private static FXR_BROWSING_MODE currentBrowsingMode = FXR_BROWSING_MODE.FXR_BROWSER_MODE_DESKTOP_INSTALL;

    public FxRPlugin Plugin => fxr_plugin;

    // Main reference to the plugin functions. Created in OnEnable(), destroyed in OnDisable().
    private FxRPlugin fxr_plugin = null;

    public bool DontCloseNativeWindowOnClose = false;

    private List<FxRLaserPointer> LaserPointers
    {
        get
        {
            if (laserPointers == null)
            {
                laserPointers = new List<FxRLaserPointer>();
                laserPointers.AddRange(FindObjectsOfType<FxRLaserPointer>());
            }

            return laserPointers;
        }
    }

    private List<FxRLaserPointer> laserPointers;
    
    private List<Hand> Hands
    {
        get
        {
            if (hands == null)
            {
                hands = new List<Hand>();
                hands.AddRange(FindObjectsOfType<Hand>());
            }

            return hands;
        }
    }

    private List<Hand> hands;
    private int _hackKeepWindowIndex;

    //
    // MonoBehavior methods.
    //

    void Awake()
    {
        Debug.Log("FxRController.Awake())");
    }

    [AOT.MonoPInvokeCallback(typeof(FxRPluginLogCallback))]
    public static void Log(System.String msg)
    {
        if (msg.StartsWith("[error]")) Debug.LogError(msg);
        else if (msg.StartsWith("[warning]")) Debug.LogWarning(msg);
        else Debug.Log(msg); // includes [info] and [debug].
    }

    public void SendKeyEvent(int keycode)
    {
        // TODO: Introduce concept of "focused" window, once we allow more than one, so these events can be sent to the window that has focus 
        FxRWindow[] fxrwindows = FindObjectsOfType<FxRWindow>();

        if (fxrwindows.Length > 0)
        {
            fxr_plugin.fxrKeyEvent(fxrwindows[0].WindowIndex, keycode);
        }
    }

    private Vector3 initialBodyDirection;
    private bool bodyDirectionInitialzed;
    private int bodyDirectionChecks;
    
    void OnEnable()
    {
        initialBodyDirection = Player.instance.bodyDirectionGuess;

        Debug.Log("FxRController.OnEnable()");

        fxr_plugin = new FxRPlugin();

        Application.runInBackground = true;

        // Register the log callback.
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor: // Unity Editor on OS X.
            case RuntimePlatform.OSXPlayer: // Unity Player on OS X.
            case RuntimePlatform.WindowsEditor: // Unity Editor on Windows.
            case RuntimePlatform.WindowsPlayer: // Unity Player on Windows.
            case RuntimePlatform.LinuxEditor:
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.WSAPlayerX86: // Unity Player on Windows Store X86.
            case RuntimePlatform.WSAPlayerX64: // Unity Player on Windows Store X64.
            case RuntimePlatform.WSAPlayerARM: // Unity Player on Windows Store ARM.
            case RuntimePlatform.Android: // Unity Player on Android.
            case RuntimePlatform.IPhonePlayer: // Unity Player on iOS.
                fxr_plugin.fxrRegisterLogCallback(Log);
                break;
            default:
                break;
        }

        // Register the full screen video callbacks
        fxr_plugin.fxrRegisterFullScreenBeginCallback(HandleFullScreenBegin);
        fxr_plugin.fxrRegisterFullScreenEndCallback(HandleFullScreenEnd);

        // Give the plugin a place to look for resources.

        string resourcesPath = Application.streamingAssetsPath;

#if (UNITY_EDITOR && USE_EDITOR_HARDCODED_FIREFOX_PATH)
        resourcesPath = HardcodedFirefoxPath;
#endif
        fxr_plugin.fxrSetResourcesPath(resourcesPath);

        // Set any launch-time parameters.
        if (DontCloseNativeWindowOnClose)
            fxr_plugin.fxrSetParamBool(FxRPlugin.FxRParam.b_CloseNativeWindowOnClose, false);

        // Set the reference to the plugin in any other objects in the scene that need it.
        FxRWindow[] fxrwindows = FindObjectsOfType<FxRWindow>();
        foreach (FxRWindow w in fxrwindows)
        {
            w.fxr_plugin = fxr_plugin;
        }

        VideoController.fxr_plugin = fxr_plugin;
        // VRIME keyboard event registration
        VRIME_Manager.Ins.onCallIME.AddListener(imeShowHandle);

        FxRFirefoxDesktopInstallation.OnInstallationProcessComplete += HandleInstallationProcessComplete;
    }

    private void HandleInstallationProcessComplete()
    {
        CurrentBrowsingMode = FXR_BROWSING_MODE.FXR_BROWSER_MODE_WEB_BROWSING;
    }

    private void HandleFullScreenBegin(int pixelwidth, int pixelheight, int format, int projection)
    {
        HandleFullScreenBegin(pixelwidth, pixelheight, format, (FxRVideoProjectionMode.PROJECTION_MODE) projection);
    }

    private void HandleFullScreenBegin(int pixelwidth, int pixelheight, int format,
        FxRVideoProjectionMode.PROJECTION_MODE projectionMode)
    {
        Debug.Log("Received Full Screen Begin from Plugin");

        if (VideoController.ShowVideo(pixelwidth, pixelheight, format, projectionMode, _hackKeepWindowIndex))
        {
            CurrentBrowsingMode = FXR_BROWSING_MODE.FXR_BROWSER_MODE_FULLSCREEN_VIDEO;
        }
        else
        {
            Debug.LogError("FxRController::HandleFullScreenBegin: Couldn't start full screen video.");
        }
    }

    public void UserExitFullScreenVideo()
    {
        // Notify plugin we are closing video by sending escape key
        SendKeyEvent(27);
        HandleFullScreenEnd();
    }

    private void HandleFullScreenEnd()
    {
        VideoController.ExitVideo();
        CurrentBrowsingMode = FXR_BROWSING_MODE.FXR_BROWSER_MODE_WEB_BROWSING;
        FxRWindow[] fxrwindows = FindObjectsOfType<FxRWindow>();
        foreach (FxRWindow window in fxrwindows)
        {
            window.RecreateVideoTexture();
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

        fxr_plugin.fxrSetResourcesPath(null);

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

        fxr_plugin.fxrRegisterFullScreenBeginCallback(null);
        fxr_plugin.fxrRegisterFullScreenEndCallback(null);

        fxr_plugin = null;

        // VRIME keyboard event registration
        VRIME_Manager.Ins.onCallIME.RemoveListener(imeShowHandle);
    }

    void Start()
    {
        Debug.Log("FxRController.Start()");

        Debug.Log("Fx version " + fxr_plugin.fxrGetFxVersion());

        fxr_plugin.fxrStartFx(OnFxWindowCreationRequestComplete, OnFxWindowResized, OnFxRVREvent);

        IntPtr openVRSession = XRDevice.GetNativePtr();
        if (openVRSession != IntPtr.Zero)
        {
            fxr_plugin.fxrSetOpenVRSessionPtr(openVRSession);
        }
    }

    void Update()
    {
        if (!bodyDirectionInitialzed 
            && !initialBodyDirection.Equals(Player.instance.bodyDirectionGuess))
        {
            bodyDirectionChecks++;
            if (bodyDirectionChecks > 3)
            {
                EnvironmentOrigin.forward = Player.instance.bodyDirectionGuess;
                EnvironmentOrigin.transform.position = Player.instance.feetPositionGuess;
                bodyDirectionInitialzed = true;
            }
        }

        if (IMEStateChanged && lastIMEState == FxRPlugin.FxREventState.Focus && !VRIME_Manager.Ins.ShowState)
        {
            VRIME_Manager.Ins.ShowIME("");
        }
        else if (IMEStateChanged && lastIMEState == FxRPlugin.FxREventState.Blur && VRIME_Manager.Ins.ShowState)
        {
            VRIME_Manager.Ins.HideIME();
        }

        IMEStateChanged = false;

        if (FullScreenStateChanged)
        {
            if (lastFullScreenState == FxRPlugin.FxREventState.Fullscreen_Enter)
            {
                FxRWindow[] fxrwindows = FindObjectsOfType<FxRWindow>();
                if (fxrwindows.Length > 0)
                {
                    // TODO: Eventually, the pixel size, format, and video projection should come from browser. For now, we'll grab it from the window
                    var window = fxrwindows[0];

                    int formatNative;
                    switch (window.TextureFormat)
                    {
                        case TextureFormat.RGBA32:
                            formatNative = 1;
                            break;
                        case TextureFormat.BGRA32:
                            formatNative = 2;
                            break;
                        case TextureFormat.ARGB32:
                            formatNative = 3;
                            break;
                        case TextureFormat.RGB24:
                            formatNative = 5;
                            break;
                        case TextureFormat.RGBA4444:
                            formatNative = 7;
                            break;
                        case TextureFormat.RGB565:
                            formatNative = 9;
                            break;
                        default:
                            formatNative = 0;
                            break;
                    }

                    HandleFullScreenBegin(window.PixelSize.x, window.PixelSize.y, formatNative,
                        FxRVideoProjectionMode.PROJECTION_MODE.VIDEO_PROJECTION_2D);
                }
            }
            else if (lastFullScreenState == FxRPlugin.FxREventState.Fullscreen_Exit)
            {
                HandleFullScreenEnd();
            }
        }

        FullScreenStateChanged = false;

        if (WindowCreationRequestCompleteCallbackCalled)
        {
            HandleWindowCreated();
            WindowCreationRequestCompleteCallbackCalled = false;
        }
    }

    public void ToggleKeyboard()
    {
        if (!VRIME_Manager.Ins.ShowState)
        {
            VRIME_Manager.Ins.ShowIME("");
        }
        else
        {
            VRIME_Manager.Ins.HideIME();
        }
    }

    private void imeShowHandle(bool iShow)
    {
        foreach (var laserPointer in LaserPointers)
        {
            laserPointer.enabled = !iShow;
        }

        if (iShow)
        {
            SteamVR_Actions._default.GrabGrip.AddOnChangeListener(VRIME_Manager.Ins.MoveKeyboardHandle,
                SteamVR_Input_Sources.LeftHand);
            SteamVR_Actions._default.GrabGrip.AddOnChangeListener(VRIME_Manager.Ins.MoveKeyboardHandle,
                SteamVR_Input_Sources.RightHand);

            foreach (var hand in Hands)
            {
                hand.HideController();
            }
            //SteamVR_Actions._default.TouchPress.AddOnChangeListener(VRIME_Manager.Ins.MoveCursorHandle, SteamVR_Input_Sources.LeftHand);
            //SteamVR_Actions._default.TouchPress.AddOnChangeListener(VRIME_Manager.Ins.MoveCursorHandle, SteamVR_Input_Sources.RightHand);

            //SteamVR_Actions._default.TouchPos.AddOnAxisListener(VRIME_Manager.Ins.MoveCursorPositionHandle, SteamVR_Input_Sources.LeftHand);
            //SteamVR_Actions._default.TouchPos.AddOnAxisListener(VRIME_Manager.Ins.MoveCursorPositionHandle, SteamVR_Input_Sources.RightHand);
        }
        else
        {
            SteamVR_Actions._default.GrabGrip.RemoveOnChangeListener(VRIME_Manager.Ins.MoveKeyboardHandle,
                SteamVR_Input_Sources.LeftHand);
            SteamVR_Actions._default.GrabGrip.RemoveOnChangeListener(VRIME_Manager.Ins.MoveKeyboardHandle,
                SteamVR_Input_Sources.RightHand);

            foreach (var hand in Hands)
            {
                hand.ShowController();
            }
            //SteamVR_Actions._default.TouchPress.RemoveOnChangeListener(VRIME_Manager.Ins.MoveCursorHandle, SteamVR_Input_Sources.LeftHand);
            //SteamVR_Actions._default.TouchPress.RemoveOnChangeListener(VRIME_Manager.Ins.MoveCursorHandle, SteamVR_Input_Sources.RightHand);

            //SteamVR_Actions._default.TouchPos.RemoveOnAxisListener(VRIME_Manager.Ins.MoveCursorPositionHandle, SteamVR_Input_Sources.LeftHand);
            //SteamVR_Actions._default.TouchPos.RemoveOnAxisListener(VRIME_Manager.Ins.MoveCursorPositionHandle, SteamVR_Input_Sources.RightHand);
        }
    }

    [AOT.MonoPInvokeCallback(typeof(FxRPluginWindowCreationRequestCompleteCallback))]
    void OnFxWindowCreationRequestComplete(int uid, int windowIndex)
    {
        WindowCreationRequestCompleteCallbackCalled = true;
        WindowCreationRequestCompleteCallbackParams = new WindowCreationRequestCompleteParams()
        {
            uid = uid,
            windowIndex = windowIndex
        };
    }

//    private IEnumerator TestRsize(FxRWindow window)
//    {
//        while (true)
//        {
//            yield return new WaitForSeconds(5f);
//            int width = Mathf.CeilToInt(Random.Range(1000f, 1080f));
//            int height = Mathf.CeilToInt(Random.Range(1000f, 1080f));
//            if (!window.Resize(width, height))
//            {
//                Debug.LogWarning(">>> unsuccessful at resizing window.");
//            }
//        }
//    }

    [AOT.MonoPInvokeCallback(typeof(FxRPluginWindowResizedCallback))]
    void OnFxWindowResized(int uid, int widthPixels, int heightPixels)
    {
        FxRWindow window = FxRWindow.FindWindowWithUID(uid);
        if (window == null)
        {
            Debug.LogError("FxRController.OnFxWindowResized: Received update request for a window that doesn't exist.");
            return;
        }

        window.WasResized(widthPixels, heightPixels);
    }

    FxRPlugin.FxREventState lastIMEState = FxRPlugin.FxREventState.Blur;
    private bool IMEStateChanged;
    private bool FullScreenStateChanged;
    private bool WindowCreationRequestCompleteCallbackCalled;

    private struct WindowCreationRequestCompleteParams
    {
        public int uid;
        public int windowIndex;
    }

    private WindowCreationRequestCompleteParams WindowCreationRequestCompleteCallbackParams;

    FxRPlugin.FxREventState lastFullScreenState = FxRPlugin.FxREventState.Fullscreen_Exit;

    [AOT.MonoPInvokeCallback(typeof(FxRPluginVREventCallback))]
    void OnFxRVREvent(int uid, int eventType, int eventData1, int eventData2)
    {
        FxRPlugin.FxREventState eventState = (FxRPlugin.FxREventState) eventData1;
        if ((FxRPlugin.FxREventType) eventType == FxRPlugin.FxREventType.IME)
        {
            if (eventState != lastIMEState)
            {
                IMEStateChanged = true;
                lastIMEState = eventState;
            }
        }
        else if ((FxRPlugin.FxREventType) eventType == FxRPlugin.FxREventType.Fullscreen)
        {
            FullScreenStateChanged = true;
            lastFullScreenState = eventState;
        }
    }


    private void OnApplicationQuit()
    {
        Debug.Log("FxRController.OnApplicationQuit()");
        FxRWindow[] fxrwindows = FindObjectsOfType<FxRWindow>();
        foreach (FxRWindow w in fxrwindows)
        {
            w.Close();
        }

        fxr_plugin.fxrStopFx();
    }

    public FXR_LOG_LEVEL LogLevel
    {
        get { return currentLogLevel; }

        set
        {
            currentLogLevel = value;
            fxr_plugin.fxrSetLogLevel((int) currentLogLevel);
        }
    }

    private void HandleWindowCreated()
    {
        fxr_plugin.fxrFinishWindowCreation(WindowCreationRequestCompleteCallbackParams.uid, WindowCreationRequestCompleteCallbackParams.windowIndex, OnFxWindowCreated);
    }

    void OnFxWindowCreated(int uid, int windowIndex, int widthPixels, int heightPixels, int formatNative)
    {
        FxRWindow window = FxRWindow.FindWindowWithUID(uid);
        if (window == null)
        {
            window = FxRWindow.CreateNewInParent(transform.parent.gameObject);
        }

        TextureFormat format;
        switch (formatNative)
        {
            case 1:
                format = TextureFormat.RGBA32;
                break;
            case 2:
                format = TextureFormat.BGRA32;
                break;
            case 3:
                format = TextureFormat.ARGB32;
                break;
            case 5:
                format = TextureFormat.RGB24;
                break;
            case 7:
                format = TextureFormat.RGBA4444;
                break;
            case 9:
                format = TextureFormat.RGB565;
                break;
            default:
                format = (TextureFormat) 0;
                break;
        }

        _hackKeepWindowIndex = windowIndex;
        window.WasCreated(windowIndex, widthPixels,
            heightPixels, format);
    }
}