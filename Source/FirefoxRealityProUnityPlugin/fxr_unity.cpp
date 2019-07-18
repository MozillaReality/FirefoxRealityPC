//
// fxr_unity.cpp
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0.If a copy of the MPL was not distributed with this
// file, You can obtain one at https ://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019- Mozilla, Inc.
//
// Author(s): Philip Lamb
//
// Implementations of plugin interfaces which are invoked from Unity via P/Invoke.
//

#include "fxr_unity_c.h"
#include "fxr_log.h"

#include "FxRWindowDX11.h"
#include "FxRWindowGL.h"
#include <memory>

//
// Unity low-level plugin interface.
//

#include "IUnityInterface.h"
#include "IUnityGraphics.h"

// --------------------------------------------------------------------------
// UnitySetInterfaces

static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;
static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	s_UnityInterfaces = unityInterfaces;
	s_Graphics = unityInterfaces->Get<IUnityGraphics>();
	s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
	// Run OnGraphicsDeviceEvent(initialize) manually on plugin load
	// to not miss the event in case the graphics device is already initialized.
	OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
	s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

static UnityGfxRenderer s_RendererType = kUnityGfxRendererNull;

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
	switch (eventType) {
		case kUnityGfxDeviceEventInitialize:
		{
			s_RendererType = s_Graphics->GetRenderer();
			switch (s_RendererType) {
			case kUnityGfxRendererD3D11:
				FXRLOGi("Using DirectX 11 renderer.\n");
                FxRWindowDX11::init(s_UnityInterfaces);
				break;
			case kUnityGfxRendererOpenGLCore:
				FXRLOGi("Using OpenGL renderer.\n");
				FxRWindowGL::init();
				break;
			default:
				FXRLOGe("Unsupported renderer.\n");
				return;
			}
			break;
		}
		case kUnityGfxDeviceEventShutdown:
		{
			switch (s_RendererType) {
			case kUnityGfxRendererD3D11:
                FxRWindowDX11::finalize();
				break;
			case kUnityGfxRendererOpenGLCore:
				FxRWindowGL::finalize();
				break;
			}
			s_RendererType = kUnityGfxRendererNull;
			break;
		}
	};
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
	// Unknown / unsupported graphics device type? Do nothing
	switch (s_RendererType) {
	case kUnityGfxRendererD3D11:
		break;
	case kUnityGfxRendererOpenGLCore:
		break;
	default:
		FXRLOGe("Unsupported renderer.\n");
		return;
	}

	switch (eventID) {
	case 1:
		fxrRequestWindowUpdate(0, 0.0f);
		break;
	default:
		break;
	}
}


// GetRenderEventFunc, a function we export which is used to get a rendering event callback function.
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
	return OnRenderEvent;
}

//
// FxR plugin implementation.
//


static std::unique_ptr<FxRWindow> gWindow = nullptr;

void fxrRegisterLogCallback(PFN_LOGCALLBACK callback)
{
	fxrLogSetLogger(callback, 1); // 1 -> only callback on same thread, as required e.g. by C# interop.
}

void fxrSetLogLevel(const int logLevel)
{
	if (logLevel >= 0) {
		fxrLogLevel = logLevel;
	}
}

bool fxrGetFxVersion(char *buffer, int length)
{
	if (!buffer) return false;

	if (const char *version = "69.0") { // TODO: replace with method that fetches the actual Firefox version.
		strncpy(buffer, version, length - 1); buffer[length - 1] = '\0';
		return true;
	}
	return false;
}

void fxrKeyEvent(int keyCode)
{
	FXRLOGi("Got keyCode %d.\n", keyCode);
}

void fxrSetOpenVRSessionPtr(void *p)
{
	FXRLOGi("Got OpenVR session ptr %p.\n", p);
}

int fxrGetWindowCount(void)
{
	if (gWindow) return 1;
	return 0;
}

int fxrNewWindowFromTexture(void *nativeTexturePtr, int widthPixels, int heightPixels, int format)
{
    if (s_RendererType != kUnityGfxRendererD3D11 && s_RendererType != kUnityGfxRendererOpenGLCore) {
        FXRLOGe("Unsupported renderer.\n");
        return -1;
    }
   
    FXRLOGi("fxrNewWindowFromTexture got texturePtr %p size %dx%d, format %d.\n", nativeTexturePtr, widthPixels, heightPixels, format);
    FxRWindow::Size size = {widthPixels, heightPixels};
    if (s_RendererType == kUnityGfxRendererD3D11) {
        gWindow = std::make_unique<FxRWindowDX11>(size, nativeTexturePtr, format);
    } else if (s_RendererType == kUnityGfxRendererOpenGLCore) {
        gWindow = std::make_unique<FxRWindowGL>(size, nativeTexturePtr, format);
    }
	return (0);
}

bool fxrCloseWindow(int windowIndex)
{
	if (windowIndex < 0 || windowIndex >= fxrGetWindowCount()) return false;
	
	if (!gWindow) return false;
	gWindow = nullptr;
	return true;
}

bool fxrCloseAllWindows(void)
{
	gWindow = nullptr;
	return true;
}

bool fxrGetWindowTextureFormat(int windowIndex, int *width, int *height, int *format, bool *mipChain, bool *linear, void **nativeTextureID_p)
{
	if (windowIndex < 0 || windowIndex >= fxrGetWindowCount()) return false;

	if (!gWindow) return false;
	FxRWindow::Size size = gWindow->size();
	if (width) *width = size.w;
	if (height) *height = size.h;
	if (format) *format = gWindow->format();
	if (mipChain) *mipChain = false;
	if (linear) *linear = true;
	if (nativeTextureID_p) *nativeTextureID_p = gWindow->getNativePtr();
	return true;
}

bool fxrSetWindowSize(int windowIndex, int width, int height)
{
	if (windowIndex < 0 || windowIndex >= fxrGetWindowCount()) return false;

	if (!gWindow) return false;
	gWindow->setSize({width, height});
	return true;
}

FXR_EXTERN void fxrRequestWindowUpdate(int windowIndex, float timeDelta)
{
	if (windowIndex < 0 || windowIndex >= fxrGetWindowCount()) return;

	if (!gWindow) return;
	gWindow->requestUpdate(timeDelta);
}
