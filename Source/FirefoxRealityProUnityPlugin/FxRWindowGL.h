#pragma once
#include "FxRWindow.h"
#include <cstdint>

class FxRWindowGL : public FxRWindow
{
private:
	Size m_size;
	uint32_t m_texID;
	bool m_generatedTex;
	uint8_t *m_buf;
public:
	FxRWindowGL(Size size, uint32_t texID); // pass 0 for texID to allocate internally.
	~FxRWindowGL() ;

    RendererAPI rendererAPI() override {return RendererAPI::OpenGL;}
	Size size() override;
	void setSize(Size size) override;
	void* getNativeID() override;
	void requestUpdate(float timeDelta) override;
};

