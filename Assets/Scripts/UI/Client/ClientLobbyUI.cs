using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace ubv.ui.client
{
    public class ClientLobbyUI : MonoBehaviour
    {
        [SerializeField] private Transform m_playerListParent;
        [SerializeField] private ubv.client.logic.ClientSyncLobby m_lobby;
        [SerializeField] private TextMeshProUGUI m_defaultPlayerNameItem;

        // TODO : switch type from int to something more adapted
        // to UI, which would contain more client-specific info 
        // (with future Lobby UI task)
        private List<int> m_connectedPlayers;
        private List<int> m_newPlayers;

        private void Awake()
        {
            m_connectedPlayers = new List<int>();
        }

        private void Start()
        {
            m_lobby.ClientListUpdate.AddListener(UpdatePlayers);
        }

        private void Update()
        {
            // TODO manage player lobby disconnect (UBI-283)
            if (m_newPlayers != null)
            {
                foreach (int player in m_newPlayers)
                {
                    if (!m_connectedPlayers.Contains(player))
                    {
                        TextMeshProUGUI newPlayerItem = GameObject.Instantiate(m_defaultPlayerNameItem, m_playerListParent);
                        newPlayerItem.text = player.ToString();
                        m_connectedPlayers.Add(player);
                    }
                    m_newPlayers = null;
                }
            }
        }

        private void UpdatePlayers(List<int> players)
        {
            m_newPlayers = players;
        }
    }
}
