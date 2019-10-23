// ========================================================================== //
//  Created: 2019-01-07
// ========================================================================== //
namespace VRIME2
{
	using UnityEngine;
	[AddComponentMenu("VRIME/IMEControllers/Drumstick")]
	public class VRIME_Drumstick : VRIME_BaseController
	{
		#region Stick Position
		public Transform HandlePostiton {
			get { return stickHandlePos; }
		}
		[SerializeField]
		private Transform stickHandlePos;
		[SerializeField]
		private VRIME_StickCollision HandleCollision;
		public Transform HeadPosition {
			get { return stickHeadPos; }
		}
		[SerializeField]
		private Transform stickHeadPos;
		[SerializeField]
		private VRIME_StickCollision HeadCollision;
        #endregion
        #region protected Function
        protected override void Init()
		{
			VRIME_Debugger.Log(name, "Init.");

			if(HandlePostiton != null){
				HandleCollision = HandlePostiton.gameObject.GetComponent<VRIME_StickCollision>();
				if(HandleCollision == null)
					HandleCollision = HandlePostiton.gameObject.AddComponent<VRIME_StickCollision>();
				HandleCollision.GetComponent<Collider>().isTrigger = true;

			}
			if(HeadPosition != null){
				HeadCollision = HeadPosition.gameObject.GetComponent<VRIME_StickCollision>();
				if(HeadCollision == null)
					HeadCollision = HeadPosition.gameObject.AddComponent<VRIME_StickCollision>();
				HeadCollision.GetComponent<Collider>().isTrigger = true;
			}
		}
        #endregion

    }
}