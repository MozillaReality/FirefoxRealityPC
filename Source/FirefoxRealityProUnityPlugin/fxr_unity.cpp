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

extern "C" void UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	s_UnityInterfaces = unityInterfaces;
	s_Graphics = unityInterfaces->Get<IUnityGraphics>();
	s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
	// Run OnGraphicsDeviceEvent(initialize) manually on plugin load
	// to not miss the event in case the graphics device is already initialized.
	OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

extern "C" void UnityPluginUnload()
{
	s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

static UnityGfxRenderer s_RendererType = kUnityGfxRendererNull;

static void OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
	switch (eventType) {
		case kUnityGfxDeviceEventInitialize:
		{
			s_RendererType = s_Graphics->GetRenderer();
			//TODO: user initialization code
			break;
		}
		case kUnityGfxDeviceEventShutdown:
		{
			s_RendererType = kUnityGfxRendererNull;
			//TODO: user shutdown code
			break;
		}
		case kUnityGfxDeviceEventBeforeReset:
		{
			//TODO: user Direct3D 9 code
			break;
		}
		case kUnityGfxDeviceEventAfterReset:
		{
			//TODO: user Direct3D 9 code
			break;
		}
	};
}

//
// FxR plugin implementation.
//

static std::unique_ptr<FxRWindowGL> gWindow = nullptr;

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

int fxrNewWindow(void)
{
	// For now, only one window.
	gWindow = std::make_unique<FxRWindowGL>();
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
	if (mipChain) *mipChain = true;
	if (linear) *linear = true;
	if (nativeTextureID_p) *nativeTextureID_p = gWindow->getNativeID();
	return true;
}

bool fxrSetWindowSize(int windowIndex, int width, int height)
{
	if (windowIndex < 0 || windowIndex >= fxrGetWindowCount()) return false;

	if (!gWindow) return false;
	gWindow->setSize({width, height});
	return true;
}

