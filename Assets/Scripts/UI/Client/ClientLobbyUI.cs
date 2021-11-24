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
        [SerializeField] private int m_refreshRateUsers = 37;
        [SerializeField] private TextMeshProUGUI m_gameIDText;
        private PlayerInLobby[] m_playersInLobby;

        private UserInfo m_activeUser;
        private CharacterData m_activeUserCharacter;
        private bool m_activeUserDisplayed = false;

        private SocialServicesController m_socialServices;

        private List<CharacterData> m_characters;
        private Dictionary<int, UserInfo> m_users;

        private void Awake()
        {
            m_characters = new List<CharacterData>();
            m_users = new Dictionary<int, UserInfo>();
            m_playersInLobby = new PlayerInLobby[] { m_mainPlayer, m_playerOne, m_playerTwo, m_playerThree };
        }

        protected override void Start()
        {
            base.Start();
            m_socialServices = ubv.client.logic.ClientNetworkingManager.Instance.SocialServices;
            m_socialServices.UpdateUserStatus(StatusType.InLobby);
            m_activeUser = m_lobby.GetActiveUser();
            m_lobby.OnClientCharacterListUpdate += UpdatePlayers;
            m_lobby.OnReadyClientSetUpdate += UpdateReadiness;

            m_gameIDText.text = ubv.client.data.LoadingData.GameID;

            m_playerOne.HidePlayer();
            m_playerTwo.HidePlayer();
            m_playerThree.HidePlayer();
        }

        protected override void Update()
        {
            base.Update();

            if (Time.frameCount % m_refreshRateUsers == 0)
            {
                RefreshUsersInLobby();
            }
        }

        private void RefreshUsersInLobby()
        {
            for (int i = 0; i < m_playersInLobby.Length; i++)
            {
                if (i < m_characters.Count)
                {
                    CharacterData character = m_characters[i];
                    int playerIntID = character.PlayerID.GetHashCode();
                    UserInfo user = m_users.ContainsKey(playerIntID) ? m_users[playerIntID] : null;
                    
                    if (user != null && character != null)
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
                    playerIntIDs.Add(playerIntID);

                    if (!m_users.ContainsKey(playerIntID))
                    {
                        m_socialServices.GetUserInfo(character.PlayerID, OnGetUserInfo);
                    }
                }
                
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
                    m_users.Remove(id);
                }
            }
        }

        private void OnGetUserInfo(UserInfo info)
        {
            lock (m_userLock)
            {
                m_users[info.ID] = info;
            }
        }

        public void Back()
        {
            //Go to game search scene
            m_lobby.BackToCharacterSelect();
        }
       
        private void UpdateReadiness(HashSet<int> readyClients)
        {
            foreach(int id in m_users.Keys)
            {
                m_users[id].Ready = readyClients.Contains(id);
            }
        }
    }
}
