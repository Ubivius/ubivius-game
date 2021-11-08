using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ubv.ui.client
{
    public class ClientCharacterSelectUI : TabBehaviour
    {
        [SerializeField] private ubv.client.logic.ClientCharacterSelectState m_initState;
        [SerializeField] private Button m_searchButton;
        [SerializeField] private Button m_createButton;
        [SerializeField] private Button m_joinButton;
        [SerializeField] private CharacterPickerUI m_characterPicker;
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
            m_initState.Back();
        }
    }
}
