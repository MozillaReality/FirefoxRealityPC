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

void FxRWindowDX11::init(IUnityInterfaces* unityInterfaces) {
	IUnityGraphicsD3D11* ud3d = unityInterfaces->Get<IUnityGraphicsD3D11>();
	s_D3D11Device = ud3d->GetDevice();
}

void FxRWindowDX11::finalize() {
	s_D3D11Device = nullptr; // The object itself being owned by Unity will go away without our help, but we should clear our weak reference.
}

FxRWindowDX11::FxRWindowDX11(Size size, void *unityTexPtr, int format, HANDLE fxTexHandle, PFN_SENDUIMESSAGE pfnSendUIMessage, UINT vrWin) :
	m_size(size),
	m_unityTexPtr(unityTexPtr),
	m_buf(NULL),
	m_format(format),
	m_pixelSize(0),
	m_fxTexPtr(nullptr),
	m_pfnSendUIMessage(pfnSendUIMessage),
	m_vrWin(vrWin)
{
	if (!fxTexHandle) {
		FXRLOGw("Warning: Firefox texture handle is null.\n");
	} else {
		// Extract a pointer to the D3D texture from the shared handle.
		HRESULT hr = s_D3D11Device->OpenSharedResource(
			fxTexHandle,
			IID_PPV_ARGS(&m_fxTexPtr)
		);
		if (hr != S_OK) {
			FXRLOGe("Can't get pointer to Firefox texture from handle.\n");
		}
	}

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
	
	// TODO: callback to Unity here.
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
	return m_unityTexPtr;
}

void FxRWindowDX11::requestUpdate(float timeDelta) {

	if (!m_fxTexPtr || !m_unityTexPtr) {
		return;
	}

	ID3D11DeviceContext* ctx = NULL;
	s_D3D11Device->GetImmediateContext(&ctx);

	D3D11_TEXTURE2D_DESC descUnity = { 0 };
	D3D11_TEXTURE2D_DESC descFxr = { 0 };

	m_fxTexPtr->GetDesc(&descFxr);
	((ID3D11Texture2D*)m_unityTexPtr)->GetDesc(&descUnity);
	if (descFxr.Width != descUnity.Width || descFxr.Height != descUnity.Height) {
		FXRLOGe("Error: Unity and Firefox texture sizes do not match.\n");
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
