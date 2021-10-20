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
            CharacterService.Request(new GetCharactersFromUserRequest(UserInfo.ID, OnCharactersFetchedFromService));
        }
        
        private void OnCharactersFetchedFromService(CharacterData[] characters)
        {
            m_cachedCharacters = characters;
            // for now, skip selection
            SetActiveCharacter(m_cachedCharacters[0].ID);
        }

        public void SetActiveCharacter(string characterID)
        {
            data.LoadingData.ActiveCharacterID = characterID;
        }

        public override void StateUpdate()
        {
            if (data.LoadingData.ActiveCharacterID != null && !m_joiningGame)
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
            m_joiningGame = false;
        }

        protected override void StatePause()
        {
        }

        protected override void StateResume()
        {
        }
    }   
}
