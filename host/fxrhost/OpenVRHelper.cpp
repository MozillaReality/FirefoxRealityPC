// OpenVRHelper.cpp
//
// This file contains code to interact with OpenVR overlays.

#include "stdafx.h"
#include "OpenVRHelper.h"

// Class-wide override to enable calls made to OpenVR
bool OpenVRHelper::s_isEnabled = true;

#define FXR_UI_HWND_COUNT 2
#define FX_DESKTOP_HWND_COUNT 1

// Setup the initial interaction with OpenVR
void OpenVRHelper::Init(HWND hwndHost)
{
  if (s_isEnabled)
  {
    m_hwndHost = hwndHost;

    // COpenVROverlayController::Init
    vr::EVRInitError eError = vr::VRInitError_None;
    m_pHMD = vr::VR_Init(&eError, vr::VRApplication_Overlay);
    if (eError == vr::VRInitError_None)
    {
      // TODO: May need to assert a particular index until this is supported
      // between the two products.
      m_pHMD->GetDXGIOutputInfo(&m_dxgiAdapterIndex);
      assert(m_dxgiAdapterIndex != -1);

      CreateOverlay();
    }
    else
    {
      assert(!"Failed to initialze OpenVR");
    }
  }
  else
  {
    _RPTF0(_CRT_WARN, "\n\t**OpenVRHelper is disabled for this session.\n");
  }
}

// Create the VROverlay and set the properties
void OpenVRHelper::CreateOverlay()
{
  if (s_isEnabled)
  {
    if (vr::VROverlay() != nullptr)
    {
      std::string sKey = std::string(FXRHOST_NAME);
      vr::VROverlayError overlayError = vr::VROverlayError_None;

      overlayError = vr::VROverlay()->CreateDashboardOverlay(
        sKey.c_str(),
        sKey.c_str(),
        &m_ulOverlayHandle,
        &m_ulOverlayThumbnailHandle
      );

      if (overlayError == vr::VROverlayError_None)
      {
        overlayError = vr::VROverlay()->SetOverlayWidthInMeters(m_ulOverlayHandle, 2.5f);
        if (overlayError == vr::VROverlayError_None)
        {
          overlayError = vr::VROverlay()->SetOverlayFlag(m_ulOverlayHandle, vr::VROverlayFlags_VisibleInDashboard, true);
          if (overlayError == vr::VROverlayError_None)
          {
            overlayError = vr::VROverlay()->SetOverlayInputMethod(m_ulOverlayHandle, vr::VROverlayInputMethod_Mouse);
            if (overlayError == vr::VROverlayError_None)
            {
              char rgchKey[vr::k_unVROverlayMaxKeyLength] = { 0 };
              vr::VROverlay()->GetOverlayKey(m_ulOverlayHandle, rgchKey, ARRAYSIZE(rgchKey), &overlayError);
              if (overlayError == vr::VROverlayError_None)
              {
                overlayError = vr::VROverlay()->SetOverlayFlag(m_ulOverlayHandle, vr::VROverlayFlags_SendVRScrollEvents, true);
                if (overlayError == vr::VROverlayError_None)
                {
                  vr::VROverlay()->ShowDashboard(rgchKey);
                }
              }
            }
          }
        }
      }

      assert(overlayError == vr::VROverlayError_None);
    }
    else
    {
      assert(!"Failed to get VROverlay");
    }
  }
}

// Set the HWND passed from Firefox, which will be used to pass window messages (typicaly,
// input messages) back to Firefox. 
void OpenVRHelper::SetFxHwnd(HWND fx, bool fHasCustomUI)
{
  // TODO: Need to understand which HWND to send from the Fx side. Firefox sends multiple unique
  // HWNDs, which changes depending on if --chrome vs default is used.
  UINT compareCount = fHasCustomUI ? FXR_UI_HWND_COUNT : FX_DESKTOP_HWND_COUNT;
  if (++m_cHwndFx == compareCount) {
    m_hwndFx = fx;
  }
}

