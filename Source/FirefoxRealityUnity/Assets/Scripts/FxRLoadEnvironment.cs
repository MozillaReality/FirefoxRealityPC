using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

public class FxRLoadEnvironment : MonoBehaviour
{
    private AsyncOperation LoadingOperation;

    [SerializeField] protected SteamVR_Overlay LoadingOverlay;
    // Start is called before the first frame update
    IEnumerator Start()
    {
       yield return new WaitForSeconds(1f);
       LoadingOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
    }

    private void Update()
    {
        if (LoadingOperation != null && LoadingOperation.isDone)
        {
            LoadingOverlay.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
