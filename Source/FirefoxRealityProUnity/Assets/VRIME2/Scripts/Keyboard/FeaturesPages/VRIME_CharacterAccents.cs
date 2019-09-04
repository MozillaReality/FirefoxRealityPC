// ========================================================================== //
//  Created: 2019-07-01
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

    public class VRIME_CharacterAccents : VRIME_FeaturesPage
    {
		public VRIME_KeyboardAccent Accent{ get{ return mAccentObj; } }
		private VRIME_KeyboardAccent mAccentObj;
		#region override Function
        public override void Init(VRIME_KeyboardStaff iParent)
        {
			parentFunc = iParent;
            GameObject aTmpPanel = VRIME_AssetLoad.GetGameObjectResource("Prefabs/AccentPanel");
			if(aTmpPanel != null)
			{
				aTmpPanel.transform.parent = this.transform;
				aTmpPanel.transform.localPosition = new Vector3(0f, 0f, -0.03f);
				aTmpPanel.transform.localRotation = Quaternion.Euler(Vector3.zero);

				mAccentObj = aTmpPanel.GetComponent<VRIME_KeyboardAccent>();
				if(mAccentObj == null)
					mAccentObj = aTmpPanel.AddComponent<VRIME_KeyboardAccent>();
				mAccentObj.Init();
			}
        }
		#endregion
    }
}