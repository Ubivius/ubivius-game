using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ubv.client;

namespace ubv.ui.client
{
    public class ClientMenuUI : TabBehaviour
    {
        [SerializeField] private ClientMenuState m_menuState;

        public void Play()
        {
            m_menuState.GoToPlay();
        }

        public void MyCharacters()
        {
            m_menuState.GoToMyCharacters();
        }

        public void Achievements()
        {
            m_menuState.GoToMyAchievements();
        }

        public void Options()
        {
            // to be implemented
        }

        public void Quit()
        {
            m_menuState.Quit();
        }
    }
}
