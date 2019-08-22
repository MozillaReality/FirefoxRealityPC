// ========================================================================== //
//  Created: 2019-07-18
// ========================================================================== //
namespace VRIME2
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
    using Htc.Omni;
    using UnityEngine;

	public class VRIME_PanelCatchLayout : MonoBehaviour
	{
		#region private Field
		private const int cLayoutMaxRow = 5;
		private Dictionary<eKeypadLayoutType, List<CatchRowLayouts>> catchObjects = new Dictionary<eKeypadLayoutType, List<CatchRowLayouts>>();
		private Dictionary<eOmniKeyRows, Transform> standardRows = new Dictionary<eOmniKeyRows, Transform>();
		private VRIME_PanelCategory panelmeshStandard;
		#endregion
		#region public Function
		public void MakeCatchObjects(VRIME_PanelCategory iCategory)
		{
			if(iCategory == null)
				return;// category is null not working.
			if(iCategory.parent.name.ToLower() != ePanelMeshNames.StandardKeys.ToString().ToLower())
				return;// only support standardkeys category
			panelmeshStandard = iCategory;
			
			if(catchObjects.Count > 0)// Already have data no need to repeat.
				return;
			// Move standard row
			CatchStandardBaseRoot();
			// Make catch objects
			// Before refresh, destory objects still in scenes.
			// transform.childCount still get those objects.
			CancelInvoke("MakeAllCatchObjects");
			Invoke("MakeAllCatchObjects", 0f);
		}
		/// <summary>
		/// Update standard keyboard category layout.
		/// </summary>
		/// <param name="iLayout"></param>
		public void SetCategoryLayout(VRIME_KeypadLayout iLayout)
		{
			ResetCatchRoot();
			for(int i = 0; i < iLayout.rowInfos.Length; i++)
			{
				VRIME_KeypadLayoutInfo aSetInfo = iLayout.rowInfos[i];
				Transform aMoveRow = GetCatchRowRoot(aSetInfo.rowType, aSetInfo.atRow);
				if(aMoveRow == null)
					continue;
				// Set Parent, sibling index, and set local scale is one.
				aMoveRow.parent = panelmeshStandard.transform;
				aMoveRow.SetSiblingIndex((int)aSetInfo.atRow);
				aMoveRow.localScale = Vector3.one;
			}
			// Update layout is reset position and rotation.
			// Update rows is relink category rows in new layout.
			panelmeshStandard.root.UpdateLayout();
			panelmeshStandard.UpdateRows();
			panelmeshStandard.UpdateLayout();
		}
		#endregion
		#region private Function
		/// <summary>
		/// Standard keyboard rows reset in catch root.
		/// </summary>
		private void ResetCatchRoot()
		{
			foreach (var item in catchObjects)
			{
				for(int i = 0; i < item.Value.Count; i++)
				{
					CatchRowLayouts aLayout = item.Value[i];
					aLayout.rowRoot.parent = aLayout.typeRoot;
				}
			}
		}
		/// <summary>
		/// Get row root by type.
		/// </summary>
		/// <param name="iType"></param>
		/// <param name="iatRow"></param>
		/// <returns></returns>
		private Transform GetCatchRowRoot(eKeypadLayoutType iType, eOmniKeyRows iatRow)
		{
			Transform aResult = null;
			List<CatchRowLayouts> aLayouts = null;
			bool iSuccess = catchObjects.TryGetValue(iType, out aLayouts);
			if(iSuccess)
			{
				foreach (var item in aLayouts)
				{
					if(item.layoutRow == iatRow)
					{
						aResult = item.rowRoot;
						return aResult;
					}
				}
				return aResult;
			}
			else
				return aResult;
		}
		/// <summary>
		/// Get all layout button data.
		/// </summary>
		/// <returns></returns>
		public static Dictionary<eKeypadLayoutType, VRIME_KeypadLayoutRowData[]> LoadKeypadLayoutTypeData()
		{
			Dictionary<eKeypadLayoutType, VRIME_KeypadLayoutRowData[]> aResult = new Dictionary<eKeypadLayoutType, VRIME_KeypadLayoutRowData[]>();
			string[] aLayoutFileNames = VRIME_AssetLoad.LoadKeypadFileNameList(eConfigFolder.KeypadLayoutData);
			for(int i = 0; i < aLayoutFileNames.Length; i++)
			{
				string aJsonString = VRIME_AssetLoad.LoadKeypadConfig(aLayoutFileNames[i], eConfigFolder.KeypadLayoutData);
				VRIME_KaypadLayoutConfig aLayoutConfig = JsonUtility.FromJson<VRIME_KaypadLayoutConfig>(aJsonString);
				aResult.Add(aLayoutConfig.rowType, aLayoutConfig.rowDatas);
			}

			return aResult;
		}
		#endregion
		#region Make CatchObjects
		/// <summary>
		/// Move standard board.
		/// </summary>
		private void CatchStandardBaseRoot()
		{
			// Make copy Standard head 
			GameObject aStandardKeys = new GameObject(ePanelMeshNames.StandardKeys.ToString());
			aStandardKeys.transform.parent = this.transform;
			aStandardKeys.transform.localPosition = Vector3.zero;
			aStandardKeys.transform.localRotation = Quaternion.Euler(Vector3.zero);
			// Move category row to catch root
			for(int i = 0; i < cLayoutMaxRow; i++)
			{
				Transform aGetRow = panelmeshStandard.rows[i];
				panelmeshStandard.rows[i].parent = aStandardKeys.transform;
				standardRows.Add((eOmniKeyRows)i, aGetRow);
				// Clean scenes objects
				for(int k = 0; k < aGetRow.childCount; k++) {
					Destroy(aGetRow.GetChild(k).gameObject);
				}
			}
			panelmeshStandard.UpdateRows();
		}
		/// <summary>
		/// Search all type and set objects
		/// </summary>
		private void MakeAllCatchObjects()
		{
			// Make All row objects
			string[] aLayoutTypes =  Enum.GetNames(typeof(eKeypadLayoutType));
			for(int i = 0; i < aLayoutTypes.Length; i++)
			{
				VRIME_KeypadLayoutRowData[] aRowDatas = null;
				Dictionary<eKeypadLayoutType, VRIME_KeypadLayoutRowData[]> catchAllRowData = LoadKeypadLayoutTypeData();
				bool aSuccess = catchAllRowData.TryGetValue((eKeypadLayoutType)i, out aRowDatas);
				if(aSuccess == false)
					continue;
				// Row data instantiate to gameobject, by type.
				MakeTypeRootObject((eKeypadLayoutType)i, aRowDatas);
			}
		}
		/// <summary>
		/// Stairs 1. Type Root.
		/// </summary>
		private void MakeTypeRootObject(eKeypadLayoutType iType, VRIME_KeypadLayoutRowData[] iRowDatas)
		{
			// Make type head
			GameObject aTypeHead = new GameObject();
			aTypeHead.name = iType.ToString();
			aTypeHead.transform.parent = this.transform;
			aTypeHead.transform.localPosition = Vector3.zero;
			aTypeHead.transform.localRotation = Quaternion.Euler(Vector3.zero);

			List<CatchRowLayouts> aCatchRow = new List<CatchRowLayouts>();
			for(int i = 0; i < iRowDatas.Length; i++)
			{
				VRIME_KeypadLayoutRowData aData = iRowDatas[i];
				Transform aStandRowHead = null;
				bool aSuccess = standardRows.TryGetValue(aData.atRow, out aStandRowHead);
				if(aSuccess == false)
					continue;
				
				GameObject aRowHead = MakeRowItemObjects(aStandRowHead, aTypeHead.transform, aData);
				// Add Info
				CatchRowLayouts aCatchItem = new CatchRowLayouts();
				aCatchItem.typeRoot = aTypeHead.transform;
				aCatchItem.rowRoot = aRowHead.transform;
				aCatchItem.layoutRow = aData.atRow;
				
				aCatchRow.Add(aCatchItem);
			}
			// Save catch row head transform
			catchObjects.Add(iType, aCatchRow);
		}
		/// <summary>
		/// Stairs 2. Item Objects.
		/// </summary>
		private GameObject MakeRowItemObjects(Transform iStandHead, Transform iTypeHead, VRIME_KeypadLayoutRowData iData)
		{
			// Make Head
			GameObject aRowHead = GameObject.Instantiate(iStandHead.gameObject);
			aRowHead.name = iStandHead.name;
			aRowHead.transform.parent = iTypeHead;
			aRowHead.transform.localPosition = Vector3.zero;
			aRowHead.transform.localRotation = Quaternion.Euler(Vector3.zero);
			// Make Item
			for(int i = 0; i < iData.objDatas.Length; i++)
			{
				VRIME_KeypadLayoutData aLayoutItem = iData.objDatas[i];
				GameObject aTempObj = InstantiateNewObject(aLayoutItem, aRowHead.transform);
			}
			// Back Row Head
			return aRowHead;
		}
		/// <summary>
		/// Instantiate keyboard buttons.
		/// </summary>
		/// <param name="iData"></param>
		/// <param name="iRowHead"></param>
		/// <returns></returns>
		private GameObject InstantiateNewObject(VRIME_KeypadLayoutData iData, Transform iRowHead)
		{
			GameObject aResult = null;
			if(iData.spacing)
			{
				aResult = new GameObject();
				aResult.name = "Spacing";
				LinearLayoutElement aLinEle = aResult.AddComponent<LinearLayoutElement>();
				aLinEle.size = new Vector3(iData.layoutSpaceX, 1f, 1f);
			}
			else
			{
				aResult = VRIME_KeyboardOversee.Ins.GetKeybuttonByType(iData.objectType);
				if(aResult == null)
					return aResult;
				
				aResult.SetActive(true);
				VRIME_KeyboardButton aButton = aResult.GetComponent<VRIME_KeyboardButton>();
				aButton.Init();
				if(iData.layoutSpaceX > 0)
				{
					LinearLayoutElement aLinEle = aResult.GetComponent<LinearLayoutElement>();
					aLinEle.size = new Vector3(iData.layoutSpaceX, 1f, 1f);
				}
			}
			aResult.transform.parent = iRowHead;
			aResult.transform.localScale = Vector3.one;
			return aResult;
		}
		#endregion
		private class CatchRowLayouts
		{
			public eOmniKeyRows layoutRow;
			public Transform rowRoot;
			public Transform typeRoot;
		}
	}
}