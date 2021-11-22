using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ubv.client;
using ubv.client.logic;

namespace ubv.ui.client
{
    public class ClientPauseMenuUI : TabBehaviour
    {
        [SerializeField] private ClientSyncPlay m_gameState;

        public void LeaveGame()
        {
            m_gameState.LeaveGame();
        }
    }
}
