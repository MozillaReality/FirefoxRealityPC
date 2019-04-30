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
    m_dwPidGPU(0),
    m_hThreadInput(nullptr),
    m_rcFx(),
    m_ptLastMouse(),
    m_fExitInputThread(false)
  {
  }

  void Init(HWND hwndHost);

  void TryStartInputThread();
  void EndInputThread() { m_fExitInputThread = true; }

  void SetDrawPID(DWORD pid);
  void SetFxHwnd(HWND fx, bool fHasCustomUI);
  HWND GetFxHwnd() const { return m_hwndFx; }

  int32_t GetAdapterIndex() const { return m_dxgiAdapterIndex; }
  vr::VROverlayHandle_t GetOverlayHandle() const { return m_ulOverlayHandle; }

private:
  void CreateOverlay();

  void ShowVirtualKeyboard();

  static DWORD WINAPI InputThreadProc(_In_ LPVOID lpParameter);

  void OverlayPump();
  void CheckOverlayMouseScale();
  void ProcessMouseEvent(vr::VREvent_t vrEvent);

  // OpenVR state
  vr::IVRSystem* m_pHMD;
  int32_t m_dxgiAdapterIndex;
  vr::VROverlayHandle_t m_ulOverlayHandle;
  vr::VROverlayHandle_t m_ulOverlayThumbnailHandle;

  // Window/Process State for Host and Firefox
  HWND m_hwndHost;
  HWND m_hwndFx;
  UINT m_cHwndFx;
  DWORD m_dwPidGPU;

  // OpenVR Input and Input Thread State
  HANDLE m_hThreadInput;
  // Note: the following 2 variable should only be accessed on OpenVR polling thread
  RECT m_rcFx;
  // OpenVR scroll event doesn't provide the position of the controller on the
  // overlay, so keep track of the last-known position to use with the scroll
  // event
  POINT m_ptLastMouse;
  // Set to true during shutdown to indicate to Input Thread to stop polling for
  // and sending messages
  volatile bool m_fExitInputThread;

  // Helper static variable to prevent OpenVR from starting for certain debug cases
  static bool s_isEnabled;
};