// Show the Virtual Keyboard in the HMD
void OpenVRHelper::ShowVirtualKeyboard()
{
  // Note: bUseMinimalMode set to true so that each char arrives as a separate event.
  vr::VROverlayError overlayError = vr::VROverlay()->ShowKeyboardForOverlay(
    m_ulOverlayHandle,
    vr::k_EGamepadTextInputModeNormal,
    vr::k_EGamepadTextInputLineModeSingleLine,
    FXRHOST_NAME, // pchDescription,
    100, // unCharMax,
    "", // pchExistingText,
    true, // bUseMinimalMode
    0 //uint64_t uUserValue
  );
}



// Spins up a new thread so that polling of input events can happen without
// blocking the UI thread.
void OpenVRHelper::TryStartInputThread() {
  // Only spin up the new thread if
  // - Using OpenVR is enabled
  // - Both the Fx HWND and GPU PID have been set
  // - An Input Thread has not already been started
  // - The Input thread is allowed to run (i.e., should not be exiting)
  if (s_isEnabled && m_hwndFx != nullptr && m_dwPidGPU != 0 && m_hThreadInput == nullptr && !m_fExitInputThread) {
    // Assert that the following variables are already set before spinning
    // up a new thread
    assert(m_hwndFx != nullptr);
    assert(m_ulOverlayHandle != vr::k_ulOverlayHandleInvalid);
    // Assert that the following variables are not set because they should
    // only be modified and accessed on the new thread
    assert(::IsRectEmpty(&m_rcFx));
    assert(m_ptLastMouse.x == 0 && m_ptLastMouse.y == 0);

    DWORD dwTid = 0;
    m_hThreadInput =
      CreateThread(
        nullptr,  // LPSECURITY_ATTRIBUTES lpThreadAttributes
        0,        // SIZE_T dwStackSize,
        OpenVRHelper::InputThreadProc,
        this,  //__drv_aliasesMem LPVOID lpParameter,
        0,     // DWORD dwCreationFlags,
        &dwTid);

    if (m_hThreadInput == nullptr) {
      DebugBreak();
    }
    else {
      SetThreadDescription(m_hThreadInput, L"OpenVR Input");
    }
  }
}

// ThreadProc to handle Overlay event messages
DWORD OpenVRHelper::InputThreadProc(_In_ LPVOID lpParameter) {
  OpenVRHelper* pInstance = static_cast<OpenVRHelper*>(lpParameter);

  // HACK: For now, scrolling only works when the mouse cursor is located
  // over the window (because scroll code in Fx does a hittest for HWND
  // under the cursor). So...programmatically make this so.
  ::SetFocus(pInstance->m_hwndFx);
  RECT rcHwndFx;
  if (::GetWindowRect(pInstance->m_hwndFx, &rcHwndFx)) {
    ::SetCursorPos(rcHwndFx.left + 50, rcHwndFx.top + 50);
  }

  while (!pInstance->m_fExitInputThread) {
    pInstance->OverlayPump();
  }

  ::ExitThread(0);
}

// Serves as a simple way to retrieve the size of the Firefox window/texture/client area for
// relevant translation of input events.
void OpenVRHelper::CheckOverlayMouseScale() {
  // Need to find a better place to put this. The problem that needs to be
  // solved is knowing the texture size so that mouse coords can be translated
  // late. This is put in this function because it won't block the UI thread.
  // .right is compared to <= 1 because
  // - if == 0, then uninitialized
  // - if == 1, then mousescale hasn't been set by GPU process yet (default
  // normalizes to 1.0f)
  if (m_rcFx.right <= 1) {
    vr::HmdVector2_t vecWindowSize = { 0 };
    vr::EVROverlayError error = vr::VROverlay()->GetOverlayMouseScale(
      m_ulOverlayHandle, &vecWindowSize);

    if (error == vr::VROverlayError_None) {
      m_rcFx.right = vecWindowSize.v[0];
      m_rcFx.bottom = vecWindowSize.v[1];
    }
    else {
      DebugBreak();
    }
  }
}

