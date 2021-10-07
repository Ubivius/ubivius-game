using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using static ubv.microservices.CharacterDataService;
using UnityEngine.SceneManagement;

namespace ubv.ui.client
{
    public class ClientGameSearchUI : TabBehaviour
    {
        [SerializeField] private string m_mainMenuScene;

        [SerializeField] private ubv.client.logic.ClientSyncInit m_initState;
        [SerializeField] private TextMeshProUGUI m_characterName;

        private int selectedCharacterIndex = 0;
        private CharacterData[] m_characters = null;

        // TODO LATER:
        // offer choice to choose among living characters/make a new one ?
        private void Start()
        {
            m_characters = m_initState.GetCharacters();
        }

        private void Update()
        {
            if (Time.frameCount % 69 == 0)
            {
                if(m_characters == null)
                {
                    m_characters = m_initState.GetCharacters();
                }
               
                m_characterName.text = m_characters[selectedCharacterIndex].Name;
            }
        }

        public void nextCharacter()
        {
            if(selectedCharacterIndex < m_characters.Length-1)
            {
                selectedCharacterIndex++; 
                m_characterName.text = m_characters[selectedCharacterIndex].Name;
            }
            else
            {
                selectedCharacterIndex = 0;
            }
            setActiveCharacter();
        }
        public void previousCharacter()
        {
            if (selectedCharacterIndex > 0)
            {
                selectedCharacterIndex--;
            }
            else
            {
                selectedCharacterIndex = m_characters.Length - 1;
            }
            setActiveCharacter();
        }

        public void Back()
        {
            AsyncOperation loadMainMenu = SceneManager.LoadSceneAsync(m_mainMenuScene);
        }

        private void setActiveCharacter()
        {
            CharacterData selectedCharacter = m_characters[selectedCharacterIndex];
            m_characterName.text = selectedCharacter.Name;
            m_initState.SetActiveCharacter(selectedCharacter);
        }
    }
}
