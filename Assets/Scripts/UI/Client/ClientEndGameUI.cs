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
        private ClientEndGameState m_EndGameState;

        [SerializeField] private Button m_menuButton;

        public void Menu()
        {
            m_EndGameState.GoToMenu();
        }
    }
}