// This function polls the Overlay for any events that are pending. Some events are
// forwarded to Firefox as window messages for UI interaction.
void OpenVRHelper::OverlayPump()
{
  assert(s_isEnabled);
  assert(vr::VROverlay() != nullptr && m_hwndFx != nullptr);
  
  CheckOverlayMouseScale();

  vr::VREvent_t vrEvent;
  while (vr::VROverlay()->PollNextOverlayEvent(m_ulOverlayHandle, &vrEvent, sizeof(vrEvent)))
  {
    // _RPTF1(_CRT_WARN, "VREvent_t.eventType: %s\n", vr::VRSystem()->GetEventTypeNameFromEnum((vr::EVREventType)(vrEvent.eventType)));
    switch (vrEvent.eventType)
    {
    case vr::VREvent_MouseMove:
    case vr::VREvent_MouseButtonUp:
    case vr::VREvent_MouseButtonDown: {
      ProcessMouseEvent(vrEvent);
      break;
    }

    case vr::VREvent_Scroll: {
      vr::VREvent_Scroll_t data = vrEvent.data.scroll;
      SHORT scrollDelta = WHEEL_DELTA * (SHORT)data.ydelta;

      // Route this back to the Firefox window for processing
      ::PostMessage(m_hwndFx, WM_MOUSEWHEEL, MAKELONG(0, scrollDelta), POINTTOPOINTS(m_ptLastMouse));
      break;
    }

    case vr::VREvent_KeyboardCharInput:
    {
      vr::VREvent_Keyboard_t data = vrEvent.data.keyboard;
      _RPTF1(_CRT_WARN, "  VREvent_t.data.keyboard.cNewInput --%s--\n", data.cNewInput);

      // Route this back to the Firefox window for processing
      ::PostMessage(m_hwndFx, WM_CHAR, data.cNewInput[0], 0);
      break;
    }

    case vr::VREvent_ButtonPress:
    {
      // This button press causes the virtual keyboard to be manually invoked.
      vr::VREvent_Controller_t data = vrEvent.data.controller;
      if (data.button == 2) {
        ShowVirtualKeyboard();
      }
      break;
    }
    }
  }
}

// This function handles the common code for processing Mouse events from the Overlay
void OpenVRHelper::ProcessMouseEvent(vr::VREvent_t vrEvent) {
  vr::VREvent_Mouse_t data = vrEvent.data.mouse;

  // Windows' origin is top-left, whereas OpenVR's origin is
  // bottom-left, so transform the y-coordinate.
  m_ptLastMouse.x = (LONG)(data.x);
  m_ptLastMouse.y = m_rcFx.bottom - (LONG)(data.y);

  UINT nMsg;
  if (vrEvent.eventType == vr::VREvent_MouseMove) {
    nMsg = WM_MOUSEMOVE;
  }
  else if (vrEvent.eventType == vr::VREvent_MouseButtonDown) {
    nMsg = WM_LBUTTONDOWN;
  }
  else if (vrEvent.eventType == vr::VREvent_MouseButtonUp) {
    nMsg = WM_LBUTTONUP;
  }
  else {
    DebugBreak();
  }

  // Route this back to the Firefox window for processing
  ::PostMessage(m_hwndFx, nMsg, 0, POINTTOPOINTS(m_ptLastMouse));
}

// In order to draw from a process that didn't create the overlay, SetOverlayRenderingPid must be called
// for that process to allow it to render to the overlay texture.
void OpenVRHelper::SetDrawPID(DWORD pid)
{
  m_dwPidGPU = pid;
  if (s_isEnabled)
  {
    vr::VROverlayError error = vr::VROverlay()->SetOverlayRenderingPid(m_ulOverlayHandle, m_dwPidGPU);
    assert(error == vr::VROverlayError_None);
  }
}