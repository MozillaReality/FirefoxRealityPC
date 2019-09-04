// ========================================================================== //
//  Created: 2019-01-14
// ========================================================================== //
namespace VRIME2
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using Htc.Omni;

	public class VRIME_KeyboardAccent : MonoBehaviour
	{
		#region const Field
		private const float mOneButtonWidth = 0.05f;
		private const float mCloseButtonsTimeout = 3f;
		#endregion
		#region public Field
		public Animator motion;
		public BoxCollider mMeshPhy;
		public LinearLayout mButtonUIRoot;
		public bool IsShow { get { return mShowState; } }
		private bool mShowState = false;
		public string ShowWord { get {return mShowingWord; } }
		private string mShowingWord;
		#endregion
		#region private Field
		private VRIME_KeyboardButton mOriginal;
		private List<VRIME_KeyboardButton> mAccentButtons;
		private Transform defaultParent;
		private Transform root;
		#endregion
		#region public Function
		public void Init()
		{
			root = this.transform;
			defaultParent = root.parent;
			motion = this.GetComponentInChildren<Animator>(true);
			Transform motionChild0 = motion.transform.GetChild(0);
			motionChild0.localPosition =  new Vector3( 0f, 0.06f, -0.06f);
			mMeshPhy = this.GetComponentInChildren<BoxCollider>(true);
			if(mMeshPhy != null)
			{
				GameObject aTmpUI = new GameObject("Accent_UI");
				aTmpUI.transform.parent = mMeshPhy.transform;
				aTmpUI.transform.localPosition = new Vector3(0f, 0f, -0.0013f);
				aTmpUI.transform.localRotation =  Quaternion.Euler(Vector3.zero);
				mButtonUIRoot = aTmpUI.GetComponent<LinearLayout>();
				if(mButtonUIRoot == null)
					mButtonUIRoot = aTmpUI.AddComponent<LinearLayout>();
				mButtonUIRoot.defaultElementWidth = 0.049f;
				mButtonUIRoot.controlRotation = true;
				mButtonUIRoot.alignment = LinearLayout.Alignment.Center;
			}
			GameObject aTmpLabel = VRIME_AssetLoad.GetGameObjectResource("Prefabs/LabelButton");
			if(aTmpLabel != null)
			{
				mAccentButtons = new List<VRIME_KeyboardButton>();
				aTmpLabel.SetActive(false);
				aTmpLabel.transform.parent = VRIME_Manager.Ins.BackgroundObjPath;
				mOriginal = aTmpLabel.AddComponent<VRIME_KeyboardButton>();
				mOriginal.Init();
				mOriginal.btnType = eButtonType.LetterAccent;
			}
			CloseAccent();// Animation Status Close Accent Panel
		}
		public void CallButtons(bool iShow, VRIME_KeyboardButton iBtn)
		{
			if(mShowState && iShow == false)
			{
				CloseAccent();
				return;
			}
			if(VRIME_InputFieldOversee.Ins.PasswordMode)
				return;
			if(iShow)
			{
				OpenAccent(iBtn);
			}
			else
			{
				CloseAccent();
			}
		}
		public void CapsChangeShow(VRIME_KeyboardButton iBtn)
		{
			if(iBtn == null || string.IsNullOrEmpty(mShowingWord))
				return;
			if(iBtn.btnType != eButtonType.Caps)
				return;
			string aChangeWord = string.Empty;
			switch(VRIME_KeyboardOversee.Ins.CapsType)
			{
				case eCapsState.Lower:
					aChangeWord = mShowingWord.ToLower();
					break;
				default:
					aChangeWord = mShowingWord.ToUpper();
					break;
			}
			List<Words> aList = VRIME_Manager.runSystem.keymap.GetDisplays(aChangeWord);
			if(aList == null)
				return;
			if(aList.Count <= 0)
				return;
			// Set Button Data
			SetButtonItems(aList);
		}
		#endregion
		#region private Function
		private void SetButtonItems(List<Words> iKeyWords)
		{
			if(mAccentButtons == null)
				return;
			
			for(int i = 0; i < mAccentButtons.Count; i++) {
				mAccentButtons[i].gameObject.SetActive(false);
			}
			float aFinalWidth = 0f;
			for(int i = 0; i < iKeyWords.Count; i++)
			{
				if(i < mAccentButtons.Count)
				{
					mAccentButtons[i].Word = iKeyWords[i].Value;
					mAccentButtons[i].gameObject.SetActive(true);
				}
				else
				{
					GameObject aTmpObj = GameObject.Instantiate(mOriginal.gameObject, mButtonUIRoot.transform);
					aTmpObj.name = "AccentItem" + i;
					aTmpObj.SetActive(true);
					VRIME_KeyboardButton aNewBtn = aTmpObj.GetComponent<VRIME_KeyboardButton>();
					if(aNewBtn == null){
						aNewBtn = aTmpObj.AddComponent<VRIME_KeyboardButton>();
					}
					aNewBtn.btnType = eButtonType.LetterAccent;
					aNewBtn.Init();
					aNewBtn.Word = iKeyWords[i].Value;
					mAccentButtons.Add(aNewBtn);
				}
				aFinalWidth += mOneButtonWidth;
			}
			ResetPanelMesh(aFinalWidth);
			mButtonUIRoot.UpdateLayout();
		}
		/// <summary>
		/// Reset Omni ProceduralMeshAdapter meshs.
		/// For Meets the Accent Panel Size
		/// </summary>
		/// <param name="iWidth"></param>
		private void ResetPanelMesh(float iWidth)
		{
			ProceduralMeshAdapter aTmpAdapter = mMeshPhy.GetComponent<ProceduralMeshAdapter>();
			ProceduralPanel aFileMesh = (ProceduralPanel)aTmpAdapter.proceduralMesh;
			ProceduralPanel aSetMesh = new ProceduralPanel();
			// Basic
			aSetMesh.width = iWidth;
			aSetMesh.height = aFileMesh.height;
			aSetMesh.thickness = aFileMesh.thickness;
			aSetMesh.cornerRadius = aFileMesh.cornerRadius;
			aSetMesh.frontBevel = aFileMesh.frontBevel;
			aSetMesh.backBevel = aFileMesh.backBevel;
			// Tessellation
			aSetMesh.widthDivision = aFileMesh.widthDivision;
			aSetMesh.heightDivision = aFileMesh.heightDivision;
			aSetMesh.thicknessDivision = aFileMesh.thicknessDivision;
			aSetMesh.cornerDivision = aFileMesh.cornerDivision;
			aSetMesh.frontBevelDivision = aFileMesh.frontBevelDivision;
			aSetMesh.backBevelDividion = aFileMesh.backBevelDividion;
			// UV Mapping
			aSetMesh.frontFaceUV = aFileMesh.frontFaceUV;
			aSetMesh.sideUV = aFileMesh.sideUV;
			aSetMesh.backFaceUV = aFileMesh.backFaceUV;
			aSetMesh.mapFrontBevelUVToSide = aFileMesh.mapFrontBevelUVToSide;
			aSetMesh.mapBackBevelUVToSide = aFileMesh.mapBackBevelUVToSide;
			// Transforms
			aSetMesh.extrudeDirection = aFileMesh.extrudeDirection;
			// X/Y direction curvature in degrees per unit length
			aSetMesh.xCurvature = aFileMesh.xCurvature;
			aSetMesh.yCurvature = aFileMesh.yCurvature;
			// Generation
			aSetMesh.hasNormals = aFileMesh.hasNormals;
			aSetMesh.hasUV = aFileMesh.hasUV;
			// Update New Set
			aTmpAdapter.proceduralMesh = aSetMesh;
			Vector3 aOldSize = mMeshPhy.size;
			mMeshPhy.size = new Vector3(iWidth, aOldSize.y, aOldSize.z);
			aTmpAdapter.UpdateMesh();
		}
		/// <summary>
		/// Need Auto Close Accent List
		/// </summary>
		private void TimeOutCloseButtons() { CallButtons(false, null); }
		/// <summary>
		/// Open Accen Panel and move upper to button position.
		/// </summary>
		/// <param name="iBtn"></param>
		private void OpenAccent(VRIME_KeyboardButton iBtn)
		{
			if(iBtn == null)
				return;
			if(VRIME_Manager.runSystem.accentShow == false)
				return;
			if(mShowingWord == iBtn.Word)
			{
				// Reset auto close timeout.
				AutoCloseAccent();
				return;
			}
			List<Words> aList = VRIME_Manager.runSystem.keymap.GetDisplays(iBtn.Word);
			if(aList == null)
				return;
			if(aList.Count <= 0)
				return;
			// Set Word
			mShowingWord = iBtn.Word;
			// Set Button Data
			SetButtonItems(aList);
			// Set Open Position
			root.parent = iBtn.transform;
			root.localPosition = Vector3.zero;
			root.localRotation = Quaternion.Euler(Vector3.zero);
			root.localScale = Vector3.one;
			// Play Animation
			mShowState = true;
			PlayAnimator(eAnimAccent.Opened);
			// Reset TimeOut
			AutoCloseAccent();
		}
		/// <summary>
		/// Close Accent Panel
		/// </summary>
		private void CloseAccent()
		{
			mShowState = false;
			mShowingWord = string.Empty;
			root.parent = defaultParent;
			PlayAnimator(eAnimAccent.Closed);
		}
		/// <summary>
		/// Auto Close
		/// </summary>
		private void AutoCloseAccent()
		{
			CancelInvoke("TimeOutCloseButtons");
			Invoke("TimeOutCloseButtons", mCloseButtonsTimeout);// Start Timeout Count
		}
		private void PlayAnimator(eAnimAccent iState)
		{
			motion.Play(iState.ToString());
		}		
		private void StopAnimator(eAnimAccent iState)
		{
			motion.ResetTrigger(iState.ToString());
		}
		#endregion
	}
}