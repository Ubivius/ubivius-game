using TMPro;
using UnityEngine;

namespace ubv.ui.client
{
    public class AddCharacterUI : ModalUI
    {
        [SerializeField] private ubv.client.logic.ClientMyCharactersState m_myCharactersState;
        [SerializeField] private TMP_InputField m_characterNameToAdd;
        [SerializeField] private TextMeshProUGUI m_errorText;

        protected override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.Return) && m_characterNameToAdd.text != null)
            {
                AddCharacter();
            }
        }

        public override void OpenModal()
        {
            base.OpenModal();
            ResetText();
        }

        public void AddCharacter()
        {
            string characterNameToAdd = m_characterNameToAdd.text;
            m_myCharactersState.AddCharacter(characterNameToAdd);
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
    }
}
