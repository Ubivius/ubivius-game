using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using static ubv.microservices.CharacterDataService;
using UnityEngine.SceneManagement;

namespace ubv.ui.client
{
    public class ClientCharacterSelectUI : TabBehaviour
    {
        [SerializeField] private ubv.client.logic.ClientCharacterSelectState m_initState;
        [SerializeField] private TextMeshProUGUI m_characterName;

        private void Update()
        {
            base.Update();

            if (Time.frameCount % 69 == 0)
            {
                m_characterName.text = m_initState.GetActiveCharacter()?.Name;
            }
        }

        public void NextCharacter()
        {
            string characterName = m_initState.NextCharacter();
            if (characterName != null)
            {
                m_characterName.text = characterName;
            }  
        }

        public void PreviousCharacter()
        {
            string characterName = m_initState.PreviousCharacter();
            if (characterName != null)
            {
                m_characterName.text = characterName;
            }
        }

        public void Back()
        {
            // Return to main menu
        }
    }
}
