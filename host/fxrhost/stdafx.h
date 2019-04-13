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

#define OVR_PROC	L"--ovr"
#define DRAW_PROC	L"--draw"

// https://stackoverflow.com/questions/293723/how-could-i-create-a-custom-windows-message
#define WM_VR_POLL				(WM_USER+0)
#define WM_OVR_DRAWPID    (WM_USER+9)
#define WM_OVR_FXHWND (WM_USER + 10)