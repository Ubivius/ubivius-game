using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ubv.ui.client
{
    public class AddCharacterUI : MonoBehaviour
    {

        [SerializeField] private ubv.client.logic.ClientCharacterSelectState m_initState;
        [SerializeField] private TMP_InputField m_characterNameToAdd;
        [SerializeField] private TextMeshProUGUI m_errorText;
        private bool m_canCloseModal = false;
        private EventSystem system;

        private void Awake()
        {
            system = EventSystem.current;
        }

        private void Update()
        {
            if (m_canCloseModal)
            {
                m_canCloseModal = false;
                CloseAddCharacterModal();
            }
        }

        public void OpenAddCharacterModal()
        {
            m_canCloseModal = false;
            gameObject.SetActive(true);
            m_characterNameToAdd.Select();
            ResetText();
        }

        public void CloseAddCharacterModal()
        {
            gameObject.SetActive(false);
            system.SetSelectedGameObject(null);
        }

        public void AddCharacter()
        {
            string characterNameToAdd = m_characterNameToAdd.text;
            //AddCharacter
        }

        public void ResetText()
        {
            m_characterNameToAdd.text = null;
            m_errorText.text = null;
        }

        public void SetError(string error)
        {
            m_errorText.text = error;
        }

        public void SetCanCloseModal(bool canCloseModal)
        {
            m_canCloseModal = canCloseModal;
        }

    }
}
