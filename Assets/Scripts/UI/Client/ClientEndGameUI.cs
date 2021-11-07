using UnityEngine;
using System.Collections;
using ubv.client.logic;
using System.Collections.Generic;
using ubv.microservices;
using UnityEngine.UI;
using ubv.client;

namespace ubv.ui.client
{
    public class ClientEndGameUI : TabBehaviour
    {
        [SerializeField]
        private ClientMenuState m_menuState;

        [SerializeField] private Button m_playButton;
        [SerializeField] private Button m_statsButton;
        [SerializeField] private Button m_optionsButton;
        [SerializeField] private Button m_quitButton;

        public void Play()
        {
            m_menuState.GoToPlay();
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
}