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
#include "IUnityInterface.h"

class FxRWindowDX11 : public FxRWindow
{
private:
	Size m_size;
	void *m_texPtr;
	uint8_t *m_buf;
	int m_format;
    int m_pixelSize;
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
};

