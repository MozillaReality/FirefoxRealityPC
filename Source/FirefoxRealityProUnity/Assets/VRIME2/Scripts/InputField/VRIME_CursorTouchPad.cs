// ========================================================================== //
//  Created: 2019-06-25
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using Valve.VR;

	public class VRIME_CursorTouchPad : MonoBehaviour
	{
		private bool isMoving = false;
		private float pressTime;
		private const float longPressTime = 1;
		private const float moveSlowDelayTime = 0.2f;
		private const float moveFastDelayTime = 0.05f;
		private bool isFirstMove = false;		
		private bool pressLeft = false;
		private bool pressRight = false;

		private const float effectiveTouchRange = 0.5f; // touchpad outside 50% area
		private const int effectAngleRange = 30; // ex right angle is  90 +- 30 = 60~120
		
		#region unity function
		private void Update() {
			if(VRIME_Manager.Ins.MoveCursorByTouchpad)
			{
				CheckTouchPadPress();
			}
		}
		#endregion

#if steamvr_v2
		private bool leftTouched = false;
		private bool rightTouched = false;
		private Vector2 leftVector = Vector2.zero;
		private Vector2 rightVector = Vector2.zero;

        public void TouchChangeHandler(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            if(fromSource == SteamVR_Input_Sources.LeftHand) {
                leftTouched = newState;
            } else if(fromSource == SteamVR_Input_Sources.RightHand) {
                rightTouched = newState;
            }
        }

		public void TouchAxisHandler(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
			if(fromSource == SteamVR_Input_Sources.LeftHand) {
                leftVector = axis;
            } else if(fromSource == SteamVR_Input_Sources.RightHand) {
                rightVector = axis;
            }
		}
#endif

		private void CheckTouchPadPress() {
#if steamvr_v2			
			int leftIndex = (int)VRIME_Manager.Ins.userControllerLeft.GetDeviceIndex();
			int rightIndex = (int)VRIME_Manager.Ins.userControllerRight.GetDeviceIndex();
#else
			int leftIndex = (int)VRIME_Manager.Ins.userControllerLeft.index;
			int rightIndex = (int)VRIME_Manager.Ins.userControllerRight.index;
#endif			


			bool isLeftPressTouched = false;
			bool isRightPressTouched = false;

#if steamvr_v2
			isLeftPressTouched = leftTouched;
			isRightPressTouched = rightTouched;
#else
			// for vive, use press event, for oculus should use touch event
            VRControllerState_t left_t = new VRControllerState_t();
			VRControllerState_t right_t = new VRControllerState_t();
            
			OpenVR.System.GetControllerState((uint)leftIndex, ref left_t, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t)));
			OpenVR.System.GetControllerState((uint)rightIndex, ref right_t, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t)));
			
			string model_name = UnityEngine.XR.XRDevice.model;
			bool isOculus = (model_name != null && model_name.ToLower().Contains("oculus"));
			bool isViveCosmos = (model_name != null && model_name.ToLower().Contains("cosmos"));

			if(isOculus || isViveCosmos) { // for oculus: buttonTouched, for vive: buttonPressed
				isLeftPressTouched = (left_t.ulButtonTouched & (1UL << ((int)EVRButtonId.k_EButton_SteamVR_Touchpad))) > 0;
				isRightPressTouched = (right_t.ulButtonTouched & (1UL << ((int)EVRButtonId.k_EButton_SteamVR_Touchpad))) > 0;
			} else {
				isLeftPressTouched = (left_t.ulButtonPressed & (1UL << ((int)EVRButtonId.k_EButton_SteamVR_Touchpad))) > 0;
				isRightPressTouched = (right_t.ulButtonPressed & (1UL << ((int)EVRButtonId.k_EButton_SteamVR_Touchpad))) > 0;
			}
