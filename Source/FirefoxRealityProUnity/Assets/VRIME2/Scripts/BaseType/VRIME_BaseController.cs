// ========================================================================== //
//  Created: 2019-01-08
// ========================================================================== //
namespace VRIME2
{
	using UnityEngine;

	abstract public class VRIME_BaseController : MonoBehaviour
	{
		#region unity Function
		private void Awake() {
			Init();
		}
		#endregion
		#region protected Function
		abstract protected void Init();
		#endregion
	}
}