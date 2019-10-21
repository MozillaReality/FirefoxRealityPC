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

enum {
	FxRVideoProjection_2D = 0,
	FxRVideoProjection_360 = 1,
	FxRVideoProjection_360S = 2, // 360 stereo
	FxRVideoProjection_180 = 3,
	FxRVideoProjection_180LR = 4, // 180 left to right
	FxRVideoProjection_180TB = 5, // 180 top to bottom
	FxRVideoProjection_3D = 6 // 3D side by side
};
//
// FxR custom plugin interface API.
//

typedef void (FXR_CALLBACK *PFN_LOGCALLBACK)(const char* msg);

typedef void (FXR_CALLBACK *PFN_WINDOWCREATEDCALLBACK)(int uidExt, int windowIndex, int pixelWidth, int pixelHeight, int format);
typedef void (FXR_CALLBACK *PFN_WINDOWRESIZEDCALLBACK)(int uidExt, int pixelWidth, int pixelHeight);

typedef void (FXR_CALLBACK *PFN_FULLSCREENBEGINCALLBACK)(int pixelWidth, int pixelHeight, int format, int projection);

typedef void (FXR_CALLBACK *PFN_FULLSCREENENDCALLBACK)();

typedef void (FXR_CALLBACK *PFN_VREVENTCALLBACK)(int uidExt, int eventType, int eventData1, int eventData2);

FXR_EXTERN void fxrTriggerFullScreenBeginEvent();

/**
 * Registers a callback function to use when a message is logged.
 * If the callback is to become invalid, be sure to call this function with NULL
 * first so that the callback is unregistered.
 */
FXR_EXTERN void fxrRegisterLogCallback(PFN_LOGCALLBACK logCcallback);

FXR_EXTERN void fxrRegisterFullScreenBeginCallback(PFN_FULLSCREENBEGINCALLBACK);

FXR_EXTERN void fxrRegisterFullScreenEndCallback(PFN_FULLSCREENENDCALLBACK);

FXR_EXTERN void fxrSetLogLevel(const int logLevel);

/**
 * Gets the Firefox version as a string, such as "69.0".
 * @param buffer	The character buffer to populate
 * @param length	The maximum number of characters to set in buffer
 * @return			true if successful, false if an error occurred
 */
FXR_EXTERN bool fxrGetFxVersion(char *buffer, int length);

FXR_EXTERN void fxrStartFx(PFN_WINDOWCREATEDCALLBACK windowCreatedCallback, PFN_WINDOWRESIZEDCALLBACK windowResizedCallback, PFN_VREVENTCALLBACK vrEventCallback);

FXR_EXTERN void fxrStopFx(void);

// Set the path in which the plugin should look for resources. Should be full filesystem path without trailing slash.
// This should be called early on in the plugin lifecycle, typically from a Unity MonoBehaviour.OnEnable() event.
// Normally this would be the path to Unity's StreamingAssets folder, which holds unprocessed resources for use at runtime.
FXR_EXTERN void fxrSetResourcesPath(const char *path);

FXR_EXTERN void fxrKeyEvent(int windowIndex, int keyCode);

FXR_EXTERN void fxrSetOpenVRSessionPtr(void *p);

FXR_EXTERN int fxrGetWindowCount(void);

FXR_EXTERN bool fxrRequestNewWindow(int uidExt, int widthPixelsRequested, int heightPixelsRequested);

FXR_EXTERN bool fxrGetWindowTextureFormat(int windowIndex, int *width, int *height, int *format, bool *mipChain, bool *linear, void **nativeTextureID_p);

// On Direct3D-like devices pass a pointer to the base texture type (IDirect3DBaseTexture9 on D3D9, ID3D11Resource on D3D11),
// or on OpenGL-like devices pass the texture "name", casting the integer to a pointer.
FXR_EXTERN bool fxrSetWindowUnityTextureID(int windowIndex, void *nativeTexturePtr);

FXR_EXTERN bool fxrRequestWindowSizeChange(int windowIndex, int width, int height);

FXR_EXTERN bool fxrCloseWindow(int windowIndex);

FXR_EXTERN bool fxrCloseAllWindows(void);

// Must be called from rendering thread with active rendering context.
// As an alternative to invoking directly, an equivalent invocation can be invoked via call this sequence:
//     fxrSetRenderEventFunc1Params(windowIndex, timeDelta);
//     (*GetRenderEventFunc())(1);
FXR_EXTERN void fxrRequestWindowUpdate(int windowIndex, float timeDelta);

FXR_EXTERN void fxrSetRenderEventFunc1Params(int windowIndex, float timeDelta);

enum {
	FxRPointerEventID_Enter = 0,
	FxRPointerEventID_Exit = 1,
	FxRPointerEventID_Over = 2,
	FxRPointerEventID_Press = 3,
	FxRPointerEventID_Release = 4,
	FxRPointerEventID_ScrollDiscrete = 5
};

FXR_EXTERN void fxrWindowPointerEvent(int windowIndex, int eventID, int windowX, int windowY);

enum {
	FxRParam_b_CloseNativeWindowOnClose = 0,
	FxRParam_Max
};

FXR_EXTERN void fxrSetParamBool(int param, bool flag);
FXR_EXTERN void fxrSetParamInt(int param, int val);
FXR_EXTERN void fxrSetParamFloat(int param, float val);
FXR_EXTERN bool fxrGetParamBool(int param);
FXR_EXTERN int fxrGetParamInt(int param);
FXR_EXTERN float fxrGetParamFloat(int param);


#ifdef __cplusplus
}
#endif
#endif // !__fxr_unity_c_h__