#endif			

   
			// bool leftTouchDown = GetTouchPadPress(VRIME_Manager.Ins.userControllerLeft, out leftDevice);
			// bool rightTouchDown = GetTouchPadPress(VRIME_Manager.Ins.userControllerRight, out rightDevice);



			if(isLeftPressTouched && isRightPressTouched) // can't handle two touchpad pressed
				return;



			if(isLeftPressTouched || isRightPressTouched) {
				// use openvr
				Vector2 vector = new Vector2();
#if steamvr_v2				
				if(isLeftPressTouched) {
					vector = leftVector;
				} else if(isRightPressTouched) {
					vector = rightVector;
				}
#else		
				if(isLeftPressTouched) {
					vector.x = left_t.rAxis0.x;
					vector.y = left_t.rAxis0.y;					
				} else if(isRightPressTouched) {
					vector.x = right_t.rAxis0.x;
					vector.y = right_t.rAxis0.y;					
				}
#endif				
				

				//Vector2 vector = touchDownDevice.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);								

				float distance = Vector2.Distance(vector, Vector2.zero);
				if(distance < effectiveTouchRange) {					
					isMoving = false;
					isFirstMove = false;
					if(IsInvoking("moveCursor")) {
						CancelInvoke("moveCursor");
					}					
					return;	
				} 

				float angle = CalculateTouchpadAxisAngle(vector);
				if(angle > (90-effectAngleRange) && angle < (90+effectAngleRange)) 
				{
					//MoveCaret(1);
					if(pressLeft) {
						VRIME_Debugger.Log("change side to right");
						pressLeft = false;
						pressTime = 0; // change side, reset time
					}
					pressRight = true;
				}
				else if(angle > (270-effectAngleRange) && angle < (270+effectAngleRange)) 
				{
					//MoveCaret(-1);
					if(pressRight) {
						VRIME_Debugger.Log("change side to left");
						pressRight = false;
						pressTime = 0; // change side, reset time
					}
					pressLeft = true;
				} else {
					pressLeft = false;
					pressRight = false;
					return;
				}
				
				if(pressLeft || pressRight) {
					if(!isMoving) {
						pressTime = 0;
						isMoving = true;
						isFirstMove = true;
					} else {
						pressTime += Time.deltaTime;
					}
				}


				if(!IsInvoking("moveCursor")) {
					if(pressTime > longPressTime) {
						Invoke("moveCursor", moveFastDelayTime);
					} else {
						if(isFirstMove) {
							Invoke("moveCursor", 0);	
							isFirstMove = false;
						} else {
							Invoke("moveCursor", moveSlowDelayTime);
						}
						
					}
				}
			} else {
				isMoving = false;	
				isFirstMove = false;
				if(IsInvoking("moveCursor")) {
					CancelInvoke("moveCursor");
				}
			}
		}

		private void moveCursor(){
			if(pressLeft && !pressRight) {
				VRIME_InputFieldOversee.Ins.MoveCaret(-1);
			}

			else if(!pressLeft && pressRight) {
				VRIME_InputFieldOversee.Ins.MoveCaret(1);
			}	

					
		}

		// private bool GetTouchPadPress(SteamVR_TrackedObject controller, out SteamVR_Controller.Device device)
		// {
		// 	device = null;
        //     if(controller == null)// If Not StemVR Case will be null.
        //         return false;
        //     if (controller.index == SteamVR_TrackedObject.EIndex.None)
        //         return false;

        //     int i = (int)controller.index;
			

        //     device = SteamVR_Controller.Input(i);	
		// 	string model_name = UnityEngine.XR.XRDevice.model;
		// 	if(model_name != null && model_name.ToLower().Contains("oculus"))
		// 	{
		// 		return device.GetTouch(EVRButtonId.k_EButton_SteamVR_Touchpad);
		// 	} else {
		// 		return device.GetPress(EVRButtonId.k_EButton_SteamVR_Touchpad);
		// 	}

			
		// }

		// this code copy from VRTK
		private float CalculateTouchpadAxisAngle(Vector2 axis)
		{
			float angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg;
			angle = 90.0f - angle;
			if (angle < 0)
			{
				angle += 360.0f;
			}
			return angle;
		}
	}
}