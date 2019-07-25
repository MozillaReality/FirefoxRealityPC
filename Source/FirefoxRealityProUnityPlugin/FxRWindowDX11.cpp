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

static ID3D11Device* s_D3D11Device = nullptr;

void FxRWindowDX11::init(IUnityInterfaces* unityInterfaces) {
    IUnityGraphicsD3D11* ud3d = unityInterfaces->Get<IUnityGraphicsD3D11>();
    s_D3D11Device = ud3d->GetDevice();
}

void FxRWindowDX11::finalize() {
    s_D3D11Device = nullptr; // The object itself being owned by Unity will go away without our help, but we should clear our weak reference.
}

FxRWindowDX11::FxRWindowDX11(Size size, void *texPtr, int format, const std::string& resourcesPath) :
	m_size(size),
	m_texPtr(texPtr),
	m_buf(NULL),
	m_format(format),
	m_pixelSize(0)
{
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

	// Auto-generate a dummy texture. A 100 x 100 square, oscillating in x dimension.
	static int k = 0;
	int i, j;
	k++;
	if (k > 100) k = -100;
	memset(m_buf, 255, m_size.w * m_size.h * m_pixelSize); // Clear to white.
	// Assumes RGBA32!
	for (j = m_size.h / 2 - 50; j < m_size.h / 2 - 25; j++) {
		for (i = m_size.w / 2 - 50 + k; i < m_size.w / 2 + 50 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = m_buf[(j*m_size.w + i) * 4 + 1] = m_buf[(j*m_size.w + i) * 4 + 2] = 0; m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
	}
	for (j = m_size.h / 2 - 25; j < m_size.h / 2 + 25; j++) {
		// Black bar (25 pixels wide).
		for (i = m_size.w / 2 - 50 + k; i < m_size.w / 2 - 25 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = m_buf[(j*m_size.w + i) * 4 + 1] = m_buf[(j*m_size.w + i) * 4 + 2] = 0; m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
		// Red bar (17 pixels wide).
		for (i = m_size.w / 2 - 25 + k; i < m_size.w / 2 - 8 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = 255; m_buf[(j*m_size.w + i) * 4 + 1] = m_buf[(j*m_size.w + i) * 4 + 2] = 0; m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
		// Green bar (16 pixels wide).
		for (i = m_size.w / 2 - 8 + k; i < m_size.w / 2 + 8 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = 0; m_buf[(j*m_size.w + i) * 4 + 1] = 255; m_buf[(j*m_size.w + i) * 4 + 2] = 0; m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
		// Blue bar (17 pixels wide).
		for (i = m_size.w / 2 + 8 + k; i < m_size.w / 2 + 25 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = m_buf[(j*m_size.w + i) * 4 + 1] = 0; m_buf[(j*m_size.w + i) * 4 + 2] = m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
		// Black bar (25 pixels wide).
		for (i = m_size.w / 2 + 25 + k; i < m_size.w / 2 + 50 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = m_buf[(j*m_size.w + i) * 4 + 1] = m_buf[(j*m_size.w + i) * 4 + 2] = 0; m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
	}
	for (j = m_size.h / 2 + 25; j < m_size.h / 2 + 50; j++) {
		for (i = m_size.w / 2 - 50 + k; i < m_size.w / 2 + 50 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = m_buf[(j*m_size.w + i) * 4 + 1] = m_buf[(j*m_size.w + i) * 4 + 2] = 0; m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
	}

    ID3D11DeviceContext* ctx = NULL;
    s_D3D11Device->GetImmediateContext(&ctx);

    // Could also do this here if we needed texture info:
    //D3D11_TEXTURE2D_DESC desc;
    //((ID3D11Texture2D*)m_texPtr)->GetDesc(&desc);

#if 0
    // Updating from a source smaller than the destination.
    D3D11_BOX box;
    box.front = 0;
    box.back = 1;
    box.left = 0;
    box.right = m_size.w;
    box.top = 0;
    box.bottom = m_size.h;
    ctx->UpdateSubresource((ID3D11Texture2D*)m_texPtr, 0, &box, m_buf, m_size.w * m_pixelSize, m_size.h * m_size.w * m_pixelSize);
#else
    ctx->UpdateSubresource((ID3D11Texture2D*)m_texPtr, 0, NULL, m_buf, m_size.w * m_pixelSize, m_size.h * m_size.w * m_pixelSize);
#endif
    ctx->Release();

}

void FxRWindowDX11::pointerEnter() {
	FXRLOGi("FxRWindowDX11::pointerEnter()\n");
}

void FxRWindowDX11::pointerExit() {
	FXRLOGi("FxRWindowDX11::pointerExit()\n");
}

void FxRWindowDX11::pointerOver(int x, int y) {
	//FXRLOGi("FxRWindowDX11::pointerOver(%d, %d)\n", x, y);
}

void FxRWindowDX11::pointerPress(int x, int y) {
	FXRLOGi("FxRWindowDX11::pointerPress(%d, %d)\n", x, y);
}

void FxRWindowDX11::pointerRelease(int x, int y) {
	FXRLOGi("FxRWindowDX11::pointerRelease(%d, %d)\n", x, y);
}

void FxRWindowDX11::pointerScrollDiscrete(int x, int y) {
	FXRLOGi("FxRWindowDX11::pointerScrollDiscrete(%d, %d)\n", x, y);
}
