// ========================================================================== //
//  Created: 2019-01-09
// ========================================================================== //
namespace VRIME2
{
	using UnityEngine;

    public class VRIME_StickCollision : VRIME_BasePhysicsEvent
    {
        #region Override Function
        protected override void ButtonEnter(Collider iController)
        {
			// VRIME_Debugger.Log(this.gameObject, "Sitck Enter. " + this.transform.position);
        }

        protected override void ButtonExit(Collider iController)
        {
			// VRIME_Debugger.Log(this.gameObject, "Sitck Exit. " + this.transform.position);
        }

        protected override void ButtonStay(Collider iController)
        {
			// VRIME_Debugger.Log(this.gameObject, "Sitck Stay. " + this.transform.position);
        }
        #endregion
    }
}