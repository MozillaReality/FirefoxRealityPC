﻿//
// fxr_unity.cpp
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0.If a copy of the MPL was not distributed with this
// file, You can obtain one at https ://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019- Mozilla, Inc.
//
// Author(s): Philip Lamb, Thomas Moore
//
// Implementations of plugin interfaces which are invoked from Unity via P/Invoke.
//

#include "fxr_unity_c.h"
#include "fxr_log.h"

#include "FxRWindowDX11.h"
#include <memory>
#include <string>
#include <assert.h>
#include <map>

#include "vrhost.h"

#include "openvr.h"

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
static char *s_ResourcesPath = NULL;

// --------------------------------------------------------------------------
// vrhost.dll

// For debugging scenarios, recommended to hard code paths to these files rather than
// copying to StreamingAssets folder, which greatly slows down Unity IDE load and
// generates many .meta files.
//#define USE_HARDCODED_FX_PATHS 1
#ifdef USE_HARDCODED_FX_PATHS
static WCHAR s_pszFxPath[] = L"e:\\src4\\gecko_build_release\\dist\\bin\\firefox.exe";
static WCHAR s_pszVrHostPath[] = L"e:\\src4\\gecko_build_release\\dist\\bin\\vrhost.dll";
static WCHAR s_pszFxProfile[] = L"e:\\src4\\gecko_build_release\\tmp\\profile-default";
#else
static CHAR s_pszFxPath[MAX_PATH] = { 0 };
static WCHAR s_pszVrHostPath[MAX_PATH] = { 0 };
static CHAR s_pszFxProfile[MAX_PATH] = { 0 };
#endif
static HINSTANCE m_hVRHost = nullptr;
static PFN_CREATEVRWINDOW m_pfnCreateVRWindow = nullptr;
static PFN_SENDUIMSG m_pfnSendUIMessage = nullptr;
static PFN_CLOSEVRWINDOW m_pfnCloseVRWindow = nullptr;
static PFN_WAITFORVREVENT m_pfnWaitForVREvent = nullptr;
static PFN_WINDOWCREATIONREQUESTCOMPLETED m_windowCreationRequestCompletedCallback = nullptr;
static PFN_WINDOWRESIZEDCALLBACK m_windowResizedCallback = nullptr;
static PFN_FULLSCREENBEGINCALLBACK m_fullScreenBeginCallback = nullptr;
static PFN_FULLSCREENENDCALLBACK m_fullScreenEndCallback = nullptr;
static PFN_VREVENTCALLBACK m_vrEventCallback = nullptr;
static PFN_SENDVRTELEMETRY m_pfnSendVRTelemetry = nullptr;
static PROCESS_INFORMATION procInfoFx = { 0 };

static std::map<int, std::unique_ptr<FxRWindow>> s_windows;
static int s_windowIndexNext = 1;

static bool s_param_CloseNativeWindowOnClose = true;

#define OPENVR_API_LIBRARY_NAME "openvr_api"

// --------------------------------------------------------------------------

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
                FxRWindowDX11::initDevice(s_UnityInterfaces);
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
                FxRWindowDX11::finalizeDevice();
				break;
			}
			s_RendererType = kUnityGfxRendererNull;
			break;
		}
	};
}

static int s_RenderEventFunc1Param_windowIndex = 0;
static float s_RenderEventFunc1Param_timeDelta = 0.0f;

