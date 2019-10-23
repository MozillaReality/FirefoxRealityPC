// ========================================================================== //
//  Created: 2019-06-27
// ========================================================================== //
namespace VRIME2
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class VRIME_EuropSystem : VRIME_LanguageSys
	{
		public VRIME_EuropSystem()
		{
			keymap.ChangeAccentKeymapDict(this);
		}
		#region override Field
        public override bool accentShow { get { return true; } }
        public override bool wordCandidateShow { get { return false; } }
        public override VRIME_Keymaps keymap { get { return VRIME_KeymapsEurope.Instance; } }
		#endregion
		
	}
}