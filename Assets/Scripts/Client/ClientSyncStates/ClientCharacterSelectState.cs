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
using TMPro;

namespace ubv.client.logic
{
    /// <summary>
    /// Represents the client state after he's logged in, and before he finds a lobby
    /// </summary>
    public class ClientCharacterSelectState : ClientSyncState
    {
        [SerializeField] private string m_clientGameSearch;
        private CharacterData[] m_cachedCharacters;
        private int selectedCharacterIndex = 0;
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
            SetActiveCharacter(m_cachedCharacters[selectedCharacterIndex].ID);
        }

        public void SetActiveCharacter(string characterID)
        {
            data.LoadingData.ActiveCharacterID = characterID;
        }

        public override void StateUpdate()
        {
        }

        public CharacterData GetActiveCharacter()
        {
            if (m_cachedCharacters != null)
            {
                return m_cachedCharacters[selectedCharacterIndex];
            }
            return null;
        }
        
        public void GoToJoinGame()
        {
            m_joiningGame = true;
            ClientStateManager.Instance.PushScene(m_clientGameSearch);
        }

        public string NextCharacter()
        {
            string characterName = null;
            if (selectedCharacterIndex < m_cachedCharacters.Length - 1)
            {
                selectedCharacterIndex++;
                CharacterData character = m_cachedCharacters[selectedCharacterIndex];
                SetActiveCharacter(character.ID);

                characterName = character.Name;
            }

            return characterName;
        }

        public string PreviousCharacter()
        {
            string characterName = null;
            if (selectedCharacterIndex > 0)
            {
                selectedCharacterIndex--;
                CharacterData character = m_cachedCharacters[selectedCharacterIndex];
                SetActiveCharacter(character.ID);

                characterName = character.Name;
            }

            return characterName;
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
