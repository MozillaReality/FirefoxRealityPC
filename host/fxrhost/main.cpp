#include "stdafx.h"
#include "fxrhost.h"

void Pump()
{
  // Run the message loop.

  MSG msg = { };
  while (GetMessage(&msg, NULL, 0, 0))
  {
    TranslateMessage(&msg);
    DispatchMessage(&msg);
  }

  _RPTF1(_CRT_WARN, "Closing FxRHost at PID 0x%d\n", ::GetCurrentProcessId());
}

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE, PWSTR lpCmdLine, int nCmdShow)
{
  _RPTF2(_CRT_WARN, "Starting FxRHost with args \"%S\" at PID 0n%d\n", lpCmdLine, ::GetCurrentProcessId());

  int nArgs;
  LPWSTR *szArglist = CommandLineToArgvW(GetCommandLineW(), &nArgs);
  if (szArglist == nullptr)
  {
    assert(!"Invalid Args!");
    return 0;
  }

  // This is the main process
  return FxRHostWindow::wWinMain(nCmdShow);
}

int FxRHostWindow::wWinMain(int nCmdShow)
{
  _RPTF0(_CRT_WARN, "  Starting FxRHost main process\n");

  FxRHostWindow win;
  if (!win.Create(L"FxRHost", WS_OVERLAPPEDWINDOW, 0, 50, 50, 640, 320))
  {
    return 0;
  }

  win.OnCreate();

  ShowWindow(win.Window(), nCmdShow);

  Pump();

  win.TerminateChildProcs();

  return 0;
}
