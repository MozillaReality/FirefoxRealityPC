// OpenVRHelper.cpp
//
// This file contains code to interact with OpenVR, specifically to set the texture
// used for the VROverlay.

#include "stdafx.h"
#include "OpenVRHelper.h"

// Class-wide override to enable calls made to OpenVR
bool OpenVRHelper::s_isEnabled = true;

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


void OpenVRHelper::CreateOverlay()
{
  if (s_isEnabled)
  {
    if (vr::VROverlay() != nullptr)
    {
      std::string sKey = std::string("FxRHost");
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

// Post a custom VR_POLL msg for asynchronous processing
void OpenVRHelper::PostVRPollMsg()
{
  if (s_isEnabled)
  {
    bool success = PostMessage(m_hwndHost, WM_VR_POLL, 0, 0);
    assert(success || !"Failed to post VR msg");
  }
}

void OpenVRHelper::SetFxHwnd(HWND fx)
{
  // Need to understand which HWND to send from the Fx side. There can be multiple allocated, but not sure which is the right one.
  if (++m_cHwndFx == 1) {
    m_hwndFx = fx;
  }
}

void OpenVRHelper::ShowVirtualKeyboard()
{
  // Note: bUseMinimalMode set to true so that each char arrives as an event.
  vr::VROverlayError overlayError = vr::VROverlay()->ShowKeyboardForOverlay(
    m_ulOverlayHandle,
    vr::k_EGamepadTextInputModeNormal,
    vr::k_EGamepadTextInputLineModeSingleLine,
    "FxRHost", // pchDescription,
    100, // unCharMax,
    "", // pchExistingText,
    true, // bUseMinimalMode
    0 //uint64_t uUserValue
  );
}

// COpenVROverlayController::OnTimeoutPumpEvents()
void OpenVRHelper::OverlayPump()
{
  assert(s_isEnabled);

  if (vr::VROverlay() != nullptr && m_hwndFx != nullptr)
  {
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


    vr::VREvent_t vrEvent;
    while (vr::VROverlay()->PollNextOverlayEvent(m_ulOverlayHandle, &vrEvent, sizeof(vrEvent)))
    {
      // _RPTF1(_CRT_WARN, "VREvent_t.eventType: %s\n", vr::VRSystem()->GetEventTypeNameFromEnum((vr::EVREventType)(vrEvent.eventType)));
      switch (vrEvent.eventType)
      {
      case vr::VREvent_MouseMove:
      case vr::VREvent_MouseButtonUp:
      case vr::VREvent_MouseButtonDown: {
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
        PostMessage(m_hwndFx, nMsg, 0, POINTTOPOINTS(m_ptLastMouse));

        break;
      }

      case vr::VREvent_Scroll: {
        vr::VREvent_Scroll_t data = vrEvent.data.scroll;
        SHORT scrollDelta = WHEEL_DELTA * (short)data.ydelta;

        PostMessage(m_hwndFx, WM_MOUSEWHEEL, MAKELONG(0, scrollDelta), POINTTOPOINTS(m_ptLastMouse));
        break;
      }

      case vr::VREvent_KeyboardCharInput:
      {
        vr::VREvent_Keyboard_t data = vrEvent.data.keyboard;
        _RPTF1(_CRT_WARN, "  VREvent_t.data.keyboard.cNewInput --%s--\n", data.cNewInput);

        // Route this back to main window for processing
        PostMessage(m_hwndFx, WM_CHAR, data.cNewInput[0], 0);
        break;
      }

      case vr::VREvent_ButtonPress:
      {
        vr::VREvent_Controller_t data = vrEvent.data.controller;
        if (data.button == 2) {
          ShowVirtualKeyboard();
        }

        break;
      }
      }
    }
  }

  PostVRPollMsg();
}

// In order to draw from a process that didn't create the overlay, SetOverlayRenderingPid must be called
// for that process to allow it to render to the overlay texture.
void OpenVRHelper::SetDrawPID(DWORD pid)
{
  if (s_isEnabled)
  {
    vr::VROverlayError error = vr::VROverlay()->SetOverlayRenderingPid(m_ulOverlayHandle, pid);
    assert(error == vr::VROverlayError_None);
  }
}