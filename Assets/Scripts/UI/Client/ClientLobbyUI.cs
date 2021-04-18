using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using static ubv.microservices.CharacterDataService;
using ubv.microservices;

namespace ubv.ui.client
{
    public class ClientLobbyUI : MonoBehaviour
    {
        [SerializeField] private Transform m_playerListParent;
        [SerializeField] private ubv.client.logic.ClientSyncLobby m_lobby;
        [SerializeField] private TextMeshProUGUI m_defaultPlayerNameItem;
        [SerializeField] private LoadingScreen m_loadingScreen;
        private UserService m_userService;

        private Dictionary<int, TextMeshProUGUI> m_playerTextsObjects;
        
        private List<CharacterData> m_characters;
        private Dictionary<int, UserService.UserInfo> m_users;

        private void Awake()
        {
            m_characters = new List<CharacterData>();
            m_users = new Dictionary<int, microservices.UserService.UserInfo>();
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
            m_userService = ubv.client.logic.ClientNetworkingManager.Instance.User;
            m_loadingScreen.gameObject.SetActive(false);
            m_lobby.ClientListUpdate.AddListener(UpdatePlayers);
        }

        private void Update()
        {
            if(Time.frameCount % 69 == 0)
            {
                foreach (CharacterData character in m_characters)
                {
                    int playerIntID = character.PlayerID.GetHashCode();
                    if (m_users.ContainsKey(playerIntID))
                    {
                        if (!m_playerTextsObjects.ContainsKey(playerIntID))
                        {
                            TextMeshProUGUI playerItem = GameObject.Instantiate(m_defaultPlayerNameItem, m_playerListParent);
                            m_playerTextsObjects[playerIntID] = playerItem;
                        }

                        m_playerTextsObjects[playerIntID].text = character.Name + "(" + m_users[playerIntID].UserName + ")";
                    }
                }
            }

            if (m_loadingScreen.isActiveAndEnabled)
            {
                m_loadingScreen.LoadPercentage = m_lobby.LoadPercentage;
            }
        }
        
        private void UpdatePlayers(List<CharacterData> characters)
        {
            m_characters = characters;
            foreach(CharacterData character in m_characters)
            {
                if (!m_users.ContainsKey(character.PlayerID.GetHashCode()))
                {
                    m_userService.SendUserInfoRequest(character.PlayerID, OnGetUserInfo);
                }
            }
        }

        private void OnGetUserInfo(UserService.UserInfo info)
        {
            m_users[info.ID.GetHashCode()] = info;
        }
    }
}
