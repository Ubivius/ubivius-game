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

        // TODO : switch type from playerstate to something more adapted
        // to UI, which would contain more client-specific info 
        // (with future Lobby UI task)
        private List<common.data.PlayerState> m_connectedPlayers;

        private void Awake()
        {
            m_connectedPlayers = new List<common.data.PlayerState>();
        }

        private void Start()
        {
            m_lobby.ClientListUpdate.AddListener(UpdatePlayers);
        }

        private void UpdatePlayers(List<common.data.PlayerState> players)
        {
            // TODO manage player lobby disconnect (UBI-283)
            foreach(common.data.PlayerState player in players)
            {
                if (!m_connectedPlayers.Contains(player))
                {
                    TextMeshProUGUI newPlayerItem = GameObject.Instantiate(m_defaultPlayerNameItem, m_playerListParent);
                    newPlayerItem.text = player.GUID.Value.ToString();
                    m_connectedPlayers.Add(player);
                }
            }
        }
    }
}
