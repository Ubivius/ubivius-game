using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using static ubv.microservices.CharacterDataService;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ubv.client.logic.ClientCharacterSelectState;
using UnityEngine.EventSystems;

namespace ubv.ui.client
{
    public class ClientCharacterSelectUI : TabBehaviour
    {
        [SerializeField] private ubv.client.logic.ClientCharacterSelectState m_characterSelectState;
        [SerializeField] private Button m_searchButton;
        [SerializeField] private AddCharacterUI m_addCharacterForm;
        [SerializeField] private TextMeshProUGUI m_characterName;
        [SerializeField] private Transform m_countDotListParent;
        [SerializeField] private Image m_defaultCountDotItem;
        [SerializeField] private Color m_normalDotColor;
        [SerializeField] private Color m_selectedDotColor;
        private List<Image> m_characterCountDot;
        private EventSystem system;

        private void Awake()
        {
            system = EventSystem.current;
            m_characterCountDot = new List<Image>();
            m_characterCountDot.Add(m_defaultCountDotItem);
        }

        protected override void Update()
        {
            base.Update();

            if (Time.frameCount % 69 == 0)
            {
                m_characterName.text = m_characterSelectState.GetActiveCharacter()?.Name;
                SetSelectionDotColor(m_characterSelectState.GetCharacterCount());
                if (system.currentSelectedGameObject == null)
                {
                    m_searchButton.Select();
                }
            }
        }

        public void NextCharacter()
        {
            string characterName = m_characterSelectState.NextCharacter();
            if (characterName != null)
            {
                m_characterName.text = characterName;
                SetSelectionDotColor(m_characterSelectState.GetCharacterCount());
            }  
        }

        public void PreviousCharacter()
        {
            string characterName = m_characterSelectState.PreviousCharacter();
            if (characterName != null)
            {
                m_characterName.text = characterName;
                SetSelectionDotColor(m_characterSelectState.GetCharacterCount());
            }
        }

        public void DeleteActiveCharacter()
        {
            m_characterSelectState.DeleteActiveCharacter();
        }

        public void Back()
        {
            m_characterSelectState.GoBackToPreviousState();
        }

        private void SetSelectionDotColor(CharacterCount characterCount)
        {
            while (m_characterCountDot.Count != characterCount.total)
            {
                if(m_characterCountDot.Count > characterCount.total)
                {
                    int lastIndex = m_characterCountDot.Count - 1;
                    Destroy(m_characterCountDot[lastIndex].gameObject);
                    m_characterCountDot.RemoveAt(lastIndex);
                }
                else if(m_characterCountDot.Count < characterCount.total)
                {
                    Image playerItem = GameObject.Instantiate(m_defaultCountDotItem, m_countDotListParent);
                    m_characterCountDot.Add(playerItem);
                }
                else
                {
                    break;
                }
            }

            for (int i = 0; i < m_characterCountDot.Count; i++)
            {
                Color color = m_normalDotColor;
                if(i == characterCount.selected)
                {
                    color = m_selectedDotColor;
                }
                m_characterCountDot[i].color = color;
            }
        }
    }
}
