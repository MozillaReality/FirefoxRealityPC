// ========================================================================== //
//  Created: 2019-06-27
// ========================================================================== //
namespace VRIME2
{
	public class VRIME_LanguageFactory
	{
		public static VRIME_LanguageSys MakeUsingSystem(eLanguage iLanguage)
		{
			switch(iLanguage)
			{
				case eLanguage.Zhuyin: return new VRIME_ZhuyinSystem();
				case eLanguage.PinYin: return new VRIME_PinYinSystem();
				default:
					return new VRIME_EuropSystem();
			}

		}
	}
}