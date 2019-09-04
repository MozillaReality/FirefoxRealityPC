// ========================================================================== //
//  Created: 2019-01-10
// ========================================================================== //
//  Copy From VRIME Ver 1
//  Copyright 2018 HTC Xindian HTC1
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	[AddComponentMenu("VRIME/IMESystem/Tracking(Head) Object")]
	public class VRIME_TrackingObject : MonoBehaviour
	{
		#region enum Field
		public enum Mode
        {
            LookAt,
            Follow
        }

        public enum Rotation
        {
            None,
            HorizontalOnly,
            VerticalOnly,
            Full,
        }
		#endregion
		#region pulbic field
		[HideInInspector]
        public Transform trackedHeadEye;
		#endregion
		#region private field
		[SerializeField]
        private Mode mode = Mode.Follow;
        [SerializeField]
        private Rotation rotation = Rotation.HorizontalOnly;
		[SerializeField]
        [Range(0, 1)]
        private float movementSmoothing = 0;
        [SerializeField]
        private float distanceForward = 0.525f;
        [SerializeField]
        private float distanceUp = -0.525f;
		// Track
		private Transform trackingObject;
		#endregion
        
		#region Unity Function
		private void Awake() {
			trackingObject = this.transform;
		}
		#endregion
        #region Public Function
		public void UpdatePosition()
		{
			if(trackedHeadEye == null)
				return;
			VRIME_Debugger.Log(trackedHeadEye.name, "UpdatePosition");
			// Reset Scale
			trackingObject.localScale = Vector3.one;
			// 有點不太清楚為何在 mode != Mode.Follow的時候改判斷 rotation
			//
			if(mode == Mode.Follow)
			{
				ModeIsFollow();
			}
			else if (rotation != Rotation.None)
			{
				ModeNotFollow();
			}
		}
		#endregion
		#region private Function
		private void ModeIsFollow()
		{
			Vector3 tempForward = trackedHeadEye.transform.forward;
			tempForward.y = 0;
			tempForward.Normalize();

			Vector3 newTargetPos = trackedHeadEye.transform.position + (tempForward * distanceForward);
			newTargetPos.y += distanceUp;
			trackingObject.position = newTargetPos;

			if (rotation != Rotation.None)
			{
				var forward = trackedHeadEye.transform.forward;
				var up = trackedHeadEye.transform.up;

				if (rotation == Rotation.HorizontalOnly)
				{
					forward.y = 0;
					up = Vector3.up;
				}

				var targetRot = Quaternion.LookRotation(forward, up);

				trackingObject.rotation = Quaternion.Lerp(targetRot, transform.rotation, movementSmoothing);
			}
		}

		private void ModeNotFollow()
		{
			VRIME_Debugger.LogError(trackedHeadEye.name, "rotation != Rotation.None");
			var globalLookAt = trackedHeadEye.transform.position - transform.position;
			var localLookAt = trackingObject.InverseTransformDirection(-globalLookAt).normalized;

			if (localLookAt.magnitude > 0.01f)
			{
				switch (rotation)
				{
					case Rotation.HorizontalOnly:
						localLookAt.y = 0;
						break;
					case Rotation.VerticalOnly:
						localLookAt.x = 0;
						break;
				}

				globalLookAt = trackingObject.TransformDirection(localLookAt);

				var up = Vector3.up;
				Vector3.OrthoNormalize(ref globalLookAt, ref up);
				var targetRot = Quaternion.LookRotation(globalLookAt, up);

				trackingObject.rotation = Quaternion.Lerp(targetRot, trackingObject.rotation, movementSmoothing);
			}
			else
			{
				VRIME_Debugger.LogWarning("Cannot resolve look-at direction because transform is too close to the target");
			}
		}
		#endregion
        

        
	}
}