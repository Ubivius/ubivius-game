using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientMenuUI : MonoBehaviour
{
    [SerializeField] private string m_gameSearchScene;

    public void Play()
    {
        AsyncOperation loadLobby = SceneManager.LoadSceneAsync(m_gameSearchScene);
    }

    public void Stats()
    {
        // to be implemented
    }

    public void Options()
    {
        // to be implemented
    }

    public void Quit()
    {
        Application.Quit();
    }
}
