#include "stdafx.h"
#include "fxrhost.h"

void FxRHostWindow::OnCreate()
{
  ovrHelper.Init(Window());

  WCHAR ffCmd[MAX_PATH] = { 0 };
  int err = swprintf_s(
    ffCmd,
    ARRAYSIZE(ffCmd),
    L"%s -fxr 0x%p -overlayid 0x%p",
    L"e:\\src2\\gecko_build_debug\\dist\\bin\\firefox.exe -no-remote -wait-for-browser -profile e:\\src2\\gecko_build_debug\\tmp\\profile-default ",
    Window(),
    ovrHelper.GetOverlayHandle()
  );
  assert(err > 0);

  STARTUPINFO startupInfoFx = { 0 };
  bool fCreateContentProc = ::CreateProcess(
    nullptr, // lpApplicationName,
    ffCmd,
    nullptr, // lpProcessAttributes,
    nullptr, // lpThreadAttributes,
    TRUE, // bInheritHandles,
    0, // dwCreationFlags,
    nullptr, // lpEnvironment,
    nullptr, // lpCurrentDirectory,
    &startupInfoFx,
    &procInfoFx
  );
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
