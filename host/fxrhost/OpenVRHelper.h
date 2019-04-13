#pragma once

#include "Resource.h"
#include "openvr/headers/openvr.h"

class OpenVRHelper {
public:
  void Init(HWND hwndMain, HWND hwndOvr, RECT rc) {}
  void CreateOverlay() {}

  void ShowVirtualKeyboard() {}

  void SetDrawPID(DWORD pid) {}

  void PostVRPollMsg() {}
  void OverlayPump() {}

  void SetFxHwnd(HWND fx) {}

  int GetAdapterIndex() const { return 0; }
  unsigned int GetOverlayHandle() const { return 0; }
};
