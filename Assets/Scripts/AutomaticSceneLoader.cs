using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutomaticSceneLoader : MonoBehaviour
{
    [SerializeField] private string m_sceneToLoad;
    [SerializeField] private bool m_loadAdditive =  true;

    private void Start()
    {
        SceneManager.LoadSceneAsync(m_sceneToLoad, new LoadSceneParameters {
            loadSceneMode = m_loadAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single,
            localPhysicsMode = LocalPhysicsMode.Physics2D
        });
    }
}
