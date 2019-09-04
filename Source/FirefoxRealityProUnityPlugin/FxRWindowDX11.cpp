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

struct CreateVRWindowParams {
	PFN_CREATEVRWINDOW lpfnCreate;
	UINT vrWin;
	HANDLE fxTexHandle;
	HANDLE hSignal;
};

DWORD FxRWindowDX11::CreateVRWindow(_In_ LPVOID lpParameter) {
	CreateVRWindowParams* pParams = static_cast<CreateVRWindowParams*>(lpParameter);

	uint64_t width;
	uint64_t height;

	pParams->lpfnCreate(&pParams->vrWin, &pParams->fxTexHandle, &pParams->hSignal, &width, &height);
	::ExitThread(0);
}


void FxRWindowDX11::initDevice(IUnityInterfaces* unityInterfaces) {
	IUnityGraphicsD3D11* ud3d = unityInterfaces->Get<IUnityGraphicsD3D11>();
	s_D3D11Device = ud3d->GetDevice();
}

void FxRWindowDX11::finalizeDevice() {
	s_D3D11Device = nullptr; // The object itself being owned by Unity will go away without our help, but we should clear our weak reference.
}

FxRWindowDX11::FxRWindowDX11(int uid, int uidExt, PFN_CREATEVRWINDOW pfnCreateVRWindow, PFN_SENDUIMESSAGE pfnSendUIMessage, PFN_CLOSEVRWINDOW pfnCloseVRWindow) :
	FxRWindow(uid, uidExt),
	m_pfnCreateVRWindow(pfnCreateVRWindow),
	m_pfnSendUIMessage(pfnSendUIMessage),
	m_pfnCloseVRWindow(pfnCloseVRWindow),
	m_vrWin(0),
	m_fxTexPtr(nullptr),
	m_size({0, 0}),
	m_format(FxRTextureFormat_Invalid),
	m_unityTexPtr(nullptr)
{
}

bool FxRWindowDX11::init(PFN_WINDOWCREATEDCALLBACK windowCreatedCallback)
{
	CreateVRWindowParams* pParams = new CreateVRWindowParams;
	pParams->lpfnCreate = m_pfnCreateVRWindow;

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

	HANDLE fxTexHandle = nullptr;
	DWORD waitResult = ::WaitForSingleObject(hThreadFxWin, 10000); // 10 seconds
	if (waitResult == WAIT_TIMEOUT) {
		FXRLOGe("Gave up waiting for Firefox VR window.\n");
		return false;
	} else if (waitResult != WAIT_OBJECT_0) {
		FXRLOGe("Error waiting for Firefox VR window.\n");
		return false;
	} else {
		m_vrWin = pParams->vrWin;
		fxTexHandle = pParams->fxTexHandle;
	}

	if (!fxTexHandle) {
		FXRLOGe("Error: Firefox texture handle is null.\n");
		return false;
	} else {
		// Extract a pointer to the D3D texture from the shared handle.
		HRESULT hr = s_D3D11Device->OpenSharedResource(fxTexHandle, IID_PPV_ARGS(&m_fxTexPtr) );
		if (hr != S_OK) {
			FXRLOGe("Can't get pointer to Firefox texture from handle.\n");
			return false;
		} else {
			D3D11_TEXTURE2D_DESC descFxr = { 0 };
			m_fxTexPtr->GetDesc(&descFxr);
            m_size = Size({(int)descFxr.Width, (int)descFxr.Height});
            switch (descFxr.Format) {
				case DXGI_FORMAT_R8G8B8A8_TYPELESS:
				case DXGI_FORMAT_R8G8B8A8_UNORM:
				case DXGI_FORMAT_R8G8B8A8_UINT:
					m_format = FxRTextureFormat_RGBA32;
					break;
				case DXGI_FORMAT_B8G8R8A8_UNORM:
				case DXGI_FORMAT_B8G8R8A8_TYPELESS:
					m_format = FxRTextureFormat_BGRA32;
					break;
				case DXGI_FORMAT_B4G4R4A4_UNORM:
					m_format = FxRTextureFormat_RGBA4444;
					break;
				case DXGI_FORMAT_B5G6R5_UNORM:
					m_format = FxRTextureFormat_RGB565;
					break;
				case DXGI_FORMAT_B5G5R5A1_UNORM:
					m_format = FxRTextureFormat_RGBA5551;
					break;
				default:
					m_format = FxRTextureFormat_Invalid;
			}

			if (windowCreatedCallback) (*windowCreatedCallback)(m_uidExt, m_uid, m_size.w, m_size.h, m_format);
		}
	}
	return true;
}

FxRWindowDX11::~FxRWindowDX11() {
	m_pfnCloseVRWindow(m_vrWin);
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
	if (descFxr.Width != descUnity.Width || descFxr.Height != descUnity.Height) {
		FXRLOGe("Error: Unity texture size %dx%d does not match Firefox texture size %dx%d.\n", descUnity.Width, descUnity.Height, descFxr.Width, descFxr.Height);
	} else {
		ctx->CopyResource((ID3D11Texture2D*)m_unityTexPtr, m_fxTexPtr);
	}

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

void FxRWindowDX11::keyPress(int charCode) {
	switch (charCode) {
		case VK_BACK:
		case VK_TAB:
		case VK_RETURN:
		case VK_ESCAPE:
			m_pfnSendUIMessage(m_vrWin, WM_KEYDOWN, charCode, 0);
			m_pfnSendUIMessage(m_vrWin, WM_KEYUP,   charCode, 0);
			break;
		default:
			m_pfnSendUIMessage(m_vrWin, WM_CHAR, charCode, 0);
	}
}