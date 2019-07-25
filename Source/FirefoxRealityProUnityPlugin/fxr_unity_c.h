//
// fxr_unity_c.h
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0.If a copy of the MPL was not distributed with this
// file, You can obtain one at https ://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019- Mozilla, Inc.
//
// Author(s): Philip Lamb
//
// Declarations of plugin interfaces which are invoked from Unity via P/Invoke.
//


#ifndef __fxr_unity_c_h__
#define __fxr_unity_c_h__

#include <stdint.h>
#include <stdbool.h>

// Which Unity platform we are on?
// UNITY_WIN - Windows (regular win32)
// UNITY_OSX - Mac OS X
// UNITY_LINUX - Linux
// UNITY_IPHONE - iOS
// UNITY_ANDROID - Android
// UNITY_METRO - WSA or UWP
// UNITY_WEBGL - WebGL
#if _MSC_VER
#define UNITY_WIN 1
#elif defined(__APPLE__)
#if defined(__arm__) || defined(__arm64__)
#define UNITY_IPHONE 1
#else
#define UNITY_OSX 1
#endif
#elif defined(__ANDROID__)
#define UNITY_ANDROID 1
#elif defined(UNITY_METRO) || defined(UNITY_LINUX) || defined(UNITY_WEBGL)
	// these are defined externally
#elif defined(__EMSCRIPTEN__)
	// this is already defined in Unity 5.6
#define UNITY_WEBGL 1
#else
#error "Unknown platform!"
#endif
// Which Unity graphics device APIs we possibly support?
#if UNITY_METRO
#define SUPPORT_D3D11 1
#if WINDOWS_UWP
#define SUPPORT_D3D12 1
#endif
#elif UNITY_WIN
#define SUPPORT_D3D11 1 // comment this out if you don't have D3D11 header/library files
#define SUPPORT_D3D12 0 //@TODO: enable by default? comment this out if you don't have D3D12 header/library files
#define SUPPORT_OPENGL_UNIFIED 1
#define SUPPORT_OPENGL_CORE 1
#define SUPPORT_VULKAN 0 // Requires Vulkan SDK to be installed
#elif UNITY_IPHONE || UNITY_ANDROID || UNITY_WEBGL
#ifndef SUPPORT_OPENGL_ES
#define SUPPORT_OPENGL_ES 1
#endif
#define SUPPORT_OPENGL_UNIFIED SUPPORT_OPENGL_ES
#ifndef SUPPORT_VULKAN
#define SUPPORT_VULKAN 0
#endif
#elif UNITY_OSX || UNITY_LINUX
#define SUPPORT_OPENGL_UNIFIED 1
#define SUPPORT_OPENGL_CORE 1
#endif
#if UNITY_IPHONE || UNITY_OSX
#define SUPPORT_METAL 1
#endif

// FxR defines.

#ifdef _WIN32
#  ifdef FXR_UNITY_STATIC
#    define FXR_EXTERN
#  else
#    ifdef FXR_UNITY_EXPORTS
#      define FXR_EXTERN __declspec(dllexport)
#    else
#      define FXR_EXTERN __declspec(dllimport)
#    endif
#  endif
#  define FXR_CALLBACK __stdcall
#else
#  define FXR_EXTERN
#  define FXR_CALLBACK
#endif

#ifdef __cplusplus
extern "C" {
#endif

enum  {
	FxRTextureFormat_Invalid = 0,
	FxRTextureFormat_RGBA32 = 1,
	FxRTextureFormat_BGRA32 = 2,
	FxRTextureFormat_ARGB32 = 3,
	FxRTextureFormat_ABGR32 = 4,
	FxRTextureFormat_RGB24 = 5,
	FxRTextureFormat_BGR24 = 6,
	FxRTextureFormat_RGBA4444 = 7,
	FxRTextureFormat_RGBA5551 = 8,
	FxRTextureFormat_RGB565 = 9
};

//
// FxR custom plugin interface API.
//

typedef void (FXR_CALLBACK *PFN_LOGCALLBACK)(const char* msg);

/**
 * Registers a callback function to use when a message is logged.
 * If the callback is to become invalid, be sure to call this function with NULL
 * first so that the callback is unregistered.
 */
FXR_EXTERN void fxrRegisterLogCallback(PFN_LOGCALLBACK callback);

FXR_EXTERN void fxrSetLogLevel(const int logLevel);

/**
 * Gets the Firefox version as a string, such as "69.0".
 * @param buffer	The character buffer to populate
 * @param length	The maximum number of characters to set in buffer
 * @return			true if successful, false if an error occurred
 */
FXR_EXTERN bool fxrGetFxVersion(char *buffer, int length);

// Set the path in which the plugin should look for resources. Should be full filesystem path without trailing slash.
// This should be called early on in the plugin lifecycle, typically from a Unity MonoBehaviour.OnEnable() event.
// Normally this would be the path to Unity's StreamingAssets folder, which holds unprocessed resources for use at runtime.
FXR_EXTERN void fxrSetResourcesPath(const char *path);

FXR_EXTERN void fxrKeyEvent(int keyCode);

FXR_EXTERN void fxrSetOpenVRSessionPtr(void *p);

FXR_EXTERN int fxrGetWindowCount(void);

// Pass NULL nativeTexturePtr to allocate the texture on the native side, otherwise uses the passed-in texture.
// On Direct3D-like devices pass a pointer to the base texture type (IDirect3DBaseTexture9 on D3D9, ID3D11Resource on D3D11),
// or on OpenGL-like devices pass the texture "name", casting the integer to a pointer.
// Returns windowIndex.
// Must be called from rendering thread with active rendering context.
FXR_EXTERN int fxrNewWindowFromTexture(void *nativeTexturePtr, int widthPixels, int heightPixels, int format);

FXR_EXTERN bool fxrCloseWindow(int windowIndex);

FXR_EXTERN bool fxrCloseAllWindows(void);

FXR_EXTERN bool fxrGetWindowTextureFormat(int windowIndex, int *width, int *height, int *format, bool *mipChain, bool *linear, void **nativeTextureID_p);

// Must be called from rendering thread with active rendering context.
// Must be followed by a call to fxrGetWindowTextureFormat to check for updated format (including nativeTexureID).
FXR_EXTERN bool fxrSetWindowSize(int windowIndex, int width, int height);

FXR_EXTERN void fxrRequestWindowUpdate(int windowIndex, float timeDelta);

enum {
	FxRPointerEventID_Enter = 0,
	FxRPointerEventID_Exit = 1,
	FxRPointerEventID_Over = 2,
	FxRPointerEventID_Press = 3,
	FxRPointerEventID_Release = 4,
	FxRPointerEventID_ScrollDiscrete = 5
};

FXR_EXTERN void fxrWindowPointerEvent(int windowIndex, int eventID, int windowX, int windowY);

#ifdef __cplusplus
}
#endif
#endif // !__fxr_unity_c_h__