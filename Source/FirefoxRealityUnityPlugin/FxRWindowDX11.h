﻿//
// FxRWindowDX11.h
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0.If a copy of the MPL was not distributed with this
// file, You can obtain one at https ://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019- Mozilla, Inc.
//
// Author(s): Philip Lamb, Thomas Moore
//

#pragma once
#include "FxRWindow.h"
#include <cstdint>
#include <string>
#include <Windows.h>
#include "IUnityInterface.h"
#include "vrhost.h"

struct ID3D11Texture2D;

class FxRWindowDX11 : public FxRWindow
{
private:

	PFN_CREATEVRWINDOW m_pfnCreateVRWindow;
	PFN_SENDUIMSG m_pfnSendUIMessage;
	PFN_WAITFORVREVENT m_pfnWaitForVREvent;
	PFN_SENDVRTELEMETRY m_pfnSendVRTelemetry;
	char *m_firefoxFolderPath;
	char *m_firefoxProfilePath;
	PFN_CLOSEVRWINDOW m_pfnCloseVRWindow;
	PFN_VREVENTCALLBACK m_pfnVREventCallback;
	UINT m_vrWin;
	ID3D11Texture2D* m_fxTexPtr;
	void* m_fxTexHandle;
	Size m_size;
	int m_format;
	void *m_unityTexPtr;
    POINT m_ptLastPointer;

    void ProcessPointerEvent(UINT msg, int x, int y, LONG scroll);
	DWORD pollForVREvent();
	static DWORD PollForVREvent(LPVOID lpParameter);

public:
	static void initDevice(IUnityInterfaces* unityInterfaces);
	static void finalizeDevice();
	static DWORD WINAPI CreateVRWindow(_In_ LPVOID lpParameter);


	FxRWindowDX11(int uid, int uidExt, char *pfirefoxFolderPath, char *pfirefoxProfilePath, PFN_CREATEVRWINDOW pfnCreateVRWindow,
		PFN_SENDUIMSG pfnSendUIMessage, PFN_WAITFORVREVENT pfnWaitForVREvent, PFN_CLOSEVRWINDOW pfnCloseVRWindow, PFN_VREVENTCALLBACK pfnVREventCallback,
		PFN_SENDVRTELEMETRY pfnSendVRTelemetry);
	~FxRWindowDX11() ;
	FxRWindowDX11(const FxRWindowDX11&) = delete;
	void operator=(const FxRWindowDX11&) = delete;
	FxRWindowDX11(FxRWindowDX11 &&) noexcept = default;
	FxRWindowDX11& operator=(FxRWindowDX11 &&) noexcept = default;
	void CloseVRWindow() override;
	void WindowCreationRequestComplete(uint32_t vrWin, void* fxTexHandle) override;
	void FinishWindowCreation(PFN_WINDOWCREATEDCALLBACK pfnWindowCreatedCallback) override;

	bool init(PFN_WINDOWCREATIONREQUESTCOMPLETED pfnWindowCreationRequestCompletedCallback) override;
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
	void keyPress(int charCode) override;
};

