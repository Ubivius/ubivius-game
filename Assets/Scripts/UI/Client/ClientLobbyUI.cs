using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using static ubv.microservices.CharacterDataService;

namespace ubv.ui.client
{
    public class ClientLobbyUI : MonoBehaviour
    {
        [SerializeField] private Transform m_playerListParent;
        [SerializeField] private ubv.client.logic.ClientSyncLobby m_lobby;
        [SerializeField] private TextMeshProUGUI m_defaultPlayerNameItem;
        [SerializeField] private LoadingScreen m_loadingScreen;

        private Dictionary<int, TextMeshProUGUI> m_playerTextsObjects;

        // TODO : switch type from int to something more adapted
        // to UI, which would contain more client-specific info 
        // (with future Lobby UI task)
        private List<CharacterData> m_players;

        private void Awake()
        {
            m_players = new List<CharacterData>();
            m_playerTextsObjects = new Dictionary<int, TextMeshProUGUI>();
            m_lobby.OnStartLoadWorld += () =>
            {
                m_loadingScreen.gameObject.SetActive(true);
                m_loadingScreen.FadeLoadingScreen(1, 0.5f);
            };

            m_lobby.OnGameStart += () =>
            {
                m_loadingScreen.FadeAway(1);
            };
        }

        private void Start()
        {
            m_loadingScreen.gameObject.SetActive(false);
            m_lobby.ClientListUpdate.AddListener(UpdatePlayers);
        }

        private void Update()
        {
            if(Time.frameCount % 69 == 0)
            {
                foreach (CharacterData player in m_players)
                {
                    int playerIntID = player.PlayerID.GetHashCode();
                    if (!m_playerTextsObjects.ContainsKey(playerIntID))
                    {
                        TextMeshProUGUI playerItem = GameObject.Instantiate(m_defaultPlayerNameItem, m_playerListParent);
                        m_playerTextsObjects[playerIntID] = playerItem;
                    }

                    m_playerTextsObjects[playerIntID].text = player.Name + "(" + player.PlayerID + ")";
                }
            }

            if (m_loadingScreen.isActiveAndEnabled)
            {
                m_loadingScreen.LoadPercentage = m_lobby.LoadPercentage;
            }
        }

        private void AddNewPlayersFromList(List<CharacterData> newPlayers)
        {
        }

        private void UpdatePlayers(List<CharacterData> players)
        {
            m_players = players;
        }
    }
}
