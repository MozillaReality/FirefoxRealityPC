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

// This is copied from {%Gecko_SRC%}/moz_external_vr.h
#include "moz_external_vr.h"

#include <assert.h>
#include <stdio.h>

static ID3D11Device* s_D3D11Device = nullptr;

struct CreateVRWindowParams {
	PFN_CREATEVRWINDOW lpfnCreate;
	char* firefoxFolderPath;
	char* firefoxProfilePath;
	uint32_t vrWin;
	void* fxTexHandle;
	PFN_VREVENTCALLBACK lpfnVREvent;
	FxRWindowDX11 *pCallingWindow;
//	HANDLE hSignal;
};

DWORD FxRWindowDX11::CreateVRWindow(_In_ LPVOID lpParameter) {
	CreateVRWindowParams* pParams = static_cast<CreateVRWindowParams*>(lpParameter);

	uint32_t width;
	uint32_t height;

//	pParams->lpfnCreate(&par&pParams->vrWin, &pParams->fxTexHandle, &pParams->hSignal, &width, &height);
	//::ExitThread(0);

	pParams->lpfnCreate(pParams->firefoxFolderPath, pParams->firefoxProfilePath, 0, 0, 0, &pParams->vrWin, &pParams->fxTexHandle, &width, &height);
	pParams->pCallingWindow->WindowCreationRequestComplete(pParams->vrWin, pParams->fxTexHandle);
	::ExitThread(0);
}

void FxRWindowDX11::initDevice(IUnityInterfaces* unityInterfaces) {
	IUnityGraphicsD3D11* ud3d = unityInterfaces->Get<IUnityGraphicsD3D11>();
	s_D3D11Device = ud3d->GetDevice();
}

void FxRWindowDX11::finalizeDevice() {
	s_D3D11Device = nullptr; // The object itself being owned by Unity will go away without our help, but we should clear our weak reference.
}

FxRWindowDX11::FxRWindowDX11(int uid, int uidExt, char *pfirefoxFolderPath
	, char *pfirefoxProfilePath, PFN_CREATEVRWINDOW pfnCreateVRWindow
	, PFN_SENDUIMSG pfnSendUIMessage, PFN_WAITFORVREVENT pfnWaitForVREvent
	, PFN_CLOSEVRWINDOW pfnCloseVRWindow, PFN_VREVENTCALLBACK pfnVREventCallback) :
	FxRWindow(uid, uidExt),
	m_pfnCreateVRWindow(pfnCreateVRWindow),
	m_firefoxFolderPath(pfirefoxFolderPath),
	m_firefoxProfilePath(pfirefoxProfilePath),
	m_pfnSendUIMessage(pfnSendUIMessage),
	m_pfnWaitForVREvent(pfnWaitForVREvent),
	m_pfnCloseVRWindow(pfnCloseVRWindow),
	m_pfnVREventCallback(pfnVREventCallback),
	m_vrWin(0),
	m_fxTexPtr(nullptr),
	m_fxTexHandle(nullptr),
	m_size({0, 0}),
	m_format(FxRTextureFormat_Invalid),
	m_unityTexPtr(nullptr)
{
}

static int getFxRTextureFormatForDXGIFormat(DXGI_FORMAT format)
{
	switch (format) {
		case DXGI_FORMAT_R8G8B8A8_TYPELESS:
		case DXGI_FORMAT_R8G8B8A8_UNORM:
		case DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
		case DXGI_FORMAT_R8G8B8A8_UINT:
			return FxRTextureFormat_RGBA32;
			break;
		case DXGI_FORMAT_B8G8R8A8_UNORM:
		case DXGI_FORMAT_B8G8R8A8_UNORM_SRGB:
		case DXGI_FORMAT_B8G8R8A8_TYPELESS:
			return FxRTextureFormat_BGRA32;
			break;
		case DXGI_FORMAT_B4G4R4A4_UNORM:
			return FxRTextureFormat_RGBA4444;
			break;
		case DXGI_FORMAT_B5G6R5_UNORM:
			return FxRTextureFormat_RGB565;
			break;
		case DXGI_FORMAT_B5G5R5A1_UNORM:
			return FxRTextureFormat_RGBA5551;
			break;
		default:
			return FxRTextureFormat_Invalid;
	}
}

