//
// FxRWindowGL.cpp
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0.If a copy of the MPL was not distributed with this
// file, You can obtain one at https ://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019- Mozilla, Inc.
//
// Author(s): Philip Lamb
//

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
#include "fxr_unity_c.h"
#include "fxr_log.h"

void FxRWindowGL::init() {
#ifdef _WIN32
	gl3wInit();
#endif
}

void FxRWindowGL::finalize() {
}

FxRWindowGL::FxRWindowGL(Size size, void* texPtr, int format) :
	m_size(size),
#pragma warning(disable:4302) // Disable truncation warnings, it's the desired behaviour.
#pragma warning(disable:4311)
	m_texID((uint32_t)texPtr),
#pragma warning(default:4302)
#pragma warning(default:4311)
    m_generatedTex(false),
	m_buf(NULL),
	m_format(format),
	m_pixelIntFormatGL(0),
	m_pixelFormatGL(0),
	m_pixelTypeGL(0),
	m_pixelSize(0)
{
	switch (format) {
	case FxRTextureFormat_RGBA32:
		m_pixelIntFormatGL = GL_RGBA;
		m_pixelFormatGL = GL_RGBA;
		m_pixelTypeGL = GL_UNSIGNED_BYTE;
		m_pixelSize = 4;
		break;
	case FxRTextureFormat_BGRA32:
		m_pixelIntFormatGL = GL_RGBA;
		m_pixelFormatGL = GL_BGRA;
		m_pixelTypeGL = GL_UNSIGNED_BYTE;
		m_pixelSize = 4;
		break;
	case FxRTextureFormat_ARGB32:
		m_pixelIntFormatGL = GL_RGBA;
		m_pixelFormatGL = GL_BGRA;
		m_pixelTypeGL = GL_UNSIGNED_INT_8_8_8_8; // GL_UNSIGNED_INT_8_8_8_8_REV on big-endian.
		m_pixelSize = 4;
		break;
	//case FxRTextureFormat_ABGR32: // Needs GL_EXT_abgr
	//	m_pixelIntFormatGL = GL_RGBA;
	//	m_pixelFormatGL = GL_ABGR_EXT;
	//	m_pixelTypeGL = GL_UNSIGNED_BYTE;
	//	m_pixelSize = 4;
	//	break;
	case FxRTextureFormat_RGB24:
		m_pixelIntFormatGL = GL_RGB;
		m_pixelFormatGL = GL_RGB;
		m_pixelTypeGL = GL_UNSIGNED_BYTE;
		m_pixelSize = 3;
		break;
	case FxRTextureFormat_BGR24:
		m_pixelIntFormatGL = GL_RGBA;
		m_pixelFormatGL = GL_BGR;
		m_pixelTypeGL = GL_UNSIGNED_BYTE;
		m_pixelSize = 3;
		break;
	case FxRTextureFormat_RGBA4444:
		m_pixelIntFormatGL = GL_RGBA;
		m_pixelFormatGL = GL_RGBA;
		m_pixelTypeGL = GL_UNSIGNED_SHORT_4_4_4_4;
		m_pixelSize = 2;
		break;
	case FxRTextureFormat_RGBA5551:
		m_pixelIntFormatGL = GL_RGBA;
		m_pixelFormatGL = GL_RGBA;
		m_pixelTypeGL = GL_UNSIGNED_SHORT_5_5_5_1;
		m_pixelSize = 2;
		break;
	case FxRTextureFormat_RGB565:
		m_pixelIntFormatGL = GL_RGB;
		m_pixelFormatGL = GL_RGB;
		m_pixelTypeGL = GL_UNSIGNED_SHORT_5_6_5;
		m_pixelSize = 2;
		break;
	default:
		break;
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
	m_buf = (uint8_t *)calloc(1, m_size.w * m_size.h * m_pixelSize);
}

void* FxRWindowGL::getNativePtr() {
#pragma warning(disable:4312) // Disable size-extension warning, it's the desired behaviour.
	return (void *)m_texID;
#pragma warning(default:4312)
}

void FxRWindowGL::requestUpdate(float timeDelta) {

	// Auto-generate a dummy texture. A 100 x 100 square, oscillating in x dimension.
	static int k = 0;
	int i, j;
	k++;
	if (k > 100) k = -100;
	memset(m_buf, 255, m_size.w * m_size.h * m_pixelSize); // Clear to white.
	// Assumes RGBA32!
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

	// Setup a texture ourselves:
	//glGenTextures(1, &m_texID);
	//glBindTexture(GL_TEXTURE_2D, m_texID);
	//glActiveTexture(GL_TEXTURE0);
	//glTexImage2D(GL_TEXTURE_2D, 0, m_pixelIntFormatGL, m_size.w, m_size.h, 0, m_pixelFormatGL, m_pixelTypeGL, m_buf);
	////glTexImage2D(GL_TEXTURE_RECTANGLE, 0, m_pixelIntFormatGL, m_size.w, m_size.h, 0, m_pixelFormatGL, m_pixelTypeGL, m_buf);
	// Would also later require cleanup:
	//glBindTexture(GL_TEXTURE_2D, 0);
	//glDeleteTextures(1, &m_texID);
	//m_texID = 0;

	glBindTexture(GL_TEXTURE_2D, m_texID);
	glActiveTexture(GL_TEXTURE0);
	glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, m_size.w, m_size.h, GL_RGBA, GL_UNSIGNED_BYTE, m_buf);
	//glTexSubImage2D(GL_TEXTURE_RECTANGLE, 0, 0, 0, m_size.w, m_size.h, GL_RGBA, GL_UNSIGNED_BYTE, m_buf);
}

void FxRWindowGL::pointerEnter() {
	FXRLOGi("FxRWindowGL::pointerEnter()\n");
}

void FxRWindowGL::pointerExit() {
	FXRLOGi("FxRWindowGL::pointerExit()\n");
}

void FxRWindowGL::pointerOver(int x, int y) {
	FXRLOGi("FxRWindowGL::pointerOver(%d, %d)\n", x, y);
}

void FxRWindowGL::pointerPress(int x, int y) {
	FXRLOGi("FxRWindowGL::pointerPress(%d, %d)\n", x, y);
}

void FxRWindowGL::pointerRelease(int x, int y) {
	FXRLOGi("FxRWindowGL::pointerRelease(%d, %d)\n", x, y);
}

