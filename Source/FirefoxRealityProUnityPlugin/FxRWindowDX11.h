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
#include <string>
#include <Windows.h>
#include "IUnityInterface.h"
#include "vrhost.h"
#include "fxr_unity_c.h"

struct ID3D11Texture2D;

class FxRWindowDX11 : public FxRWindow
{
private:

	PFN_CREATEVRWINDOW m_pfnCreateVRWindow;
	PFN_SENDUIMESSAGE m_pfnSendUIMessage;
	PFN_CLOSEVRWINDOW m_pfnCloseVRWindow;
	UINT m_vrWin;
	ID3D11Texture2D* m_fxTexPtr;
	Size m_size;
	int m_format;
	void *m_unityTexPtr;
    POINT m_ptLastPointer;

    void ProcessPointerEvent(UINT msg, int x, int y, LONG scroll);

public:
	static void init(IUnityInterfaces* unityInterfaces);
	static void finalize();
	static DWORD CreateVRWindow(_In_ LPVOID lpParameter);

	FxRWindowDX11(int index, PFN_CREATEVRWINDOW pfnCreateVRWindow, PFN_SENDUIMESSAGE pfnSendUIMessage, PFN_CLOSEVRWINDOW pfnCloseVRWindow, PFN_WINDOWCREATEDCALLBACK windowCreatedCallback);
	~FxRWindowDX11() ;

    RendererAPI rendererAPI() override {return RendererAPI::DirectX11;}
	Size size() override;
	void setSize(Size size) override;
	void setNativePtr(void* texPtr) override;
	void* nativePtr() override;

	// Must be called from render thread.
	void requestUpdate(float timeDelta) override;

	int format() override { return m_format; }

	void pointerEnter() override;
	void pointerExit() override;
	void pointerOver(int x, int y) override;
	void pointerPress(int x, int y) override;
	void pointerRelease(int x, int y) override;
	void pointerScrollDiscrete(int x, int y) override;

};

