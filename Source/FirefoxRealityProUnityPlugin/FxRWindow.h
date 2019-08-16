//
// FxRWindow.h
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
class FxRWindow
{
protected:
	FxRWindow(int index) : m_index(index) { }
	int m_index;
public:
	virtual ~FxRWindow() {};

	enum RendererAPI {
		None = 0,
		Unknown,
		DirectX11,
		OpenGLCore
	};
	struct Size {
		int w;
		int h;
	};

	int index() { return m_index; }
	virtual RendererAPI rendererAPI() = 0;
	virtual Size size() = 0;
	virtual void setSize(Size size) = 0;
	virtual int format() = 0;
	virtual void setNativePtr(void* texPtr) = 0;
	virtual void* nativePtr() = 0;
	virtual void requestUpdate(float timeDelta) = 0;

	virtual void pointerEnter() = 0;
	virtual void pointerExit() = 0;
	virtual void pointerOver(int x, int y) = 0;
	virtual void pointerPress(int x, int y) = 0;
	virtual void pointerRelease(int x, int y) = 0;
	virtual void pointerScrollDiscrete(int x, int y) = 0; // x and y are a discrete scroll count, e.g. count of mousewheel "clicks".
};

