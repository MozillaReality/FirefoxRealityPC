#ifdef __APPLE__
#  include <OpenGL/gl3.h>
#else
#  ifndef _WIN32
#    define GL_GLEXT_PROTOTYPES
#  endif 
#  include <GL/glcorearb.h>
#endif

#include "FxRWindowGL.h"

FxRWindowGL::FxRWindowGL() :
	m_TexID(0) 
{

}

FxRWindowGL::~FxRWindowGL() {

}

FxRWindow::Size FxRWindowGL::size() {
	return {0, 0};
}

void FxRWindowGL::setSize(FxRWindow::Size size) {

}

void* FxRWindowGL::getNativeID() {
	return (void *)m_TexID;
}

