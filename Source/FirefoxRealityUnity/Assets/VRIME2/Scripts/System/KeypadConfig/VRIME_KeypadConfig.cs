// ========================================================================== //
//  Created: 2019-07-15
// ========================================================================== //
namespace VRIME2
{
    using System;

	[Serializable]
	public class VRIME_KeypadConfig
	{
		public string name;
		public eKeyboardStage layoutType;
		public VRIME_KeypadConfigRowData[] rowDatas;
	}
	[Serializable]
	public class VRIME_KeypadConfigRowData
	{
		public eOmniKeyRows atRow;
		public VRIME_KeypadConfigData[] objDatas;

	}
	[Serializable]
	public class VRIME_KeypadConfigData
	{
		public VRIME_KeypadConfigData(int iIndex)
		{
			index = iIndex;
		}
		public int index;
		public string word;
		public eButtonType keyFuncType;
		public string[] lowerAccentWords;
		public string[] upperAccentWords;
	}

	[Serializable]
	public class VRIME_KeypadLayout
	{
		public eKeyboardStage layoutType;
		public VRIME_KeypadLayoutInfo[] rowInfos;
	}
	[Serializable]
	public class VRIME_KeypadLayoutInfo
	{
		public eOmniKeyRows atRow;
		public eKeypadLayoutType rowType;
	}

	[Serializable]
	public class VRIME_KaypadLayoutConfig
	{
		public eKeypadLayoutType rowType;
		public VRIME_KeypadLayoutRowData[] rowDatas;
	}
	[Serializable]
	public class VRIME_KeypadLayoutRowData
	{
		public eOmniKeyRows atRow;
		public VRIME_KeypadLayoutData[] objDatas;
	}
	[Serializable]
	public class VRIME_KeypadLayoutData
	{
		public VRIME_KeypadLayoutData(int iIndex)
		{
			index = iIndex;
		}
		public int index;
		public bool spacing;
		public float layoutSpaceX;
		public eOmniButtonType objectType;
	}

	public enum eKeypadLayoutType
	{
		typeA,
		typeB,
		typeC,
		typeD
	}
		// UI Have Pad Set Rows
	public enum eOmniKeyRows{
		Row_1 = 0,
		Row_2,
		Row_3,
		Row_4,
		Row_5,
		Row_6,
	}
	// Omni Button Type = Prefab Name
	public enum eOmniButtonType
	{
		None,
		Key_Large,
		Key_Medium_Character,
		Key_Medium_Modifier,
		Key_Small_character,
		Key_Small_Modifier,
		Key_Spacebar,
	}
}