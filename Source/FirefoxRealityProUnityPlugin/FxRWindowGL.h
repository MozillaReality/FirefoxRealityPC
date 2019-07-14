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
	int m_format;
	uint32_t m_pixelIntFormatGL;
	uint32_t m_pixelFormatGL;
	uint32_t m_pixelTypeGL;
	uint32_t m_pixelSize;
public:
	static void init();
	static void finalize();
	FxRWindowGL(Size size, uint32_t texID, int format); // pass 0 for texID to allocate internally.
	~FxRWindowGL() ;

    RendererAPI rendererAPI() override {return RendererAPI::OpenGL;}
	Size size() override;
	void setSize(Size size) override;
	void* getNativeID() override;

	// Must be called from render thread.
	void requestUpdate(float timeDelta) override;

	int format() { return m_format;	};
};

