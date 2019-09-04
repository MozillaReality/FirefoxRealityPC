// ========================================================================== //
//  Created: 2019-06-28
// ========================================================================== //
namespace VRIME2
{
    using Htc.Omni;
    using TMPro;
    using UnityEngine;

	public class VRIME_PanelCategory : MonoBehaviour
	{
		#region public Field
		public LinearLayout root{
			get { return rootTransform.GetComponent<LinearLayout>(); }
			set {
				rootTransform = value.transform;
				parent = rootTransform.parent;
			}
		}
		private Transform rootTransform;
		public Transform parent;
		public Transform[] rows;
		#endregion
		#region public function
		public void UpdateLayout()
		{
			if(rows == null)
				return;

			for(int i = 0; i < rows.Length; i++)
			{
				LinearLayout aLayout = rows[i].GetComponent<LinearLayout>();
				if(aLayout == null)
					continue;
				aLayout.UpdateLayout();
			}
		}

		public void UpdateRows()
		{
			int aPCCount = rootTransform.childCount;
			Transform[] aTmpRows = new Transform[aPCCount];
			for(int k = 0; k < aPCCount;k ++)
			{
				aTmpRows[k] = rootTransform.GetChild(k);
			}
			rows = aTmpRows;
		}
		#endregion
	}

	public class VRIME_LanguageWingButton
	{
		public VRIME_KeyboardButton button;
		public eLanguage language;
		public SupportLanguage voiceLangeage;
	}

	public class VRIME_ToolTipPage
	{
		public Transform path;
		public int index;
		public TextMeshPro title;
		public TextMeshPro body;
		public TextMeshPro buttonWord;
		public VRIME_KeyboardButton nextBtn;

		public VRIME_ToolTipPage(Transform iPath, int iIndex)
		{
			path = iPath;
			index = iIndex;
			Transform aTemp = path.Find("Text_Title");
			if(aTemp != null)
				title = aTemp.GetComponent<TextMeshPro>();
			
			aTemp = path.Find("Text_Body");
			if(aTemp != null)
				body = aTemp.GetComponent<TextMeshPro>();
			
			aTemp = path.Find("IconButton_Default");
			if(aTemp != null){
				nextBtn = aTemp.GetComponent<VRIME_KeyboardButton>();
				if(nextBtn == null) {
					nextBtn = aTemp.gameObject.AddComponent<VRIME_KeyboardButton>();
				}
				nextBtn.Init();
				nextBtn.btnType = eButtonType.ToolTipPage;
				// Get Button
				Transform aWordPath = nextBtn.rootUI.Find("ButtonCharacter");
				if(aWordPath != null) {
					buttonWord = aWordPath.GetComponent<TextMeshPro>();
				}
			}
		}
		public void SetPageWords(string iTiele, string iBody, string iBtnWord)
		{
			if(title != null)
				title.text = iTiele;
			if(body != null)
				body.text = iBody;
			if(buttonWord != null)
				buttonWord.text = iBtnWord;
		}
	}

	public struct VRIME_TipWords
	{
		public string title;
		public string body;
		public string button;

		public VRIME_TipWords(string iTitle, string iBody, string iBtn = "")
		{
			title = iTitle;
			body = iBody;
			button = iBtn;
		}
	}
}