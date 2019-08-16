//
// FxRWindowGL.h
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

class FxRWindowGL : public FxRWindow
{
private:
	Size m_size;
	uint32_t m_texID;
	uint8_t *m_buf;
	int m_format;
	uint32_t m_pixelIntFormatGL;
	uint32_t m_pixelFormatGL;
	uint32_t m_pixelTypeGL;
	uint32_t m_pixelSize;
public:
	static void init();
	static void finalize();
	FxRWindowGL(int index, Size size);
	~FxRWindowGL() ;

    RendererAPI rendererAPI() override {return RendererAPI::OpenGLCore;}
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

