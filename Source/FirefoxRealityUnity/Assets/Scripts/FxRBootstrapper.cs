using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FxRBootstrapper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FxRFirefoxRealityVersionChecker.Instance.CheckForNewFirefoxRealityPC((newVersionAvailable, serverVersionInfo) =>
        {
            if (newVersionAvailable)
            {
                SceneManager.LoadScene("LoadingScene");
            }
            else
            {
                FxRController.LaunchFirefoxDesktop();
                FxRController.Quit(0);
            }
        });
    }
}