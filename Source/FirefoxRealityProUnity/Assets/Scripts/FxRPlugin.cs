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

public class FxRPlugin
{
    // Delegate instance.
    private FxRPluginLogCallback logCallback = null;
    private GCHandle logCallbackGCH;
    private FxRPluginWindowCreatedCallback windowCreatedCallback = null;
    private GCHandle windowCreatedCallbackGCH;
    private FxRPluginWindowResizedCallback windowResizedCallback = null;
    private GCHandle windowResizedCallbackGCH;

    public void fxrRegisterLogCallback(FxRPluginLogCallback lcb)
    {
        logCallback = lcb; // Set or unset.
        if (lcb != null)
        { // If setting, create the callback stub prior to registering the callback on the native side.
            logCallbackGCH = GCHandle.Alloc(logCallback); // Does not need to be pinned, see http://stackoverflow.com/a/19866119/316487 
        }
        FxRPlugin_pinvoke.fxrRegisterLogCallback(logCallback);
        if (lcb == null)
        { // If unsetting, free the callback stub after deregistering the callback on the native side.
            logCallbackGCH.Free();
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

    public void fxrStartFx(FxRPluginWindowCreatedCallback wccb, FxRPluginWindowResizedCallback wrcb)
    {
        windowCreatedCallback = wccb;
        windowResizedCallback = wrcb;
        // Create the callback stub prior to registering the callback on the native side.
        windowCreatedCallbackGCH = GCHandle.Alloc(windowCreatedCallback); // Does not need to be pinned, see http://stackoverflow.com/a/19866119/316487 
        windowResizedCallbackGCH = GCHandle.Alloc(windowResizedCallback);
        
        FxRPlugin_pinvoke.fxrStartFx(windowCreatedCallback, windowResizedCallback);
    }

    public void fxrStopFx()
    {
        FxRPlugin_pinvoke.fxrStopFx();
        windowCreatedCallback = null;
        windowResizedCallback = null;
        // Free the callback stubs after deregistering the callbacks on the native side.
        windowCreatedCallbackGCH.Free();
        windowResizedCallbackGCH.Free();
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

    public bool fxrGetTextureFormat(int windowIndex, out int width, out int height, out TextureFormat format, out bool mipChain, out bool linear, out IntPtr nativeTexureID)
    {
        int formatNative;
        IntPtr[] nativeTextureIDHandle = new IntPtr[1];
        FxRPlugin_pinvoke.fxrGetWindowTextureFormat(windowIndex, out width, out height, out formatNative, out mipChain, out linear, nativeTextureIDHandle);
        nativeTexureID = nativeTextureIDHandle[0];

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
            //case 4:
            //    format = TextureFormat.ABGR32;
            //    break;
            case 5:
                format = TextureFormat.RGB24;
                break;
            //case 6:
            //    format = TextureFormat.BGR24;
            //    break;
            case 7:
                format = TextureFormat.RGBA4444;
                break;
            //case 8:
            //    format = TextureFormat.RGBA5551;
            //    break;
            case 9:
                format = TextureFormat.RGB565;
                break;
            default:
                format = (TextureFormat)0;
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

    public enum FxRPointerEventID {
        Enter = 0,
        Exit = 1,
        Over = 2,
        Press = 3,
        Release = 4,
        ScrollDiscrete = 5
    };

    public void fxrWindowPointerEvent(int windowIndex, FxRPointerEventID eventID, int windowX, int windowY)
    {
        FxRPlugin_pinvoke.fxrWindowPointerEvent(windowIndex, (int)eventID, windowX, windowY);
    }

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
}
