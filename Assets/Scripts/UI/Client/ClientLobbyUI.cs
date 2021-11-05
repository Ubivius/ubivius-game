using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using static ubv.microservices.CharacterDataService;
using ubv.microservices;
using UnityEngine.SceneManagement;

namespace ubv.ui.client
{
    public class ClientLobbyUI : TabBehaviour
    {
        protected readonly object m_UILock = new object();
        protected readonly object m_userLock = new object();

        [SerializeField] private ubv.client.logic.ClientSyncLobby m_lobby;

        [SerializeField] private PlayerInLobby m_mainPlayer;
        [SerializeField] private PlayerInLobby m_playerOne;
        [SerializeField] private PlayerInLobby m_playerTwo;
        [SerializeField] private PlayerInLobby m_playerThree;
        private PlayerInLobby[] m_playersInLobby;

        private UserInfo m_activeUser;
        private CharacterData m_activeUserCharacter;
        private bool activeUserDisplayed = false;

        private SocialServicesController m_socialServices;

        private List<CharacterData> m_characters;
        private Dictionary<int, UserInfo> m_users;

        private void Awake()
        {
            m_characters = new List<CharacterData>();
            m_users = new Dictionary<int, UserInfo>();
            m_playersInLobby = new PlayerInLobby[] { m_playerOne, m_playerTwo, m_playerThree };
        }

        protected override void Start()
        {
            base.Start();
            m_socialServices = ubv.client.logic.ClientNetworkingManager.Instance.SocialServices;
            m_lobby.ClientListUpdate.AddListener(UpdatePlayers);
            m_activeUser = m_lobby.GetActiveUser();

            m_playerOne.HidePlayer();
            m_playerTwo.HidePlayer();
            m_playerThree.HidePlayer();
        }

        protected override void Update()
        {
            base.Update();

            if (Time.frameCount % 69 == 0)
            {
                RefreshUsersInLobby();
            }
        }

        private void RefreshUsersInLobby()
        {
            if (!activeUserDisplayed)
            {
                m_mainPlayer.ShowPlayer(m_activeUser, m_activeUserCharacter);
                activeUserDisplayed = true;
            }

            for (int i = 0; i < m_playersInLobby.Length; i++)
            {
                if (i < m_characters.Count)
                {
                    CharacterData character = m_characters[i];
                    int playerIntID = character.PlayerID.GetHashCode();
                    UserInfo user = m_users[playerIntID];
                    if (user != null)
                    {
                        m_playersInLobby[i].ShowPlayer(user, character);
                    }
                    else if(m_playersInLobby[i].IsVisible())
                    {
                        m_playersInLobby[i].HidePlayer();
                    }
                    
                }
                else if (m_playersInLobby[i].IsVisible())
                {
                    m_playersInLobby[i].HidePlayer();
                }
            }
        }
        
        private void UpdatePlayers(List<CharacterData> characters)
        {
            lock (m_UILock)
            {
                m_characters = characters;
                List<int> playerIntIDs = new List<int>();
                foreach (CharacterData character in m_characters)
                {
                    int playerIntID = character.PlayerID.GetHashCode();
                    if (playerIntID == m_activeUser.StringID.GetHashCode())
                    {
                        m_activeUserCharacter = character;
                    }
                    else
                    {
                        playerIntIDs.Add(playerIntID);
                    }

                    if (!m_users.ContainsKey(playerIntID))
                    {
                        
                        m_socialServices.GetUserInfo(character.PlayerID, OnGetUserInfo);
                    }
                }

                m_characters.Remove(m_activeUserCharacter);

                List<int> toRemove = new List<int>();
                foreach (int id in m_users.Keys)
                {
                    if (!playerIntIDs.Contains(id))
                    {
                        toRemove.Add(id);
                    }
                }

                foreach (int id in toRemove)
                {
                    Debug.Log("Removing user id " + id);
                    m_users.Remove(id);
                }
            }
        }

        private void OnGetUserInfo(UserInfo info)
        {
            lock (m_userLock)
            {
                Debug.Log("Got user info in UI from " + info.UserName);
                m_users[info.StringID.GetHashCode()] = info;
            }
        }

        public void Back()
        {
            //Go to game search scene
        }

        public void ToggleReady()
        {
            m_activeUser.Ready = !m_activeUser.Ready;
            string statusText = "NOT READY";
            if (m_activeUser.Ready)
            {
                statusText = "READY";
            }
      
            m_mainPlayer.SetStatus(statusText);
        } 
    }
}
