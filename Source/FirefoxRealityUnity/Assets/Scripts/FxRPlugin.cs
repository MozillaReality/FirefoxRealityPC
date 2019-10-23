//
// FxRPlugin.cs
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0.If a copy of the MPL was not distributed with this
// file, You can obtain one at https ://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019- Mozilla, Inc.
//
// Author(s): Philip Lamb
//
// Main declarations of plugin interfaces, including any managed-to-native wrappers.
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

// Delegate type declaration for log callback.
public delegate void FxRPluginLogCallback([MarshalAs(UnmanagedType.LPStr)] string msg);

// Delegate type declaration for window size callback.
public delegate void FxRPluginWindowCreatedCallback(int uid, int windowIndex, int pixelWidth, int pixelHeight, int format);
public delegate void FxRPluginWindowResizedCallback(int uid, int pixelWidth, int pixelHeight);
public delegate void FxRPluginVREventCallback(int uid, int eventType, int eventData1, int eventData2);


// Delegate type declarations for full screen video callbacks
public delegate void FxRPluginFullScreenBeginCallback(int pixelWidth, int pixelHeight, int format, int projection);
public delegate void FxRPluginFullEndCallback();

public class FxRPlugin
{
    // Delegate instance.
    private FxRPluginLogCallback logCallback = null;
    private GCHandle logCallbackGCH;
    private FxRPluginWindowCreatedCallback windowCreatedCallback = null;
    private GCHandle windowCreatedCallbackGCH;

    private FxRPluginWindowResizedCallback windowResizedCallback = null;
    private GCHandle windowResizedCallbackGCH;

    private FxRPluginVREventCallback vrEventCallback = null;
    private GCHandle vrEventCallbackGCH;

    private FxRPluginFullScreenBeginCallback fullScreenBeginCallback = null;
    private GCHandle fullScreenBeginCallbackGCH;
    private FxRPluginFullEndCallback fullScreenEndCallback = null;
    private GCHandle fullScreenEndCallbackGCH;

    public void fxrRegisterLogCallback(FxRPluginLogCallback lcb)
    {
        logCallback = lcb; // Set or unset.
        if (lcb != null)
        {
            // If setting, create the callback stub prior to registering the callback on the native side.
            logCallbackGCH =
                GCHandle.Alloc(
                    logCallback); // Does not need to be pinned, see http://stackoverflow.com/a/19866119/316487 
        }

        FxRPlugin_pinvoke.fxrRegisterLogCallback(logCallback);
        if (lcb == null)
        {
            // If unsetting, free the callback stub after deregistering the callback on the native side.
            logCallbackGCH.Free();
        }
    }

    public void fxrRegisterFullScreenBeginCallback(FxRPluginFullScreenBeginCallback fsbc)
    {
        fullScreenBeginCallback = fsbc;
        if (fsbc != null)
        {
            fullScreenBeginCallbackGCH = GCHandle.Alloc(fullScreenBeginCallback);
        }

        FxRPlugin_pinvoke.fxrRegisterFullScreenBeginCallback(fullScreenBeginCallback);
        if (fsbc == null)
        {
            fullScreenBeginCallbackGCH.Free();
        }
    }

    public void fxrRegisterFullScreenEndCallback(FxRPluginFullEndCallback fsec)
    {
        fullScreenEndCallback = fsec;
        if (fsec != null)
        {
            fullScreenEndCallbackGCH = GCHandle.Alloc(fullScreenEndCallback);
        }

        FxRPlugin_pinvoke.fxrRegisterFullScreenEndCallback(fullScreenEndCallback);
        if (fsec == null)
        {
            fullScreenEndCallbackGCH.Free();
        }
    }

    public void fxrSetLogLevel(int logLevel)
    {
        FxRPlugin_pinvoke.fxrSetLogLevel(logLevel);
    }

    public string fxrGetFxVersion()
    {
        StringBuilder sb = new StringBuilder(128);
        bool ok = FxRPlugin_pinvoke.fxrGetFxVersion(sb, sb.Capacity);
        if (ok) return sb.ToString();
        else return "";
    }

    public void fxrStartFx(FxRPluginWindowCreatedCallback wccb, FxRPluginWindowResizedCallback wrcb, FxRPluginVREventCallback vrecb)
    {
        windowCreatedCallback = wccb;
        windowResizedCallback = wrcb;
        vrEventCallback = vrecb;
        
        // Create the callback stub prior to registering the callback on the native side.
        windowCreatedCallbackGCH =
            GCHandle.Alloc(
                windowCreatedCallback); // Does not need to be pinned, see http://stackoverflow.com/a/19866119/316487 
        windowResizedCallbackGCH = GCHandle.Alloc(windowResizedCallback);
        vrEventCallbackGCH = GCHandle.Alloc(vrEventCallback);
        
        FxRPlugin_pinvoke.fxrStartFx(windowCreatedCallback, windowResizedCallback, vrEventCallback);
    }

    public void fxrStopFx()
    {
        FxRPlugin_pinvoke.fxrStopFx();
        windowCreatedCallback = null;
        windowResizedCallback = null;
        vrEventCallback = null;
        
        // Free the callback stubs after deregistering the callbacks on the native side.
        windowCreatedCallbackGCH.Free();
        windowResizedCallbackGCH.Free();
        vrEventCallbackGCH.Free();
    }

