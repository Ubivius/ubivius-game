using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveSceneLoader : MonoBehaviour
{
    [SerializeField] private string m_sceneToLoad;

    private void Awake()
    {
        SceneManager.LoadSceneAsync(m_sceneToLoad, LoadSceneMode.Additive);
    }
}