bool FxRWindowDX11::init(PFN_WINDOWCREATIONREQUESTCOMPLETED windowCreationRequestCompletedCallback)
{
	FxRWindow::init(windowCreationRequestCompletedCallback);
	CreateVRWindowParams* pParams = new CreateVRWindowParams;
	pParams->lpfnCreate = m_pfnCreateVRWindow;
	pParams->firefoxFolderPath = m_firefoxFolderPath;
	pParams->firefoxProfilePath = m_firefoxProfilePath;
	pParams->pCallingWindow = this;

	DWORD dwTid = 0;
	HANDLE hThreadFxWin =
		::CreateThread(
			nullptr,  // LPSECURITY_ATTRIBUTES lpThreadAttributes
			0,        // SIZE_T dwStackSize,
			CreateVRWindow,
			pParams,  //__drv_aliasesMem LPVOID lpParameter,
			0,     // DWORD dwCreationFlags,
			&dwTid);
	assert(hThreadFxWin != nullptr);

	return true;
}

FxRWindowDX11::~FxRWindowDX11() {
}

FxRWindow::Size FxRWindowDX11::size() {
	return m_size;
}

void FxRWindowDX11::setSize(FxRWindow::Size size) {
	// TODO: request change in the Firefox VR window size.
}

void FxRWindowDX11::setNativePtr(void* texPtr) {
	m_unityTexPtr = texPtr;
}

void* FxRWindowDX11::nativePtr() {
	return m_unityTexPtr;
}

void FxRWindowDX11::requestUpdate(float timeDelta) {

	if (!m_fxTexPtr || !m_unityTexPtr) {
		FXRLOGi("FxRWindowDX11::requestUpdate() m_fxTexPtr=%p, m_unityTexPtr=%p.\n", m_fxTexPtr, m_unityTexPtr);
		return;
	}

	ID3D11DeviceContext* ctx = NULL;
	s_D3D11Device->GetImmediateContext(&ctx);

	D3D11_TEXTURE2D_DESC descUnity = { 0 };
	D3D11_TEXTURE2D_DESC descFxr = { 0 };

	m_fxTexPtr->GetDesc(&descFxr);
	((ID3D11Texture2D*)m_unityTexPtr)->GetDesc(&descUnity);
	//FXRLOGd("Unity texture is %dx%d, DXGI_FORMAT=%d (FxRTextureFormat=%d), MipLevels=%d, D3D11_USAGE Usage=%d, BindFlags=%d, CPUAccessFlags=%d, MiscFlags=%d\n", descUnity.Width, descUnity.Height, descUnity.Format, getFxRTextureFormatForDXGIFormat(descUnity.Format), descUnity.MipLevels, descUnity.Usage, descUnity.BindFlags, descUnity.CPUAccessFlags, descUnity.MiscFlags);
	if (descFxr.Width != descUnity.Width || descFxr.Height != descUnity.Height) {
		FXRLOGe("Error: Unity texture size %dx%d does not match Firefox texture size %dx%d.\n", descUnity.Width, descUnity.Height, descFxr.Width, descFxr.Height);
	} else {
		ctx->CopyResource((ID3D11Texture2D*)m_unityTexPtr, m_fxTexPtr);
	}

	ctx->Release();
}

DWORD FxRWindowDX11::PollForVREvent(_In_ LPVOID lpParameter)
{
	return ((FxRWindowDX11*)lpParameter)->pollForVREvent();
}

