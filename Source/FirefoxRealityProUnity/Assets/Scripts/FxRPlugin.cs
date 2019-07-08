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

    public void fxRSetLogLevel(int logLevel)
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
            case 8:
                format = TextureFormat.RGB565;
                break;
            default:
                format = (TextureFormat)0;
                return false;
        }
        return true;
    }
}
