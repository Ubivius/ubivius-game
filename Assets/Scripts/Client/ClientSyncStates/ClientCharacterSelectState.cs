using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;
using System.Net.Http;
using static ubv.microservices.DispatcherMicroservice;
using static ubv.microservices.CharacterDataService;
using UnityEngine.Events;
using ubv.utils;
using ubv.microservices;

namespace ubv.client.logic
{
    /// <summary>
    /// Represents the client state after he's logged in, and before he finds a lobby
    /// </summary>
    public class ClientCharacterSelectState : ClientSyncState
    {
        [SerializeField] private string m_clientGameSearch;
        private CharacterData[] m_cachedCharacters;
        private bool m_joiningGame;
        
        protected override void StateLoad()
        {
            m_cachedCharacters = null;
            m_joiningGame = false;
            if (LoadingData.ActiveCharacter == null)
            {
                m_characterService.Request(new GetCharactersFromUserRequest(UserInfo.ID, OnCharactersFetchedFromService));
            }
        }
        
        private void OnCharactersFetchedFromService(CharacterData[] characters)
        {
            m_cachedCharacters = characters;
            // for now, skip selection
            SetActiveCharacter(m_cachedCharacters[0]);
        }

        public void SetActiveCharacter(CharacterData character)
        {
            LoadingData.ActiveCharacter = character;
        }

        public override void StateUpdate()
        {
            if (LoadingData.ActiveCharacter != null && !m_joiningGame)
            {
                GoToJoinGame();
            }
        }

        public CharacterData GetActiveCharacter()
        {
            if (m_cachedCharacters != null)
            {
                return m_cachedCharacters[0];
            }
            return null;
        }
        
        private void GoToJoinGame()
        {
            m_joiningGame = true;
            ClientStateManager.Instance.PushState(m_clientGameSearch);
        }
        
        protected override void StateUnload()
        {
        }

        protected override void StatePause()
        {
        }

        protected override void StateResume()
        {
        }
    }   
}
