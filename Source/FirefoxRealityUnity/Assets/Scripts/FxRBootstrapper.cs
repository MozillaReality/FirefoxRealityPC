using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class FxRBootstrapper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FxRFirefoxRealityVersionChecker.Instance.CheckForNewFirefoxRealityPC((newVersionAvailable, serverVersionInfo) =>
        {
            if (newVersionAvailable)
            {
                StartCoroutine(LoadLoadingScene());
            }
            else
            {
                FxRController.LaunchFirefoxDesktop();
                FxRController.Quit(0);
            }
        });
    }

    IEnumerator LoadLoadingScene()
    {
        yield return new WaitForEndOfFrame();
        XRSettings.LoadDeviceByName("OpenVR");
        yield return new WaitForEndOfFrame();
        XRSettings.enabled = true;
        yield return new WaitForEndOfFrame();
        SceneManager.LoadSceneAsync("LoadingScene");
    }
}