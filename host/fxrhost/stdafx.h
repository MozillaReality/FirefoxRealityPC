// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

template <class T> void SafeRelease(T **ppT)
{
  if (*ppT)
  {
    (*ppT)->Release();
    *ppT = nullptr;
  }
}

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>

// C RunTime Header Files
#include <stdlib.h>
#include <malloc.h>
#include <memory.h>
#include <tchar.h>


// reference additional headers your program requires here
#include <assert.h>
#include <crtdbg.h>
#include <shellapi.h>

#define FXRHOST_NAME      "FxRHost"
#define FXRHOST_NAME_WIDE L"FxRHost"
#define ARG_FXPATH        L"--fxpath"
#define ARG_FXPROFILE     L"--fxprofile"
#define ARG_FXRUI         L"--fxrui"

// Messages passed from Fx
#define WM_OVR_DRAWPID    (WM_USER+9)
#define WM_OVR_FXHWND     (WM_USER + 10)