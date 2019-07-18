// OpenVRHelper.h
//
// This file contains the declaration of OpenVRHelper, which manages
// interaction with an OpenVR overlay

#pragma once

#include "openvr/headers/openvr.h"

// from vrhostex.h
typedef void(*PFN_CREATEVRWINDOW)(UINT* windowId, HANDLE* hTex, HANDLE* hEvt, uint64_t* width, uint64_t* height);
typedef void(*PFN_CLOSEVRWINDOW)(UINT nVRWindow);
typedef void(*PFN_SENDUIMESSAGE)(UINT nVRWindow, UINT msg, uint64_t wparam, uint64_t lparam);

class OpenVRHelper {
public:
  OpenVRHelper() :
    m_pHMD(nullptr),
    m_dxgiAdapterIndex(-1),
    m_ulOverlayHandle(vr::k_ulOverlayHandleInvalid),
    m_ulOverlayThumbnailHandle(vr::k_ulOverlayHandleInvalid),
    m_hwndHost(nullptr),
    m_hSignal(nullptr),
    m_vrWin(0),
    m_hThreadInput(nullptr),
    m_hThreadDraw(nullptr),
    m_rcFx(),
    m_ptLastMouse(),
    m_fExitInputThread(false),
    m_fExitDrawThread(false)
  {
  }

  void Init(HWND hwndHost);
  void CloseFxWindow();

  void TryStartInputThread();
  void EndInputThread() { m_fExitInputThread = true; }

  void StartDrawThread();
  void EndDrawThread() { m_fExitDrawThread = true; }
  vr::VROverlayError SetOverlayTexture(HANDLE hTex);


  int32_t GetAdapterIndex() const { return m_dxgiAdapterIndex; }
  vr::VROverlayHandle_t GetOverlayHandle() const { return m_ulOverlayHandle; }

private:
  void CreateOverlay();

  void ShowVirtualKeyboard();

  static DWORD WINAPI InputThreadProc(_In_ LPVOID lpParameter);
  static DWORD WINAPI DrawThreadProc(_In_ LPVOID lpParameter);

  void OverlayPump();
  void CheckOverlayMouseScale();
  void ProcessMouseEvent(vr::VREvent_t vrEvent);


  // OpenVR state
  vr::IVRSystem* m_pHMD;
  int32_t m_dxgiAdapterIndex;
  vr::VROverlayHandle_t m_ulOverlayHandle;
  vr::VROverlayHandle_t m_ulOverlayThumbnailHandle;

  // vrhost.dll Members
  HINSTANCE m_hVRHost;
  PFN_SENDUIMESSAGE m_pfnSendUIMessage;

  // Window/Process State for Host and Firefox
  HWND   m_hwndHost;
  UINT   m_vrWin;
  HANDLE m_hTex;
  HANDLE m_hSignal;

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

  // OpenVR Render Thread State
  HANDLE m_hThreadDraw;
  volatile bool m_fExitDrawThread;

  // Helper static variable to prevent OpenVR from starting for certain debug cases
  static bool s_isEnabled;
};
