//
// FxRWindowDX11.cpp
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0.If a copy of the MPL was not distributed with this
// file, You can obtain one at https ://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019- Mozilla, Inc.
//
// Author(s): Philip Lamb
//

#include <stdlib.h>
#include "FxRWindowDX11.h"
#include "fxr_unity_c.h"
#include <d3d11.h>
#include "IUnityGraphicsD3D11.h"
#include "fxr_log.h"

#include <assert.h>
#include <stdio.h>

static ID3D11Device* s_D3D11Device = nullptr;

static WCHAR s_pszFxPath[] = L"e:\\src4\\gecko_build_release\\dist\\bin\\firefox.exe";
static WCHAR s_pszVrHostPath[] = L"e:\\src4\\gecko_build_release\\dist\\bin\\vrhost.dll";
static WCHAR s_pszFxProfile[] = L"e:\\src4\\gecko_build_release\\tmp\\profile-default";
static HANDLE s_hThreadFxWin = nullptr;

// vrhost.dll Members
static HINSTANCE m_hVRHost = nullptr;
static PFN_SENDUIMESSAGE m_pfnSendUIMessage = nullptr;

// Window/Process State for Host and Firefox
static HANDLE m_hTex = nullptr;
static ID3D11Texture2D* m_pTex = nullptr;
static PROCESS_INFORMATION procInfoFx = { 0 };
static HWND   m_hwndHost = nullptr;
static UINT   m_vrWin = 0;
static HANDLE m_hSignal = nullptr;

DWORD FxRWindowDX11::FxWindowCreateInit(_In_ LPVOID lpParameter) {
  FxRWindowDX11* pInstance = static_cast<FxRWindowDX11*>(lpParameter);

  PFN_CREATEVRWINDOW lpfnCreate = (PFN_CREATEVRWINDOW)::GetProcAddress(m_hVRHost, "CreateVRWindow");
  uint64_t width;
  uint64_t height;

  lpfnCreate(&m_vrWin, &m_hTex, &m_hSignal, &width, &height);

  ::ExitThread(0);
}

