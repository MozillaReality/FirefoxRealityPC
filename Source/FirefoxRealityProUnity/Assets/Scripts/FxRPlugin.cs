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
public delegate void FxRPluginWindowSizeCallback(int windowIndex, int pixelWidth, int pixelHeight);

public class FxRPlugin
{
    // Delegate instance.
    private FxRPluginLogCallback logCallback = null;
    private GCHandle logCallbackGCH;

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

    public void fxrStartFx()
    {
        FxRPlugin_pinvoke.fxrStartFx();
    }

    public void fxrStopFx()
    {
        FxRPlugin_pinvoke.fxrStopFx();
    }

    public void fxrSetResourcesPath(string path)
    {
        FxRPlugin_pinvoke.fxrSetResourcesPath(path);
    }

    public void fxrKeyEvent(int keyCode)
    {
        FxRPlugin_pinvoke.fxrKeyEvent(keyCode);
    }

    public void fxrSetOpenVRSessionPtr(System.IntPtr p)
    {
        FxRPlugin_pinvoke.fxrSetOpenVRSessionPtr(p);
    }

    public int fxrGetWindowCount()
    {
        return FxRPlugin_pinvoke.fxrGetWindowCount();
    }

    public int fxrNewWindowFromTexture(IntPtr nativeTexturePtr, int width, int height, TextureFormat textureFormat)
    {
        int formatNative = 0;
        switch (textureFormat) {
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
                break;
        }
        if (textureFormat == 0) {
            Debug.LogError("Unsupported texture format " + textureFormat);
            return -1;
        }
        return FxRPlugin_pinvoke.fxrNewWindowFromTexture(nativeTexturePtr, width, height, formatNative);
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

    public void fxrRequestWindowUpdate(int windowIndex, float timeDelta)
    {
        //FxRPlugin_pinvoke.fxrRequestWindowUpdate(windowIndex, timeDelta);
        //FxRPlugin_pinvoke.fxrUnitySetParams_RequestWindowUpdate(windowIndex, timeDelta);
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
}
