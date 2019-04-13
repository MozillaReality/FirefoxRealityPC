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
  int ret = 0;
  _RPTF2(_CRT_WARN, "Starting FxRHost with args \"%S\" at PID 0n%d\n", lpCmdLine, ::GetCurrentProcessId());

  int nArgs;
  LPWSTR *szArglist = CommandLineToArgvW(GetCommandLineW(), &nArgs);
  if (szArglist != nullptr)
  {
    LPWSTR pszFxPath = nullptr;
    LPWSTR pszFxProfile = nullptr;

    // Skip first arg, which is always this executable
    for (int cArg = 1; cArg < nArgs; cArg++)
    {
      if (wcscmp(szArglist[cArg], ARG_FXPATH) == 0)
      {
        pszFxPath = szArglist[++cArg];
      }
      else if (wcscmp(szArglist[cArg], ARG_FXPROFILE) == 0)
      {
        pszFxProfile = szArglist[++cArg];
      }
      else
      {
        assert(!"Unsupported arg");
      }    
    }

    if (pszFxPath != nullptr && pszFxProfile != nullptr)
    {
      _RPTF0(_CRT_WARN, "  Starting FxRHost main process\n");

      FxRHostWindow win;
      if (win.Create(FXRHOST_NAME_WIDE, WS_OVERLAPPEDWINDOW, 0, 50, 50, 400, 100))
      {
        win.OnCreate(pszFxPath, pszFxProfile);

        ShowWindow(win.Window(), nCmdShow);

        Pump();

        // TODO: Need a way to terminate Fx safely without triggering crash recovery
        //win.TerminateChildProcs();
      }
    }
    else {
      assert(!"Missing args");
    }

    LocalFree(szArglist);
  }
  else {
    assert(!"Invalid Args");
  }

  return ret;
}