DWORD FxRWindowDX11::pollForVREvent()
{
	while (true)
	{
		if (m_pfnWaitForVREvent && m_pfnVREventCallback)
		{
			uint32_t windowId;
			uint32_t eventType;
			uint32_t eventData1;
			uint32_t eventData2;
			m_pfnWaitForVREvent(windowId, eventType, eventData1, eventData2);
      
			if (eventType == (uint32_t)mozilla::gfx::VRFxEventType::SHUTDOWN)
			{
				fxrCloseAllWindows();
				break;
			}
			if (eventType != (uint32_t)mozilla::gfx::VRFxEventType::NONE)
			{
				m_pfnVREventCallback(m_uid, eventType, eventData1, eventData2);
			}
		}
	}
	return 0;
}

void FxRWindowDX11::ProcessPointerEvent(UINT msg, int x, int y, LONG scroll) {
	m_ptLastPointer.x = x;
	m_ptLastPointer.y = y;

	// Route this back to the Firefox window for processing
	if (m_pfnSendUIMessage) m_pfnSendUIMessage(m_vrWin, msg, MAKELONG(0, scroll), POINTTOPOINTS(m_ptLastPointer));
}

void FxRWindowDX11::CloseVRWindow() {
  if (m_pfnCloseVRWindow) m_pfnCloseVRWindow(m_vrWin, true);
}

void FxRWindowDX11::WindowCreationRequestComplete(uint32_t vrWin, void * fxTexHandle)
{
	m_vrWin = vrWin;
	m_fxTexHandle = fxTexHandle;
	if (m_windowCreationRequestCompletedCallback) (*m_windowCreationRequestCompletedCallback)(m_uidExt, m_uid);
}

void FxRWindowDX11::FinishWindowCreation(PFN_WINDOWCREATEDCALLBACK pfnWindowCreatedCallback)
{
	if (!m_fxTexHandle) {
		FXRLOGe("Error: Firefox texture handle is null.\n");
		// TODO: Error Callback?
		//	return false;
	}
	else {
		// Extract a pointer to the D3D texture from the shared handle.
		HRESULT hr = s_D3D11Device->OpenSharedResource(m_fxTexHandle, IID_PPV_ARGS(&m_fxTexPtr));
		if (hr != S_OK) {
			FXRLOGe("Can't get pointer to Firefox texture from handle.\n");
			// TODO: Error Callback?
//			return false;
		}
		else {
			D3D11_TEXTURE2D_DESC descFxr = { 0 };
			m_fxTexPtr->GetDesc(&descFxr);
			m_size = Size({ (int)descFxr.Width, (int)descFxr.Height });
            m_format = getFxRTextureFormatForDXGIFormat(descFxr.Format);

			if (pfnWindowCreatedCallback) (*pfnWindowCreatedCallback)(m_uidExt, m_uid, m_size.w, m_size.h, m_format);

			// Start polling for vr events on this window
			// TODO: When do we need to stop this thread?
			DWORD dwTid = 0;
			// Start a thread to wait for vr events
			HANDLE hThreadFxWin =
				::CreateThread(
					nullptr,  // LPSECURITY_ATTRIBUTES lpThreadAttributes
					0,        // SIZE_T dwStackSize,
					PollForVREvent,
					this,  //__drv_aliasesMem LPVOID lpParameter,
					0,     // DWORD dwCreationFlags,
					&dwTid);
			assert(hThreadFxWin != nullptr);
		}
	}
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

void FxRWindowDX11::keyPress(int charCode) {
	switch (charCode) {
		case VK_BACK:
		case VK_TAB:
		case VK_RETURN:
		case VK_ESCAPE:
		case VK_SPACE:
		case VK_LEFT:
		case VK_RIGHT:
		case VK_DOWN:
		case VK_UP:
		case VK_HOME:
		case VK_END:
			m_pfnSendUIMessage(m_vrWin, WM_KEYDOWN, charCode, 0);
			m_pfnSendUIMessage(m_vrWin, WM_KEYUP,   charCode, 0);
			break;
		default:
			m_pfnSendUIMessage(m_vrWin, WM_CHAR, charCode, 0);
	}
}