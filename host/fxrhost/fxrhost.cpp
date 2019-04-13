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
    ovrHelper.PostVRPollMsg();
    return 0;

  case WM_OVR_FXHWND:
    ovrHelper.SetFxHwnd((HWND)wParam);
    return 0;

  case WM_VR_POLL:
    ovrHelper.OverlayPump();
    return 0;
  }
  return DefWindowProc(m_hwnd, uMsg, wParam, lParam);
}