    public void fxrSetResourcesPath(string path)
    {
        FxRPlugin_pinvoke.fxrSetResourcesPath(path);
    }

    public void fxrKeyEvent(int windowIndex, int keyCode)
    {
        FxRPlugin_pinvoke.fxrKeyEvent(windowIndex, keyCode);
    }

    public void fxrSetOpenVRSessionPtr(System.IntPtr p)
    {
        FxRPlugin_pinvoke.fxrSetOpenVRSessionPtr(p);
    }

    public int fxrGetWindowCount()
    {
        return FxRPlugin_pinvoke.fxrGetWindowCount();
    }

    public bool fxrRequestNewWindow(int uid, int widthPixelsRequested, int heightPixelsRequested)
    {
        return FxRPlugin_pinvoke.fxrRequestNewWindow(uid, widthPixelsRequested, heightPixelsRequested);
    }

    public bool fxrRequestWindowSizeChange(int windowIndex, int widthPixelsRequested, int heightPixelsRequested)
    {
        return FxRPlugin_pinvoke.fxrRequestWindowSizeChange(windowIndex, widthPixelsRequested, heightPixelsRequested);
    }

    public TextureFormat NativeFormatToTextureFormat(int formatNative)
    {
        switch (formatNative)
        {
            case 1:
                return TextureFormat.RGBA32;
            case 2:
                return TextureFormat.BGRA32;
            case 3:
                return TextureFormat.ARGB32;
            //case 4:
            //    format = TextureFormat.ABGR32;
            case 5:
                return TextureFormat.RGB24;
            //case 6:
            //    format = TextureFormat.BGR24;
            case 7:
                return TextureFormat.RGBA4444;
            //case 8:
            //    format = TextureFormat.RGBA5551;
            case 9:
                return TextureFormat.RGB565;
            default:
                return (TextureFormat) 0;
        }
    }

    public bool fxrGetTextureFormat(int windowIndex, out int width, out int height, out TextureFormat format,
        out bool mipChain, out bool linear, out IntPtr nativeTexureID)
    {
        int formatNative;
        IntPtr[] nativeTextureIDHandle = new IntPtr[1];
        FxRPlugin_pinvoke.fxrGetWindowTextureFormat(windowIndex, out width, out height, out formatNative, out mipChain,
            out linear, nativeTextureIDHandle);
        nativeTexureID = nativeTextureIDHandle[0];

        format = NativeFormatToTextureFormat(formatNative);
        if (format == (TextureFormat) 0)
        {
            return false;
        }

        return true;
    }


    public bool fxrSetWindowUnityTextureID(int windowIndex, IntPtr nativeTexturePtr)
    {
        return FxRPlugin_pinvoke.fxrSetWindowUnityTextureID(windowIndex, nativeTexturePtr);
    }

    public void fxrRequestWindowUpdate(int windowIndex, float timeDelta)
    {
        //FxRPlugin_pinvoke.fxrRequestWindowUpdate(windowIndex, timeDelta);
        FxRPlugin_pinvoke.fxrSetRenderEventFunc1Params(windowIndex, timeDelta);
        GL.IssuePluginEvent(FxRPlugin_pinvoke.GetRenderEventFunc(), 1);
    }

    public enum FxRPointerEventID
    {
        Enter = 0,
        Exit = 1,
        Over = 2,
        Press = 3,
        Release = 4,
        ScrollDiscrete = 5
    };

    public void fxrWindowPointerEvent(int windowIndex, FxRPointerEventID eventID, int windowX, int windowY)
    {
        FxRPlugin_pinvoke.fxrWindowPointerEvent(windowIndex, (int) eventID, windowX, windowY);
    }

    public enum FxREventType
    {
        None = 0,
        IME = 1,
        Total = 2
    };

    public enum FxRIMEState
    {
        Blur = 0,
        Focus = 1
    };

    public bool fxrCloseWindow(int windowIndex)
    {
        return FxRPlugin_pinvoke.fxrCloseWindow(windowIndex);
    }

    public bool fxrCloseAllWindows()
    {
        return FxRPlugin_pinvoke.fxrCloseAllWindows();
    }

    public enum FxRParam {
        b_CloseNativeWindowOnClose = 0,
        Max
    };

    public void fxrSetParamBool(FxRParam param, bool flag)
    {
        FxRPlugin_pinvoke.fxrSetParamBool((int)param, flag);
    }

    public void fxrSetParamInt(FxRParam param, int val)
    {
        FxRPlugin_pinvoke.fxrSetParamInt((int)param, val);
    }

    public void fxrSetParamFloat(FxRParam param, bool val)
    {
        FxRPlugin_pinvoke.fxrSetParamFloat((int)param, val);
    }

    public bool fxrGetParamBool(FxRParam param)
    {
        return FxRPlugin_pinvoke.fxrGetParamBool((int)param);
    }

    public int fxrGetParamInt(FxRParam param)
    {
        return FxRPlugin_pinvoke.fxrGetParamInt((int)param);
    }

    public float fxrGetParamFloat(FxRParam param)
    {
        return FxRPlugin_pinvoke.fxrGetParamFloat((int)param);
    }

    // Test method to fake notification coming from browser that we're entering full screen video
    public void fxrTriggerFullScreenBeginEvent()
    {
        FxRPlugin_pinvoke.fxrTriggerFullScreenBeginEvent();
    }

}
