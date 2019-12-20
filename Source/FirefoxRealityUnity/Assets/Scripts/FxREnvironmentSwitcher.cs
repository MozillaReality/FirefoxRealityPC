using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FxREnvironmentSwitcher : MonoBehaviour
{
    [SerializeField] private Transform EnvironmentParent;
    [SerializeField] private List<FxREnvironment> EnvironmentPrefabs;

    private FxREnvironment CurrentEnvironment;

    // Start is called before the first frame update
    void Start()
    {
        SwitchEnvironment(0);
    }

    // Update is called once per frame
    void SwitchEnvironment(int environmentIndex)
    {
        if (CurrentEnvironment != null)
        {
            Destroy(CurrentEnvironment);
        }
        CurrentEnvironment = Instantiate(EnvironmentPrefabs[environmentIndex]
            , EnvironmentParent.transform.position
            , EnvironmentParent.transform.rotation
            , EnvironmentParent);
    }
}