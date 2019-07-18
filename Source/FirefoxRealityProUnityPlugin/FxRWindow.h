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

	virtual RendererAPI rendererAPI() = 0;
	virtual Size size() = 0;
	virtual void setSize(Size size) = 0;
	virtual int format() = 0;
	virtual void* getNativePtr() = 0;
	virtual void requestUpdate(float timeDelta) = 0;
};

