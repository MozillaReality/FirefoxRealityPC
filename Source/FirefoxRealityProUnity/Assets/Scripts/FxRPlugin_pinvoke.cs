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
    public static extern void fxrRegisterLogCallback(FxRPluginLogCallback callback);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrSetLogLevel(int logLevel);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool fxrGetFxVersion([MarshalAs(UnmanagedType.LPStr)]StringBuilder buffer, int length);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrKeyEvent(int keyCode);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrSetOpenVRSessionPtr(IntPtr keyCode);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fxrGetWindowCount();

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern int fxrNewWindow();

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool fxrCloseWindow(int windowIndex);

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool fxrCloseAllWindows();

    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void fxrGetWindowTextureFormat(int windowIndex, out int width, out int height, out int format, [MarshalAsAttribute(UnmanagedType.I1)] out bool mipChain, [MarshalAsAttribute(UnmanagedType.I1)] out bool linear, IntPtr[] nativeTextureIDHandle);

    // Must be followed by a call to fxrGetWindowTextureFormat to check for updated format (including nativeTexureID).
    [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAsAttribute(UnmanagedType.I1)]
    public static extern bool fxrSetWindowSize(int windowIndex, int width, int height);
    
}
