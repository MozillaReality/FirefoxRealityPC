//
// FxRWindowDX11.h
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0.If a copy of the MPL was not distributed with this
// file, You can obtain one at https ://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019- Mozilla, Inc.
//
// Author(s): Philip Lamb
//

#pragma once
#include "FxRWindow.h"
#include <cstdint>
#include <Windows.h>
#include "IUnityInterface.h"

struct ID3D11Texture2D;

typedef void(*PFN_CREATEVRWINDOW)(UINT* windowId, HANDLE* hTex, HANDLE* hEvt, uint64_t* width, uint64_t* height);
typedef void(*PFN_CLOSEVRWINDOW)(UINT nVRWindow);
typedef void(*PFN_SENDUIMESSAGE)(UINT nVRWindow, UINT msg, uint64_t wparam, uint64_t lparam);

class FxRWindowDX11 : public FxRWindow
{
private:

	Size m_size;
	void *m_texPtr;
	uint8_t *m_buf;
	int m_format;
    int m_pixelSize;

  static DWORD FxWindowCreateInit(_In_ LPVOID lpParameter);
  void FxInit();

public:
	static void init(IUnityInterfaces* unityInterfaces);
	static void finalize();
	FxRWindowDX11(Size size, void* texPtr, int format);
	~FxRWindowDX11() ;

    RendererAPI rendererAPI() override {return RendererAPI::DirectX11;}
	Size size() override;
	void setSize(Size size) override;
	void* getNativePtr() override;

	// Must be called from render thread.
	void requestUpdate(float timeDelta) override;

	int format() override { return m_format; }

	void pointerEnter() override;
	void pointerExit() override;
	void pointerOver(int x, int y) override;
	void pointerPress(int x, int y) override;
	void pointerRelease(int x, int y) override;

};

