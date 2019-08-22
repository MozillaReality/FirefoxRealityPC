// ========================================================================== //
//  Created: 2019-06-28
// ========================================================================== //
//  先做好事先把Keympas初始化好的機制，未來如果有更多的Keymap之後再作調整。
// ========================================================================== //
namespace VRIME2
{
	public class VRIME_KeymapInstance
	{
		#region public function
		public static void InitDefaultKeymaps()
		{
			VRIME_KeymapsEurope.Instance.GetType();
			VRIME_KeymapsPinyin.Instance.GetType();
			VRIME_KeymapsZhuyin.Instance.GetType();
		}
		#endregion
	}
}