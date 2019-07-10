#pragma once
class FxRWindow
{
public:
	virtual ~FxRWindow() {};

	enum RendererAPI {
		None = 0,
		Unknown,
		OpenGL,
		DirectX
	};
	struct Size {
		int w;
		int h;
	};

	virtual RendererAPI rendererAPI() = 0;
	virtual Size size() = 0;
	virtual void setSize(Size size) = 0;
	virtual void* getNativeID() = 0;
	virtual void requestUpdate(float timeDelta) = 0;
};

