//
// FxRPlugin_pinvoke.cs
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0.If a copy of the MPL was not distributed with this
// file, You can obtain one at https ://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019- Mozilla, Inc.
//
// Author(s): Philip Lamb
//
// Low-level declarations of plugin interfaces that are exported from the native DLL.
//

using System;
using System.Text;
using System.Runtime.InteropServices;

public static class FxRPlugin_pinvoke
{
    // The name of the external library containing the native functions
    private const string LIBRARY_NAME = "fxr_unity";

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrTriggerFullScreenBeginEvent();

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr GetRenderEventFunc();

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrRegisterLogCallback(FxRPluginLogCallback callback);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrRegisterFullScreenBeginCallback(FxRPluginFullScreenBeginCallback callback);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrRegisterFullScreenEndCallback(FxRPluginFullEndCallback callback);
    
    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrSetLogLevel(int logLevel);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool fxrGetFxVersion([MarshalAs(UnmanagedType.LPStr)]StringBuilder buffer, int length);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrStartFx(FxRPluginWindowCreatedCallback callback, FxRPluginWindowResizedCallback resizedCallback);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrStopFx();

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrSetResourcesPath(string path);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrKeyEvent(int windowIndex, int keyCode);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrWaitForVREvent(int windowIndex, out int eventType, out int eventData1, out int eventData2);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrWindowPointerEvent(int windowIndex, int eventID, int windowX, int windowY);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrSetOpenVRSessionPtr(IntPtr keyCode);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fxrGetWindowCount();

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool fxrRequestNewWindow(int uid, int widthPixelsRequested, int heightPixelsRequested);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrGetWindowTextureFormat(int windowIndex, out int width, out int height, out int format, [MarshalAsAttribute(UnmanagedType.I1)] out bool mipChain, [MarshalAsAttribute(UnmanagedType.I1)] out bool linear, IntPtr[] nativeTextureIDHandle);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool fxrSetWindowUnityTextureID(int windowIndex, IntPtr nativeTexturePtr);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool fxrRequestWindowSizeChange(int windowIndex, int width, int height);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool fxrCloseWindow(int windowIndex);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool fxrCloseAllWindows();

    // Must be called from rendering thread with active rendering context.
    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrRequestWindowUpdate(int windowIndex, float timeDelta);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrSetRenderEventFunc1Params(int windowIndex, float timeDelta);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrSetParamBool(int param, bool flag);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrSetParamInt(int param, int flag);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrSetParamFloat(int param, [MarshalAsAttribute(UnmanagedType.I1)] bool flag);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool fxrGetParamBool(int param);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fxrGetParamInt(int param);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern float fxrGetParamFloat(int param);
}
