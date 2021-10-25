using UnityEngine;
using static ubv.microservices.CharacterDataService;
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
        [SerializeField] private AddCharacterUI m_addCharacterUI;
        private int m_selectedCharacterIndex = 0;
        private bool m_joiningGame;

        public struct CharacterCount
        {
            public int selected;
            public int total;

            public CharacterCount(int selected, int total)
            {
                this.selected = selected;
                this.total = total;
            }
        }

        protected override void StateLoad()
        {
            m_cachedCharacters = null;
            m_joiningGame = false;
            CharacterService.Request(new GetCharactersFromUserRequest(UserInfo.ID, OnCharactersFetchedFromService));
        }
        
        private void OnCharactersFetchedFromService(CharacterData[] characters)
        {
            m_cachedCharacters = characters;
            SetActiveCharacter(m_cachedCharacters[m_selectedCharacterIndex].ID);
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
                return m_cachedCharacters[m_selectedCharacterIndex];
            }
            return null;
        }

        public CharacterCount GetCharacterCount()
        {
            if(m_cachedCharacters != null)
            {
                return new CharacterCount(m_selectedCharacterIndex, m_cachedCharacters.Length);
            }
            return new CharacterCount(0, 1);
        }
        
        public void GoToJoinGame()
        {
            m_joiningGame = true;
            ClientStateManager.Instance.PushScene(m_clientGameSearch);
        }

        public void AddCharacter(string characterName)
        {
            m_addCharacterUI.SetError(null);
            CharacterService.Request(new PostCharacterRequest(UserInfo.ID, characterName, OnCharacterAdd));
        }

        private void OnCharacterAdd()
        {
            CharacterService.Request(new GetCharactersFromUserRequest(UserInfo.ID, OnCharactersFetchedAfterCharacterAdd));
        }

        private void OnCharactersFetchedAfterCharacterAdd(CharacterData[] characters)
        {
            m_cachedCharacters = characters;
            m_selectedCharacterIndex = m_cachedCharacters.Length - 1;
            SetActiveCharacter(m_cachedCharacters[m_selectedCharacterIndex].ID);
            m_addCharacterUI.SetCanCloseModal(true);
        }

        public void DeleteActiveCharacter()
        {
            CharacterService.Request(new DeleteCharacterRequest(m_cachedCharacters[m_selectedCharacterIndex].ID, OnCharacterDelete));
        }

        private void OnCharacterDelete()
        {
            CharacterService.Request(new GetCharactersFromUserRequest(UserInfo.ID, OnCharactersFetchedAfterDelete));
        }

        private void OnCharactersFetchedAfterDelete(CharacterData[] characters)
        {
            m_cachedCharacters = characters;
            if(m_selectedCharacterIndex >= m_cachedCharacters.Length)  
                m_selectedCharacterIndex = m_cachedCharacters.Length - 1;
            SetActiveCharacter(m_cachedCharacters[m_selectedCharacterIndex].ID);
        }

        public string NextCharacter()
        {
            string characterName = null;
            if (m_selectedCharacterIndex < m_cachedCharacters.Length - 1)
            {
                m_selectedCharacterIndex++;
                CharacterData character = m_cachedCharacters[m_selectedCharacterIndex];
                SetActiveCharacter(character.ID);

                characterName = character.Name;
            }

            return characterName;
        }

        public string PreviousCharacter()
        {
            string characterName = null;
            if (m_selectedCharacterIndex > 0)
            {
                m_selectedCharacterIndex--;
                CharacterData character = m_cachedCharacters[m_selectedCharacterIndex];
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