void fxrSetRenderEventFunc1Params(int windowIndex, float timeDelta)
{
	s_RenderEventFunc1Param_windowIndex = windowIndex;
	s_RenderEventFunc1Param_timeDelta = timeDelta;
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
	// Unknown / unsupported graphics device type? Do nothing
	switch (s_RendererType) {
	case kUnityGfxRendererD3D11:
		break;
	default:
		FXRLOGe("Unsupported renderer.\n");
		return;
	}

	switch (eventID) {
	case 1:
		fxrRequestWindowUpdate(s_RenderEventFunc1Param_windowIndex, s_RenderEventFunc1Param_timeDelta);
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

void fxrRegisterFullScreenBeginCallback(PFN_FULLSCREENBEGINCALLBACK fullScreenBeginCallback)
{
	m_fullScreenBeginCallback = fullScreenBeginCallback;
}

void fxrRegisterFullScreenEndCallback(PFN_FULLSCREENENDCALLBACK fullScreenEndCallback)
{
	m_fullScreenEndCallback = fullScreenEndCallback;
}

void fxrTriggerFullScreenBeginEvent()
{
	FXRLOGi("Triggering full screen begin.\n");
	auto window_iter = s_windows.find(1);
	if (window_iter != s_windows.end()) {
		FxRWindow::Size size = window_iter->second->size();
		m_fullScreenBeginCallback(size.w, size.h, window_iter->second->format(), FxRVideoProjection_360);
		FXRLOGi("Triggered full screen begin.\n");
	}
}

void fxrRegisterLogCallback(PFN_LOGCALLBACK logCallback)
{
	fxrLogSetLogger(logCallback, 1); // 1 -> only callback on same thread, as required e.g. by C# interop.
}

void fxrSetLogLevel(const int logLevel)
{
	if (logLevel >= 0) {
		fxrLogLevel = logLevel;
	}
}

bool fxrGetVersion(char *buffer, int length)
{
	if (!buffer) return false;

	const char *version = FXR_PLUGIN_VERSION;
	strncpy(buffer, version, length - 1); buffer[length - 1] = '\0';
	return true;
}

void fxrSetResourcesPath(const char *path)
{
	free(s_ResourcesPath);
	s_ResourcesPath = NULL;
	if (path && path[0]) {
		s_ResourcesPath = strdup(path);
		FXRLOGi("Resources path is '%s'.\n", s_ResourcesPath);
	}
}

void fxrStartFx(PFN_WINDOWCREATIONREQUESTCOMPLETED windowCreationRequestCompletedCallback, PFN_WINDOWRESIZEDCALLBACK windowResizedCallback, PFN_VREVENTCALLBACK vrEventCallback)
{
	assert(m_hVRHost == nullptr);

	int err;
#ifndef USE_HARDCODED_FX_PATHS
	err = sprintf_s(s_pszFxPath, ARRAYSIZE(s_pszFxPath), "%s/%s", s_ResourcesPath, "firefox/");
	assert(err > 0);
	err = swprintf_s(s_pszVrHostPath, ARRAYSIZE(s_pszVrHostPath), L"%S/%S", s_ResourcesPath, "firefox/vrhost.dll");
	assert(err > 0);
	err = sprintf_s(s_pszFxProfile, ARRAYSIZE(s_pszFxProfile), "%s/%s", s_ResourcesPath, "fxr-profile");
	assert(err > 0);
#endif

	m_hVRHost = ::LoadLibrary(s_pszVrHostPath);
	assert(m_hVRHost != nullptr);

	m_pfnCreateVRWindow = (PFN_CREATEVRWINDOW)::GetProcAddress(m_hVRHost, "CreateVRWindow");
	m_pfnSendUIMessage = (PFN_SENDUIMSG)::GetProcAddress(m_hVRHost, "SendUIMessageToVRWindow");
	m_pfnCloseVRWindow = (PFN_CLOSEVRWINDOW)::GetProcAddress(m_hVRHost, "CloseVRWindow");
	m_pfnWaitForVREvent = (PFN_WAITFORVREVENT)::GetProcAddress(m_hVRHost, "WaitForVREvent");
	m_pfnSendVRTelemetry = (PFN_SENDVRTELEMETRY)::GetProcAddress(m_hVRHost, "SendVRTelemetry");

	m_windowCreationRequestCompletedCallback = windowCreationRequestCompletedCallback;
	m_windowResizedCallback = windowResizedCallback;
	m_vrEventCallback = vrEventCallback;
}

void fxrStopFx(void)
{
	m_windowCreationRequestCompletedCallback = nullptr;
	m_windowResizedCallback = nullptr;
	m_vrEventCallback = nullptr;

	::FreeLibrary(m_hVRHost);
	m_hVRHost = nullptr;
}

void fxrKeyEvent(int windowIndex, int keyCode)
{
	FXRLOGd("Got keyCode %d.\n", keyCode);

	auto window_iter = s_windows.find(windowIndex);
	if (window_iter != s_windows.end()) {
		window_iter->second->keyPress(keyCode);
	}
}

void fxrSetOpenVRSessionPtr(void *p)
{
	FXRLOGd("Got OpenVR session ptr %p.\n", p);
	HMODULE hOpenVR = GetModuleHandleA(OPENVR_API_LIBRARY_NAME);
	if (hOpenVR == NULL) {
		FXRLOGw("Couldn't access " OPENVR_API_LIBRARY_NAME ".dll.\n");
	} else {
		char path[MAX_PATH];
		GetModuleFileNameA(hOpenVR, path, sizeof(path));
		FXRLOGi("Got OpenVR library at path '%s'.\n", path);
		::vr::IVRSystem *pOpenVR = (vr::IVRSystem *)p;
		uint32_t w, h;
		pOpenVR->GetRecommendedRenderTargetSize(&w, &h);
		FXRLOGi("OpenVR recommends render target size %dx%d.\n", w, h);
	}
}

int fxrGetWindowCount(void)
{
	return (int)s_windows.size();
}

bool fxrRequestNewWindow(int uidExt, int widthPixelsRequested, int heightPixelsRequested)
{
	std::unique_ptr<FxRWindow> window;
	if (s_RendererType == kUnityGfxRendererD3D11) {
		window = std::make_unique<FxRWindowDX11>(s_windowIndexNext++, uidExt, s_pszFxPath, s_pszFxProfile, m_pfnCreateVRWindow, m_pfnSendUIMessage, m_pfnWaitForVREvent, s_param_CloseNativeWindowOnClose ? m_pfnCloseVRWindow : nullptr, m_vrEventCallback, m_pfnSendVRTelemetry);
	} else {
		FXRLOGe("Cannot create window. Unknown render type detected. Only D3D11 is supported.\n");
	}
	auto inserted = s_windows.emplace(window->uid(), move(window));
	if (!inserted.second || !inserted.first->second->init(m_windowCreationRequestCompletedCallback)) {
		FXRLOGe("Error initing window.\n");
		return false;
	}
	return true;
}


void fxrFinishWindowCreation(int uidExt, int windowIndex, PFN_WINDOWCREATEDCALLBACK pfnWindowCreatedCallback)
{
	auto window_iter = s_windows.find(windowIndex);
	if (window_iter != s_windows.end()) {
		window_iter->second->FinishWindowCreation(pfnWindowCreatedCallback);
	}
}

bool fxrSetWindowUnityTextureID(int windowIndex, void *nativeTexturePtr)
{
    if (s_RendererType != kUnityGfxRendererD3D11) {
        FXRLOGe("Unsupported renderer.\n");
        return false;
    }
   
	auto window_iter = s_windows.find(windowIndex);
	if (window_iter == s_windows.end()) {
		FXRLOGe("Requested to set unity texture ID for non-existent window with index %d.\n", windowIndex);
		return false;
	}

	window_iter->second->setNativePtr(nativeTexturePtr);
	FXRLOGi("fxrSetWindowUnityTextureID set texturePtr %p.\n", nativeTexturePtr);
	return true;
}

void fxrSetParamBool(int param, bool flag)
{
	switch (param) {
		case FxRParam_b_CloseNativeWindowOnClose:
			s_param_CloseNativeWindowOnClose = flag;
			break;
		default:
			break;
	}
}

void fxrSetParamInt(int param, int val)
{
}

void fxrSetParamFloat(int param, float val)
{
}

bool fxrGetParamBool(int param)
{
	switch (param) {
		case FxRParam_b_CloseNativeWindowOnClose:
			return s_param_CloseNativeWindowOnClose;
			break;
		default:
			break;
	}
	return false;
}

int fxrGetParamInt(int param)
{
	return 0;
}

float fxrGetParamFloat(int param)
{
	return 0.0f;
}

bool fxrCloseWindow(int windowIndex)
{
	auto window_iter = s_windows.find(windowIndex);
	if (window_iter == s_windows.end()) return false;
	
	window_iter->second->CloseVRWindow();	
	s_windows.erase(window_iter);
	return true;
}

bool fxrCloseAllWindows(void)
{
	s_windows.clear();
	return true;
}

bool fxrGetWindowTextureFormat(int windowIndex, int *width, int *height, int *format, bool *mipChain, bool *linear, void **nativeTextureID_p)
{
	auto window_iter = s_windows.find(windowIndex);
	if (window_iter == s_windows.end()) return false;

	FxRWindow::Size size = window_iter->second->size();
	if (width) *width = size.w;
	if (height) *height = size.h;
	if (format) *format = window_iter->second->format();
	if (mipChain) *mipChain = false;
	if (linear) *linear = true;
	if (nativeTextureID_p) *nativeTextureID_p = window_iter->second->nativePtr();
	return true;
}

uint64_t fxrGetBufferSizeForTextureFormat(int width, int height, int format)
{
	if (!width || !height) return 0;
	int Bpp;
	switch (format) {
		case FxRTextureFormat_BGRA32:
		case FxRTextureFormat_RGBA32:
		case FxRTextureFormat_ABGR32:
		case FxRTextureFormat_ARGB32:
			Bpp = 4;
			break;
		case FxRTextureFormat_BGR24:
		case FxRTextureFormat_RGB24:
			Bpp = 3;
			break;
		case FxRTextureFormat_RGB565:
		case FxRTextureFormat_RGBA5551:
		case FxRTextureFormat_RGBA4444:
			Bpp = 2;
			break;
		default:
			Bpp = 0;
	}
	return width * height * Bpp;
}

bool fxrRequestWindowSizeChange(int windowIndex, int width, int height)
{
	auto window_iter = s_windows.find(windowIndex);
	if (window_iter == s_windows.end()) return false;
	
	window_iter->second->setSize({ width, height });

	if (m_windowResizedCallback)
	{
		// Once window resize request has completed, get the size that actually was set, and call back to Unity
		FxRWindow::Size size = window_iter->second->size();
		(*m_windowResizedCallback)(window_iter->second->uidExt(), size.w, size.h);
	}
	// TODO: Return true once above method implemented...
	return false;
}

void fxrRequestWindowUpdate(int windowIndex, float timeDelta)
{
	auto window_iter = s_windows.find(windowIndex);
	if (window_iter == s_windows.end()) {
		FXRLOGe("Requested update for non-existent window with index %d.\n", windowIndex);
		return;
	}
	window_iter->second->requestUpdate(timeDelta);
}

void fxrWindowPointerEvent(int windowIndex, int eventID, int windowX, int windowY)
{
	auto window_iter = s_windows.find(windowIndex);
	if (window_iter == s_windows.end()) return;

	switch (eventID) {
	case FxRPointerEventID_Enter:
		window_iter->second->pointerEnter();
		break;
	case FxRPointerEventID_Exit:
		window_iter->second->pointerExit();
		break;
	case FxRPointerEventID_Over:
		window_iter->second->pointerOver(windowX, windowY);
		break;
	case FxRPointerEventID_Press:
		window_iter->second->pointerPress(windowX, windowY);
		break;
	case FxRPointerEventID_Release:
		window_iter->second->pointerRelease(windowX, windowY);
		break;
	case FxRPointerEventID_ScrollDiscrete:
		window_iter->second->pointerScrollDiscrete(windowX, windowY);
		break;
	default:
		break;
	}
}
