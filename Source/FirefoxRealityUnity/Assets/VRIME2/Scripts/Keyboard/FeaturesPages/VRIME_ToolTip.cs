// ========================================================================== //
//  Created: 2019-07-01
// ========================================================================== //
namespace VRIME2
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

    public class VRIME_ToolTip : VRIME_FeaturesPage
    {
		public eToolTipPage toolTipNowPage;
		[SerializeField]
		private Animator toolTipAnime;
		private VRIME_ToolTipPage[] toolTipPages;
		#region override Function
        public override void Init(VRIME_KeyboardStaff iParent)
        {
			parentFunc = iParent;
			this.gameObject.SetActive(true);
			toolTipAnime = this.GetComponentInChildren<Animator>();
			if(toolTipAnime != null) {
				SetTipPages();
			}
			bool iShowTip = false;
			if(VRIME_KeyboardSetting.TooltipHaveSeen == false)
				iShowTip = true;

			this.gameObject.SetActive(iShowTip);
        }
		#endregion
		#region public Function
		public void callTooltip(bool iShow)
		{
			eAnimAccent aType = eAnimAccent.Normal;
			if(iShow)
				aType = eAnimAccent.Opened;
			else
				aType = eAnimAccent.Closed;
			
			PlayAnimeTooltip(aType);
			// After Show
			if(iShow)
			{
				// Fix "Animator is not playing an AnimatorController"
				for(int i = 0; i < toolTipPages.Length; i++) {
					if(toolTipPages[i].path.gameObject.activeSelf == false)
						return;						
					toolTipPages[i].nextBtn.motion.Play(eAnimBtn.Highlighted.ToString());
				}
			}
			else
			{
				Invoke("TooltipClose", 0.3f);
			}
		}
		public void SetTooltipPage(eToolTipPage iPage)
		{
			if(toolTipPages == null)
				return;
			// Close Page
			for(int i = 0; i < toolTipPages.Length; i++) {
				toolTipPages[i].path.gameObject.SetActive(false);
			}
			int aPageIndex = (int)iPage;
			// Show Page
			if(aPageIndex < toolTipPages.Length == false)
				return;
			toolTipNowPage = iPage;
			toolTipPages[aPageIndex].path.gameObject.SetActive(true);
			toolTipPages[aPageIndex].nextBtn.motion.Play(eAnimBtn.Highlighted.ToString());
		}
		public void SetTooltipPagesWords(eToolTipPage iPage, VRIME_TipWords iWords)
		{
			if(toolTipPages == null)
				return;
			
			int aPageIndex = (int)iPage;
			VRIME_ToolTipPage aItem = toolTipPages[aPageIndex];
			aItem.SetPageWords(iWords.title, iWords.body, iWords.button);
		}
		#endregion
		#region private Function
		private void SetTipPages()
		{
			string[] aPages =  Enum.GetNames(typeof(eToolTipPage));
			toolTipPages = new VRIME_ToolTipPage[aPages.Length];
			for(int i = 0; i < toolTipPages.Length; i++)
			{
				Transform aStates = toolTipAnime.transform.Find(aPages[i]);
				if(aStates == null)
					continue;
				VRIME_ToolTipPage aItem = new VRIME_ToolTipPage(aStates, i);				
				bool aPageShow = false;
				if(i == 0) {
					aPageShow = true;
					toolTipNowPage = (eToolTipPage)i;
				}
				aItem.path.gameObject.SetActive(aPageShow);
				toolTipPages[i] = aItem;
			}
		}
		private void TooltipClose()
		{
			CancelInvoke("TooltipClose");
			this.gameObject.SetActive(false);
			VRIME_KeyboardSetting.TooltipHaveSeen = true;
		}
		private void PlayAnimeTooltip(eAnimAccent iType) { PlayAnimator(toolTipAnime, iType.ToString()); }
		#endregion
    }
}