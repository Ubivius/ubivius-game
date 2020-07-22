using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveSceneLoader : MonoBehaviour
{
    [SerializeField] private string m_sceneToLoad;

    private void Start()
    {
        SceneManager.LoadSceneAsync(m_sceneToLoad, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics2D });
    }
}
