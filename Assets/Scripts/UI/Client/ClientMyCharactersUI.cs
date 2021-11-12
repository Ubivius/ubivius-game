using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ubv.microservices.CharacterDataService;

namespace ubv.ui.client
{
    public class ClientMyCharactersUI : TabBehaviour
    {
        [SerializeField] private ubv.client.logic.ClientMyCharactersState m_myCharactersState;
        [SerializeField] private CharacterPickerUI m_characterPickerUI;
        [SerializeField] private AddCharacterUI m_addCharacterUI;
        [SerializeField] private ConfirmationModalUI m_deleteConfirmationUI;
        [SerializeField] private Button m_addButton;
        [SerializeField] private Button m_deleteButton;

        [SerializeField] private TextMeshProUGUI m_aliveText;
        [SerializeField] private TextMeshProUGUI m_deadText;
        [SerializeField] private TextMeshProUGUI m_gamedPlayedValue;
        [SerializeField] private TextMeshProUGUI m_gamedWonValue;
        [SerializeField] private TextMeshProUGUI m_enemiesKilledValue;

        [SerializeField] private string m_deleteConfirmationText;

        private EventSystem system;

        private void Awake()
        {
            system = EventSystem.current;
        }

        protected override void Update()
        {
            base.Update();

            if (Time.frameCount % 69 == 0)
            {
                if (system.currentSelectedGameObject == null)
                {
                    m_addButton.Select();
                }
                CharacterData character = m_characterPickerUI.GetActiveCharacter();
                if(character == null)
                {
                    m_deleteButton.interactable = false;
                    m_aliveText.gameObject.SetActive(false);
                    m_deadText.gameObject.SetActive(false);
                    m_gamedPlayedValue.text = "-";
                    m_gamedWonValue.text = "-";
                    m_enemiesKilledValue.text = "-";
                }
                else
                {
                    m_deleteButton.interactable = true;
                    m_aliveText.gameObject.SetActive(character.Alive);
                    m_deadText.gameObject.SetActive(!character.Alive);
                    m_gamedPlayedValue.text = character.GamesPlayed.ToString();
                    m_gamedWonValue.text = character.GamesWon.ToString();
                    m_enemiesKilledValue.text = character.EnemiesKilled.ToString();
                }
            
            }
        }

        public void AddCharacter()
        {
            m_addCharacterUI.AddCharacter();
        }

        public void OpenDeleteCharacterModal()
        {
            string characterName = m_characterPickerUI.GetActiveCharacter().Name;
            string bodyText = string.Format(m_deleteConfirmationText, characterName);
            m_deleteConfirmationUI.SetBodyText(bodyText);
            m_deleteConfirmationUI.OpenModal();
        }

        public void DeleteCharacter()
        {
            m_myCharactersState.DeleteActiveCharacter();
            m_deleteConfirmationUI.CloseModal();
        }

        public void Back()
        {
            m_myCharactersState.Back();
        }
    }
}
