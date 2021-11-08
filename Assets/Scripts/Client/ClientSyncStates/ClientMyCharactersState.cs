using UnityEngine;
using ubv.microservices;
using ubv.ui.client;
using TMPro;

namespace ubv.client.logic
{
    public class ClientMyCharactersState : ClientSyncState
    {
        [SerializeField] private CharacterPickerUI m_characterPickerUI;
        [SerializeField] private AddCharacterUI m_addCharacterUI;

        protected override void Awake()
        {
            base.Awake();
            m_canBack = true; 
        }

        protected override void StateLoad()
        {
        }

        public override void StateUpdate()
        {
        }

        public void AddCharacter(string characterName)
        {
            m_addCharacterUI.SetError(null);
            CharacterService.Request(new PostCharacterRequest(UserInfo.ID, characterName, OnCharacterAdd));
        }

        private void OnCharacterAdd()
        {
            m_characterPickerUI.RefreshCharacters(true);
            m_addCharacterUI.SetCanCloseModal(true);
        }

        public void DeleteActiveCharacter()
        {
            CharacterService.Request(new DeleteCharacterRequest(m_characterPickerUI.GetActiveCharacter().ID, OnCharacterDelete));
        }

        private void OnCharacterDelete()
        {
            m_characterPickerUI.RefreshCharacters();
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
