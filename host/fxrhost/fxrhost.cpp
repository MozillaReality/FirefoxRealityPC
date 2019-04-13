// fxrhost.cpp
// 
// FxRHost is a Win32 app built for use as a VROverlay with OpenVR that
// that also hosts and interacts with Desktop Firefox.
//
// Code in this file is based upon the following samples
//   https://github.com/thomasmo/SampleVRO
//
//
// This app involves 3 processes:
// - Host: Interacts with Win32 and OpenVR
// - FxMain: Owns the window/HWND of Firefox
// - FxGPU: The GPU and compositor process of Firefox
//
// The bootstrapping of the processes are as follows:
// 
//            Host                     FxMain                   FxGPU
//              |
//       Init OpenVR System,
//       Create Overlay
//              |
//       Launch Firefox, send    --------+------------------------|
//       Host HWND and Overlay ID  ------+------------------------|            
//              |                        |                        |
//              |<---------------- Send FxMain HWND               |
//              |                        |                        |
//              |<----------------------------------------- Send GPU PID
//        Give FxGPU permission to       |                        |
//        present to Overlay             |                        |
//              |                        |            Web content is presented
//              |                        |                 to OpenVR overlay
//              |                        |                   ^----|
//        Poll and send  -------> Receive input
//        OpenVR input msgs       Win32 messsages 
//              ^------------------------|
//

#include "stdafx.h"
#include "fxrhost.h"

void FxRHostWindow::OnCreate(LPWSTR pszFxPath, LPWSTR pszFxProfile)
{
  ovrHelper.Init(Window());

  WCHAR fxCmd[MAX_PATH] = { 0 };
  int err = swprintf_s(
    fxCmd,
    ARRAYSIZE(fxCmd),
    L"%s -no-remote -wait-for-browser -profile %s -fxr 0x%p -overlayid 0x%p",
    pszFxPath,
    pszFxProfile,
    Window(),
    ovrHelper.GetOverlayHandle()
  );
  assert(err > 0);

  STARTUPINFO startupInfoFx = { 0 };
  bool fCreateContentProc = ::CreateProcess(
    nullptr,  // lpApplicationName,
    fxCmd,
    nullptr,  // lpProcessAttributes,
    nullptr,  // lpThreadAttributes,
    TRUE,     // bInheritHandles,
    0,        // dwCreationFlags,
    nullptr,  // lpEnvironment,
    nullptr,  // lpCurrentDirectory,
    &startupInfoFx,
    &procInfoFx
  );

  assert(fCreateContentProc);
}

// Synchronously terminate the new processes
void FxRHostWindow::TerminateChildProcs()
{
  ::TerminateProcess(procInfoFx.hProcess, 0);
  ::WaitForSingleObject(procInfoFx.hProcess, 10000);
}

LRESULT FxRHostWindow::HandleMessage(UINT uMsg, WPARAM wParam, LPARAM lParam)
{
  switch (uMsg)
  {
  case WM_DESTROY:
    PostQuitMessage(0);
    return 0;

  case WM_OVR_DRAWPID:
    ovrHelper.SetDrawPID(wParam);
    ovrHelper.StartInputThread();
    return 0;

  case WM_OVR_FXHWND:
    ovrHelper.SetFxHwnd((HWND)wParam);
    return 0;
  }
  return DefWindowProc(m_hwnd, uMsg, wParam, lParam);
}
