// ========================================================================== //
//  Created: 2019-01-08
// ========================================================================== //
namespace VRIME2
{
    using System;
    using UnityEngine;
    using UnityEngine.Events;

    abstract public class VRIME_BasePhysicsEvent : MonoBehaviour
	{
        #region Unity Evnets
        public class allTriggerCallback : UnityEvent<GameObject> { }
		public allTriggerCallback onEnter = new allTriggerCallback();
		public allTriggerCallback onExit = new allTriggerCallback();
		public allTriggerCallback onStay = new allTriggerCallback();
        #endregion

        #region Unity Function
        private void OnDestroy() {
			if(onEnter != null)
				onEnter.RemoveAllListeners();
			if(onExit != null)
				onExit.RemoveAllListeners();
			if(onStay != null)
				onStay.RemoveAllListeners();
		}
		protected void OnTriggerEnter(Collider other) {
			ButtonEnter(other);// Local Evnet
		}
		protected void OnTriggerExit(Collider other) {
			ButtonExit(other);// Local Evnet
		}
		protected void OnTriggerStay(Collider other) {
			ButtonStay(other);// Local Evnet
		}
		#endregion

		#region protected Function
		abstract protected void ButtonEnter(Collider iController);
		abstract protected void ButtonExit(Collider iController);
		abstract protected void ButtonStay(Collider iController);
		#endregion
		
	}
}