using UnityEngine;

// The silliest thing I could ever write
public class DebugLog : MonoBehaviour
{
	public void Log(string message)
	{
		Debug.Log(message);
	}
	
	public void LogWarning(string message)
	{
		Debug.LogWarning(message);
	}
	
	public void LogError(string message)
	{
		Debug.LogError(message);
	}
}
