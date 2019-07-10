#ifdef __APPLE__
#  include <OpenGL/gl3.h>
#elif defined(_WIN32)
#  include <gl3w/gl3w.h>
#else 
#  define GL_GLEXT_PROTOTYPES
#  include <GL/glcorearb.h>
#endif
#include <stdlib.h>
#include "FxRWindowGL.h"

FxRWindowGL::FxRWindowGL(Size size, uint32_t texID) :
	m_size(size),
	m_texID(texID),
	m_generatedTex(false),
	m_buf(NULL)
{
#ifdef _WIN32
	gl3wInit();
#endif

	if (m_texID == 0) {
		glGenTextures(1, &m_texID);
		m_generatedTex = true;
	}
	setSize(size);
}

FxRWindowGL::~FxRWindowGL() {
	if (m_generatedTex) {
		glDeleteTextures(1, &m_texID);
		m_texID = 0;
		m_generatedTex = false;
	}
	if (m_buf) {
		free(m_buf);
		m_buf = NULL;
	}
}

FxRWindow::Size FxRWindowGL::size() {
	return m_size;
}

void FxRWindowGL::setSize(FxRWindow::Size size) {
	m_size = size;
	if (m_buf) free(m_buf);
	m_buf = (uint8_t *)calloc(1, m_size.w * m_size.h * 4); // Always RGBA 4Bpp.
	glBindTexture(GL_TEXTURE_2D, m_texID);
	glActiveTexture(GL_TEXTURE0);
	//glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, m_size.w, m_size.h, 0, GL_RGBA, GL_UNSIGNED_BYTE, m_buf);
	glTexImage2D(GL_TEXTURE_RECTANGLE, 0, GL_RGBA, m_size.w, m_size.h, 0, GL_RGBA, GL_UNSIGNED_BYTE, m_buf);
}

void* FxRWindowGL::getNativeID() {
	return (void *)m_texID;
}

void FxRWindowGL::requestUpdate(float timeDelta) {

	// Auto-generate a dummy texture. A 100 x 100 square, oscillating in x dimension.
	static int k = 0;
	int i, j;
	k++;
	if (k > 100) k = -100;
	memset(m_buf, 255, m_size.w * m_size.h * 4); // Clear to white.
	for (j = m_size.h / 2 - 50; j < m_size.h / 2 - 25; j++) {
		for (i = m_size.w / 2 - 50 + k; i < m_size.w / 2 + 50 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = m_buf[(j*m_size.w + i) * 4 + 1] = m_buf[(j*m_size.w + i) * 4 + 2] = 0; m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
	}
	for (j = m_size.h / 2 - 25; j < m_size.h / 2 + 25; j++) {
		// Black bar (25 pixels wide).
		for (i = m_size.w / 2 - 50 + k; i < m_size.w / 2 - 25 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = m_buf[(j*m_size.w + i) * 4 + 1] = m_buf[(j*m_size.w + i) * 4 + 2] = 0; m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
		// Red bar (17 pixels wide).
		for (i = m_size.w / 2 - 25 + k; i < m_size.w / 2 - 8 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = 255; m_buf[(j*m_size.w + i) * 4 + 1] = m_buf[(j*m_size.w + i) * 4 + 2] = 0; m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
		// Green bar (16 pixels wide).
		for (i = m_size.w / 2 - 8 + k; i < m_size.w / 2 + 8 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = 0; m_buf[(j*m_size.w + i) * 4 + 1] = 255; m_buf[(j*m_size.w + i) * 4 + 2] = 0; m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
		// Blue bar (17 pixels wide).
		for (i = m_size.w / 2 + 8 + k; i < m_size.w / 2 + 25 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = m_buf[(j*m_size.w + i) * 4 + 1] = 0; m_buf[(j*m_size.w + i) * 4 + 2] = m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
		// Black bar (25 pixels wide).
		for (i = m_size.w / 2 + 25 + k; i < m_size.w / 2 + 50 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = m_buf[(j*m_size.w + i) * 4 + 1] = m_buf[(j*m_size.w + i) * 4 + 2] = 0; m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
	}
	for (j = m_size.h / 2 + 25; j < m_size.h / 2 + 50; j++) {
		for (i = m_size.w / 2 - 50 + k; i < m_size.w / 2 + 50 + k; i++) {
			m_buf[(j*m_size.w + i) * 4 + 0] = m_buf[(j*m_size.w + i) * 4 + 1] = m_buf[(j*m_size.w + i) * 4 + 2] = 0; m_buf[(j*m_size.w + i) * 4 + 3] = 255;
		}
	}

	glBindTexture(GL_TEXTURE_2D, m_texID);
	glActiveTexture(GL_TEXTURE0);
	//glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, m_size.w, m_size.h, GL_RGBA, GL_UNSIGNED_BYTE, m_buf);
	glTexSubImage2D(GL_TEXTURE_RECTANGLE, 0, 0, 0, m_size.w, m_size.h, GL_RGBA, GL_UNSIGNED_BYTE, m_buf);
}
