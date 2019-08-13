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
#include <string>
#include <assert.h>

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

typedef void(*PFN_CREATEVRWINDOW)(UINT* windowId, HANDLE* hTex, HANDLE* hEvt, uint64_t* width, uint64_t* height);
typedef void(*PFN_CLOSEVRWINDOW)(UINT nVRWindow);

// For debugging scenarios, recommended to hard code paths to these files rather than
// copying to StreamingAssets folder, which greatly slows down Unity IDE load and
// generates many .meta files.
//#define USE_HARDCODED_FX_PATHS 1
#ifdef USE_HARDCODED_FX_PATHS
static WCHAR s_pszFxPath[] = L"e:\\src4\\gecko_build_release\\dist\\bin\\firefox.exe";
static WCHAR s_pszVrHostPath[] = L"e:\\src4\\gecko_build_release\\dist\\bin\\vrhost.dll";
static WCHAR s_pszFxProfile[] = L"e:\\src4\\gecko_build_release\\tmp\\profile-default";
#else
static WCHAR s_pszFxPath[MAX_PATH] = { 0 };
static WCHAR s_pszVrHostPath[MAX_PATH] = { 0 };
static WCHAR s_pszFxProfile[MAX_PATH] = { 0 };
#endif
static HANDLE s_hThreadFxWin = nullptr;

// vrhost.dll Members
static HINSTANCE m_hVRHost = nullptr;
static PFN_SENDUIMESSAGE m_pfnSendUIMessage = nullptr;

// Window/Process State for Host and Firefox
static HANDLE m_fxTexHandle = nullptr;
static PROCESS_INFORMATION procInfoFx = { 0 };
static HWND   m_hwndHost = nullptr;
static UINT   m_vrWin = 0;
static HANDLE m_hSignal = nullptr;

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

void fxrSetResourcesPath(const char *path)
{
	free(s_ResourcesPath);
	s_ResourcesPath = NULL;
	if (path && path[0]) {
		s_ResourcesPath = strdup(path);
		FXRLOGi("Resources path is '%s'.\n", s_ResourcesPath);
	}
}

static DWORD fxrStartFxCreateVRWindow(_In_ LPVOID lpParameter) {
	//foo* pFoo = static_cast<foo*>(lpParameter);

	PFN_CREATEVRWINDOW lpfnCreate = (PFN_CREATEVRWINDOW)::GetProcAddress(m_hVRHost, "CreateVRWindow");
	uint64_t width;
	uint64_t height;

	lpfnCreate(&m_vrWin, &m_fxTexHandle, &m_hSignal, &width, &height);

	// TODO: Move FxRWindow instantiation to here.

	::ExitThread(0);
}

void fxrStartFx(void)
{
	assert(s_hThreadFxWin == nullptr);
	assert(m_hVRHost == nullptr);

	int err;
#ifndef USE_HARDCODED_FX_PATHS
	err = swprintf_s(s_pszFxPath, ARRAYSIZE(s_pszFxPath), L"%S/%S", s_ResourcesPath, "fxbin/firefox.exe");
	assert(err > 0);
	err = swprintf_s(s_pszVrHostPath, ARRAYSIZE(s_pszVrHostPath), L"%S/%S", s_ResourcesPath, "fxbin/vrhost.dll");
	assert(err > 0);
	err = swprintf_s(s_pszFxProfile, ARRAYSIZE(s_pszFxProfile), L"%S/%S", s_ResourcesPath, "fxbin/fxr-profile");
	assert(err > 0);
#endif

	m_hVRHost = ::LoadLibrary(s_pszVrHostPath);
	assert(m_hVRHost != nullptr);

	m_pfnSendUIMessage = (PFN_SENDUIMESSAGE)::GetProcAddress(m_hVRHost, "SendUIMessage");

	DWORD dwTid = 0;
	s_hThreadFxWin =
		CreateThread(
			nullptr,  // LPSECURITY_ATTRIBUTES lpThreadAttributes
			0,        // SIZE_T dwStackSize,
			fxrStartFxCreateVRWindow,
			nullptr,  //__drv_aliasesMem LPVOID lpParameter,
			0,     // DWORD dwCreationFlags,
			&dwTid);
	assert(s_hThreadFxWin != nullptr);

	WCHAR fxCmd[MAX_PATH + MAX_PATH] = { 0 };
	err = swprintf_s(
		fxCmd,
		ARRAYSIZE(fxCmd),
		L"%s -wait-for-browser -profile %s --fxr",
		s_pszFxPath,
		s_pszFxProfile
	);
	assert(err > 0);

	STARTUPINFO startupInfoFx = { 0 };
	bool fCreateContentProc = ::CreateProcess(
		nullptr,  // lpApplicationName,
		fxCmd,
		nullptr,  // lpProcessAttributes,
		nullptr,  // lpThreadAttributes,
		TRUE,     // bInheritHandles,
		0,        // dwCreationFlags,
		nullptr,  // lpEnvironment,
		nullptr,  // lpCurrentDirectory,
		&startupInfoFx,
		&procInfoFx
	);

	assert(fCreateContentProc);

	DWORD waitResult = ::WaitForSingleObject(s_hThreadFxWin, 10000); // 10 seconds
	if (waitResult == WAIT_TIMEOUT) {
		FXRLOGe("Gave up waiting for Firefox VR window.\n");
	} else if (waitResult != WAIT_OBJECT_0) {
		FXRLOGe("Error waiting for Firefox VR window.\n");
	}
	s_hThreadFxWin = nullptr;
}

void fxrStopFx()
{
	PFN_CLOSEVRWINDOW lpfnClose = (PFN_CLOSEVRWINDOW)::GetProcAddress(m_hVRHost, "CloseVRWindow");
	lpfnClose(m_vrWin);

	::FreeLibrary(m_hVRHost);
	m_hVRHost = nullptr;
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
        gWindow = std::make_unique<FxRWindowDX11>(size, nativeTexturePtr, format, m_fxTexHandle, m_pfnSendUIMessage, m_vrWin);
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

void fxrRequestWindowUpdate(int windowIndex, float timeDelta)
{
	if (windowIndex < 0 || windowIndex >= fxrGetWindowCount()) return;

	if (!gWindow) return;
	gWindow->requestUpdate(timeDelta);
}

void fxrWindowPointerEvent(int windowIndex, int eventID, int windowX, int windowY)
{
	if (windowIndex < 0 || windowIndex >= fxrGetWindowCount()) return;

	if (!gWindow) return;
	switch (eventID) {
	case FxRPointerEventID_Enter:
		gWindow->pointerEnter();
		break;
	case FxRPointerEventID_Exit:
		gWindow->pointerExit();
		break;
	case FxRPointerEventID_Over:
		gWindow->pointerOver(windowX, windowY);
		break;
	case FxRPointerEventID_Press:
		gWindow->pointerPress(windowX, windowY);
		break;
	case FxRPointerEventID_Release:
		gWindow->pointerRelease(windowX, windowY);
		break;
	case FxRPointerEventID_ScrollDiscrete:
		gWindow->pointerScrollDiscrete(windowX, windowY);
		break;
	default:
		break;
	}
}
