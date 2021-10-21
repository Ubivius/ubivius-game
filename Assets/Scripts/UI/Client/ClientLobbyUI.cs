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
        protected readonly object m_UILock = new object();
        protected readonly object m_userLock = new object();

        [SerializeField] private Transform m_playerListParent;
        [SerializeField] private ubv.client.logic.ClientSyncLobby m_lobby;
        [SerializeField] private TextMeshProUGUI m_defaultPlayerNameItem;
        private SocialServicesController m_socialServices;

        private Dictionary<int, TextMeshProUGUI> m_playerTextsObjects;
        
        private List<CharacterData> m_characters;
        private Dictionary<int, UserInfo> m_users;

        private void Awake()
        {
            m_characters = new List<CharacterData>();
            m_users = new Dictionary<int, UserInfo>();
            m_playerTextsObjects = new Dictionary<int, TextMeshProUGUI>();
        }

        private void Start()
        {
            m_socialServices = ubv.client.logic.ClientNetworkingManager.Instance.SocialServices;
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
                            Debug.Log("Adding character/user id to text list " + character.Name + ", " + character.PlayerID);
                            TextMeshProUGUI playerItem = GameObject.Instantiate(m_defaultPlayerNameItem, m_playerListParent);
                            m_playerTextsObjects[playerIntID] = playerItem;
                        }

                        m_playerTextsObjects[playerIntID].text = character.Name + "(" + m_users[playerIntID].UserName + ")";
                    }
                }

                List<int> toRemove = new List<int>();
                foreach(int id in m_playerTextsObjects.Keys)
                {
                    if (!m_users.ContainsKey(id))
                    {
                        Debug.Log("Removing user from text gameobjects" + id);
                        Destroy(m_playerTextsObjects[id].gameObject);
                        toRemove.Add(id);
                    }
                }

                foreach(int id in toRemove)
                {
                    m_playerTextsObjects.Remove(id);
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
                m_users[info.ID.GetHashCode()] = info;
            }
        }
    }
}
