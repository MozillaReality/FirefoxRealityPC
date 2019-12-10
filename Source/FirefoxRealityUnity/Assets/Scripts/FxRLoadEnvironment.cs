using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class FxRLoadEnvironment : MonoBehaviour
{
    private AsyncOperation LoadingOperation;

    [SerializeField] protected SteamVR_Overlay LoadingOverlay;
    [SerializeField] private List<GameObject> Hands;

    // Start is called before the first frame update
    IEnumerator Start()
    {
       yield return new WaitForSeconds(1f);
       LoadingOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
       foreach (var hand in Hands)
       {
           hand.SetActive(false);
       }
    }

    private void Update()
    {
        if (LoadingOperation != null && LoadingOperation.isDone)
        {
            LoadingOverlay.gameObject.SetActive(false);
            gameObject.SetActive(false);
            foreach (var hand in Hands)
            {
                hand.SetActive(true);
            }
        }
    }
}
