using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace ubv.ui.client
{
    public class ClientCharacterSelectUI : TabBehaviour
    {
        [SerializeField] private ubv.client.logic.ClientCharacterSelectState m_characterSelectState;
        [SerializeField] private Button m_searchButton;
        [SerializeField] private Button m_createButton;
        [SerializeField] private Button m_joinButton;
        [SerializeField] private CharacterPickerUI m_characterPicker;
        [SerializeField] private TMP_InputField m_serverID;
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
                if (!m_characterPicker.AsCharacters())
                {
                    m_searchButton.interactable = false;
                    m_createButton.interactable = false;
                    m_joinButton.interactable = false;
                }
                else
                {
                    m_searchButton.interactable = true;
                    m_createButton.interactable = true;
                    m_joinButton.interactable = true;
                }

                if (system.currentSelectedGameObject == null)
                {
                    m_searchButton.Select();
                }
            }
        }

        public void Back()
        {
            m_characterSelectState.GoBackToPreviousState();
        }

        public void GoToLobby()
        {
            this.m_characterSelectState.GoToLobby(m_serverID.text);
        }
    }
}
