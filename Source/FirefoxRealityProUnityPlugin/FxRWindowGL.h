#pragma once
#include "FxRWindow.h"

class FxRWindowGL : public FxRWindow
{
private:
	int m_TexID;
public:
	FxRWindowGL();
	~FxRWindowGL() ;

    RendererAPI rendererAPI() override {return RendererAPI::OpenGL;}
	Size size() override;
	void setSize(Size size) override;
	void* getNativeID() override;
};