void  FxRWindowDX11::FxInit() {
  assert(s_hThreadFxWin == nullptr);

  m_pfnSendUIMessage = (PFN_SENDUIMESSAGE)::GetProcAddress(m_hVRHost, "SendUIMessage");

  DWORD dwTid = 0;
  s_hThreadFxWin =
    CreateThread(
      nullptr,  // LPSECURITY_ATTRIBUTES lpThreadAttributes
      0,        // SIZE_T dwStackSize,
      FxRWindowDX11::FxWindowCreateInit,
      this,  //__drv_aliasesMem LPVOID lpParameter,
      0,     // DWORD dwCreationFlags,
      &dwTid);
  assert(s_hThreadFxWin != nullptr);

  WCHAR fxCmd[MAX_PATH + MAX_PATH] = { 0 };
  int err = swprintf_s(
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

  ::WaitForSingleObject(s_hThreadFxWin, INFINITE);
  s_hThreadFxWin = nullptr;
}

void FxRWindowDX11::FxClose() {
  PFN_CLOSEVRWINDOW lpfnClose = (PFN_CLOSEVRWINDOW)::GetProcAddress(m_hVRHost, "CloseVRWindow");
  lpfnClose(m_vrWin);

  ::FreeLibrary(m_hVRHost);
  m_hVRHost = nullptr;
}

void FxRWindowDX11::init(IUnityInterfaces* unityInterfaces) {
    IUnityGraphicsD3D11* ud3d = unityInterfaces->Get<IUnityGraphicsD3D11>();
    s_D3D11Device = ud3d->GetDevice();
}

void FxRWindowDX11::finalize() {
    s_D3D11Device = nullptr; // The object itself being owned by Unity will go away without our help, but we should clear our weak reference.

    FxClose();
}

FxRWindowDX11::FxRWindowDX11(Size size, void *texPtr, int format, const std::string& resourcesPath) :
	m_size(size),
	m_texPtr(texPtr),
	m_buf(NULL),
	m_format(format),
	m_pixelSize(0)
{
  m_hVRHost = ::LoadLibrary(s_pszVrHostPath);
  assert(m_hVRHost != nullptr);

  FxInit();

  assert(m_hTex != nullptr);
  assert(m_pTex == nullptr);

	switch (format) {
	case FxRTextureFormat_RGBA32:
		m_pixelSize = 4;
		break;
	case FxRTextureFormat_BGRA32:
		m_pixelSize = 4;
		break;
	case FxRTextureFormat_ARGB32:
		m_pixelSize = 4;
		break;
	case FxRTextureFormat_ABGR32:
		m_pixelSize = 4;
		break;
	case FxRTextureFormat_RGB24:
		m_pixelSize = 3;
		break;
	case FxRTextureFormat_BGR24:
		m_pixelSize = 3;
		break;
	case FxRTextureFormat_RGBA4444:
		m_pixelSize = 2;
		break;
	case FxRTextureFormat_RGBA5551:
		m_pixelSize = 2;
		break;
	case FxRTextureFormat_RGB565:
		m_pixelSize = 2;
		break;
	default:
		break;
	}

	setSize(size);
}

FxRWindowDX11::~FxRWindowDX11() {
	if (m_buf) {
		free(m_buf);
		m_buf = NULL;
	}
}

FxRWindow::Size FxRWindowDX11::size() {
	return m_size;
}

void FxRWindowDX11::setSize(FxRWindow::Size size) {
	m_size = size;
	if (m_buf) free(m_buf);
	m_buf = (uint8_t *)calloc(1, m_size.w * m_size.h * m_pixelSize);
}

void* FxRWindowDX11::getNativePtr() {
	return m_texPtr;
}

void FxRWindowDX11::requestUpdate(float timeDelta) {  
  HRESULT hr = S_OK;
  if (m_pTex == nullptr) {
    hr = s_D3D11Device->OpenSharedResource(
      m_hTex,
      IID_PPV_ARGS(&m_pTex)
    );
  }

  ID3D11DeviceContext* ctx = NULL;
  s_D3D11Device->GetImmediateContext(&ctx);

  D3D11_TEXTURE2D_DESC descUnity = { 0 };
  D3D11_TEXTURE2D_DESC descFxr = { 0 };
  
  m_pTex->GetDesc(&descFxr);
  ((ID3D11Texture2D*)m_texPtr)->GetDesc(&descUnity);

  ctx->CopyResource((ID3D11Texture2D*)m_texPtr, m_pTex);

  ctx->Release();
}

void FxRWindowDX11::ProcessPointerEvent(UINT msg, int x, int y, LONG scroll) {
  m_ptLastPointer.x = x;
  m_ptLastPointer.y = y;
 
  // Route this back to the Firefox window for processing
  m_pfnSendUIMessage(m_vrWin, msg, MAKELONG(0, scroll), POINTTOPOINTS(m_ptLastPointer));
}

void FxRWindowDX11::pointerEnter() {
	FXRLOGi("FxRWindowDX11::pointerEnter()\n");
}

void FxRWindowDX11::pointerExit() {
	FXRLOGi("FxRWindowDX11::pointerExit()\n");
}

void FxRWindowDX11::pointerOver(int x, int y) {
	//FXRLOGi("FxRWindowDX11::pointerOver(%d, %d)\n", x, y);
  ProcessPointerEvent(WM_MOUSEMOVE, x, y, 0);
}

void FxRWindowDX11::pointerPress(int x, int y) {
	FXRLOGi("FxRWindowDX11::pointerPress(%d, %d)\n", x, y);
  ProcessPointerEvent(WM_LBUTTONDOWN, x, y, 0);
}

void FxRWindowDX11::pointerRelease(int x, int y) {
	FXRLOGi("FxRWindowDX11::pointerRelease(%d, %d)\n", x, y);
  ProcessPointerEvent(WM_LBUTTONUP, x, y, 0);
}

void FxRWindowDX11::pointerScrollDiscrete(int x, int y) {
	FXRLOGi("FxRWindowDX11::pointerScrollDiscrete(%d, %d)\n", x, y);
  
  SHORT scrollDelta = WHEEL_DELTA * (SHORT)y;  
  ProcessPointerEvent(WM_MOUSEWHEEL, m_ptLastPointer.x, m_ptLastPointer.y, scrollDelta);
}
