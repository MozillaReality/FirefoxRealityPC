#pragma once

#include "Resource.h"
#include "openvr/headers/openvr.h"

class OpenVRHelper {
public:
  void Init(HWND hwndHost);
  void CreateOverlay();

  void ShowVirtualKeyboard();

  void SetDrawPID(DWORD pid);

  void PostVRPollMsg();
  void OverlayPump();

  void SetFxHwnd(HWND fx);

  int32_t GetAdapterIndex() const { return m_dxgiAdapterIndex; }
  vr::VROverlayHandle_t GetOverlayHandle() const { return m_ulOverlayHandle; }

private:
  vr::IVRSystem* m_pHMD;
  int32_t m_dxgiAdapterIndex;
  vr::VROverlayHandle_t m_ulOverlayHandle;
  vr::VROverlayHandle_t m_ulOverlayThumbnailHandle;

  HWND m_hwndHost;
  HWND m_hwndFx;
  UINT m_cHwndFx;
  RECT m_rcFx;

  // OpenVR scroll event doesn't provide the position of the controller on the
  // overlay, so keep track of the last-known position to use with the scroll
  // event
  POINT m_ptLastMouse;

  static bool s_isEnabled;
};
