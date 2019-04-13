// OpenVRHelper.h
//
// This file contains the declaration of OpenVRHelper, which manages
// interaction with an OpenVR overlay

#pragma once

#include "openvr/headers/openvr.h"

class OpenVRHelper {
public:
  OpenVRHelper() :
    m_pHMD(nullptr),
    m_dxgiAdapterIndex(-1),
    m_ulOverlayHandle(vr::k_ulOverlayHandleInvalid),
    m_ulOverlayThumbnailHandle(vr::k_ulOverlayHandleInvalid),
    m_hwndHost(nullptr),
    m_hwndFx(nullptr),
    m_cHwndFx(0),
    m_rcFx(),
    m_ptLastMouse()
  {
  }

  void Init(HWND hwndHost);

  void StartInputThread();

  void SetDrawPID(DWORD pid);
  void SetFxHwnd(HWND fx);

  int32_t GetAdapterIndex() const { return m_dxgiAdapterIndex; }
  vr::VROverlayHandle_t GetOverlayHandle() const { return m_ulOverlayHandle; }

private:
  void CreateOverlay();

  void ShowVirtualKeyboard();

  static DWORD WINAPI InputThreadProc(_In_ LPVOID lpParameter);

  void OverlayPump();
  void CheckOverlayMouseScale();
  void ProcessMouseEvent(vr::VREvent_t vrEvent);


  vr::IVRSystem* m_pHMD;
  int32_t m_dxgiAdapterIndex;
  vr::VROverlayHandle_t m_ulOverlayHandle;
  vr::VROverlayHandle_t m_ulOverlayThumbnailHandle;

  HWND m_hwndHost;
  HWND m_hwndFx;
  UINT m_cHwndFx;

  HANDLE hThreadInput;
  // Note: the following 2 variable should only be accessed on OpenVR polling thread
  RECT m_rcFx;
  // OpenVR scroll event doesn't provide the position of the controller on the
  // overlay, so keep track of the last-known position to use with the scroll
  // event
  POINT m_ptLastMouse;

  static bool s_isEnabled;
};
