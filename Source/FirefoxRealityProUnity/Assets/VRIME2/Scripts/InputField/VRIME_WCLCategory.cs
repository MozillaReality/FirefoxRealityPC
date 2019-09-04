// ========================================================================== //
//  Created: 2019-06-26
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
    using Htc.Omni;
    using UnityEngine;

	public class VRIME_WCLCategory : MonoBehaviour
	{

		#region public Field
		public LinearLayout Layout;
		public eWCLRows atRows;
		public Transform RowHead;
		public Transform RowBody;
		public int RowLimit;
		// Objects
		public List<GameObject> Heads;
		public List<GameObject> Bodys;
		public List<GameObject> BodyTextDivs;
		#endregion
		#region public Function
		public void Init(eWCLRows iRow, Transform iBody, Transform iHead = null)
		{
			atRows = iRow;
			RowBody = iBody;
			RowHead = iHead;

			Layout = RowBody.GetComponent<LinearLayout>();
			Heads = new List<GameObject>();
			Bodys = new List<GameObject>();
			BodyTextDivs = new List<GameObject>();
		}

		public void BodyObjectsSet(bool iShow)
		{
			GameObjListSetActive(iShow, Bodys, RowLimit);
			GameObjListSetActive(iShow, BodyTextDivs, RowLimit - 1);
		}

		public void HeadObjectsSet(bool iShow)
		{
			GameObjListSetActive(iShow, Heads);
		}
		#endregion
		#region private Function
		private void GameObjListSetActive(bool iShow, List<GameObject> iList, int iLimit = 0)
		{
			int aCount = iList.Count;
			if(iLimit < aCount && iLimit > 0 && iShow)
				aCount = iLimit;
			
			for(int i = 0; i < aCount; i++)
			{
				iList[i].SetActive(iShow);
			}
		}
		#endregion
	}
}