// ========================================================================== //
//  Created: 2019-06-28
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public abstract class VRIME_FeaturesPage : MonoBehaviour
	{
		protected VRIME_KeyboardStaff parentFunc;
		abstract public void Init(VRIME_KeyboardStaff iParent);
		

		protected void PlayAnimator(Animator iAnime, string iClipName)
		{
			if(iAnime == null)
				return;
			
			iAnime.Play(iClipName);
		}
	}
}