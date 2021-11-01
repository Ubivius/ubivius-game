using System.Collections.Generic;
using TMPro;
using ubv.client.logic;
using ubv.microservices;
using UnityEngine;
using UnityEngine.UI;
using static ubv.microservices.CharacterDataService;

namespace ubv.ui.client
{
    public class CharacterPickerUI : TabBehaviour
    {
        private CharacterDataService m_characterServices;
        private SocialServicesController m_socialServices;

        [Header("UI components")]
        [SerializeField] private TextMeshProUGUI m_characterName;
        [SerializeField] private Transform m_countDotListParent;
        [SerializeField] private Image m_defaultCountDotItem;

        [Header("Colors")]
        [SerializeField] private Color m_normalDotColor;
        [SerializeField] private Color m_selectedDotColor;


        private CharacterData[] m_cachedCharacters;
        private int m_selectedCharacterIndex = 0;
        private List<Image> m_characterCountDot;

        private void Awake()
        {
            m_characterCountDot = new List<Image> { m_defaultCountDotItem };
            m_characterServices = ClientSyncState.CharacterService;
            m_socialServices = ClientSyncState.SocialServices;
            m_characterServices.Request(new GetCharactersFromUserRequest(m_socialServices.CurrentUser.ID, OnCharactersFetchedFromService));
        }

        protected override void Update()
        {
            base.Update();

            if (Time.frameCount % 69 == 0)
            {
                m_characterName.text = m_cachedCharacters[m_selectedCharacterIndex]?.Name;
                SetSelectionDotColor();
            }
        }

        private void OnCharactersFetchedFromService(CharacterData[] characters)
        {
            m_cachedCharacters = characters;
            SetActiveCharacter(m_cachedCharacters[m_selectedCharacterIndex].ID);
        }

        public void SetActiveCharacter(string characterID)
        {
            ubv.client.data.LoadingData.ActiveCharacterID = characterID;
        }

        public void NextCharacter()
        {
            if (m_selectedCharacterIndex >= m_cachedCharacters.Length - 1)
            {
                m_selectedCharacterIndex = m_cachedCharacters.Length - 1;
            }
            else
            {
                m_selectedCharacterIndex++;
            }

            SetSelectedCharacter();
        }

        public void PreviousCharacter()
        {
            if (m_selectedCharacterIndex <= 0)
            {
                m_selectedCharacterIndex = 0;
            }
            else
            {
                m_selectedCharacterIndex--;
            }
            SetSelectedCharacter();
        }

        public void SetSelectedCharacter()
        {
            CharacterData character = m_cachedCharacters[m_selectedCharacterIndex];
            SetActiveCharacter(character.ID);

            m_characterName.text = character.Name;
            SetSelectionDotColor();
        }

        private void SetSelectionDotColor()
        {
            while (m_characterCountDot.Count != m_cachedCharacters.Length)
            {
                if (m_characterCountDot.Count > m_cachedCharacters.Length)
                {
                    int lastIndex = m_characterCountDot.Count - 1;
                    Destroy(m_characterCountDot[lastIndex].gameObject);
                    m_characterCountDot.RemoveAt(lastIndex);
                }
                else if (m_characterCountDot.Count < m_cachedCharacters.Length)
                {
                    Image playerItem = Instantiate(m_defaultCountDotItem, m_countDotListParent);
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
                if (i == m_selectedCharacterIndex)
                {
                    color = m_selectedDotColor;
                }
                m_characterCountDot[i].color = color;
            }
        }
    }
}
