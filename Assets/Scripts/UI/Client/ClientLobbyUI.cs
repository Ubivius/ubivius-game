﻿using UnityEngine;
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

        [SerializeField] private string m_gameSearchScene;

        [SerializeField] private PlayerInLobby m_mainPlayer;
        [SerializeField] private PlayerInLobby m_playerOne;
        [SerializeField] private PlayerInLobby m_playerTwo;
        [SerializeField] private PlayerInLobby m_playerThree;

        private UserService.UserInfo m_activeUser;
        private bool activeUserDisplayed = false;

        [SerializeField] private LoadingScreen m_loadingScreen;
        private UserService m_userService;

        private Dictionary<int, PlayerInLobby> m_playerTextsObjects;
        
        private List<CharacterData> m_characters;
        private Dictionary<int, UserService.UserInfo> m_users;

        private void Awake()
        {
            m_characters = new List<CharacterData>();
            m_users = new Dictionary<int, microservices.UserService.UserInfo>();
            m_playerTextsObjects = new Dictionary<int, PlayerInLobby>();
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
            m_activeUser = m_lobby.GetActiveUser();

            m_playerOne.RemovePlayer();
            m_playerTwo.RemovePlayer();
            m_playerThree.RemovePlayer();
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
                        if(playerIntID == m_activeUser.ID.GetHashCode() && !activeUserDisplayed)
                        {
                            Debug.Log("Adding active character/user id to lobby " + character.Name + ", " + character.PlayerID);
                            m_mainPlayer.AddPlayer(m_users[playerIntID], character);
                            activeUserDisplayed = true;
                        }
                        else if (!m_playerTextsObjects.ContainsKey(playerIntID) && playerIntID != m_activeUser.ID.GetHashCode())
                        {
                            Debug.Log("Adding character/user id to lobby " + character.Name + ", " + character.PlayerID);
                            int playerCountInLobby = m_playerTextsObjects.Count;

                            switch (playerCountInLobby)
                            {
                                case 0:
                                    m_playerTextsObjects[playerIntID] = m_playerOne;
                                    break;
                                case 1:
                                    m_playerTextsObjects[playerIntID] = m_playerTwo;
                                    break;
                                default:
                                    m_playerTextsObjects[playerIntID] = m_playerThree;
                                    break;
                            }
                            m_playerTextsObjects[playerIntID].AddPlayer(m_users[playerIntID], character);
                        }
                    }
                }

                List<int> toRemove = new List<int>();
                foreach(int id in m_playerTextsObjects.Keys)
                {
                    if (!m_users.ContainsKey(id))
                    {
                        Debug.Log("Removing user from text gameobjects" + id);
                        toRemove.Add(id);
                    }
                }

                foreach(int id in toRemove)
                {
                    m_playerTextsObjects[id].RemovePlayer();
                    m_playerTextsObjects.Remove(id);
                }
            }

            if (m_loadingScreen.isActiveAndEnabled)
            {
                m_loadingScreen.LoadPercentage = m_lobby.LoadPercentage;
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
                        m_userService.SendUserInfoRequest(character.PlayerID, OnGetUserInfo);
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

        private void OnGetUserInfo(UserService.UserInfo info)
        {
            lock (m_userLock)
            {
                Debug.Log("Got user info in UI from " + info.UserName);
                m_users[info.ID.GetHashCode()] = info;
            }
        }

        public void Back()
        {
            AsyncOperation loadGameSearch = SceneManager.LoadSceneAsync(m_gameSearchScene);
        }

        public void ToggleReady()
        {
            string statusText = "NOT READY";
            if (!m_activeUser.Ready)
            {
                statusText = "READY";
            }
            m_activeUser.Ready = !m_activeUser.Ready;
            m_mainPlayer.SetStatus(statusText);
        } 
    }
}
