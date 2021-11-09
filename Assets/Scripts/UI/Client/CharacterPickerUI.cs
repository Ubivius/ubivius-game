using System;
using System.Collections.Generic;
using TMPro;
using ubv.client.logic;
using ubv.microservices;
using UnityEngine;
using UnityEngine.UI;
using static ubv.microservices.CharacterDataService;

namespace ubv.ui.client
{
    public class CharacterPickerUI : MonoBehaviour
    {
        private CharacterDataService m_characterServices;
        private SocialServicesController m_socialServices;

        [Header("UI components")]
        [SerializeField] private TextMeshProUGUI m_characterName;
        [SerializeField] private Transform m_countDotListParent;
        [SerializeField] private Image m_defaultCountDotItem;
        [SerializeField] private Image m_characterImage;
        [SerializeField] private TextMeshProUGUI m_noCharacter;

        [Header("Colors")]
        [SerializeField] private Color m_normalDotColor;
        [SerializeField] private Color m_selectedDotColor;

        [SerializeField] private bool m_onlyAlive;

        private PlayerControls m_controls;
        private CharacterData[] m_cachedCharacters = {};
        private int m_selectedCharacterIndex = 0;
        private List<Image> m_characterCountDot;
        private bool m_selectLast = false;
        private bool m_awake = true;

        private void Awake()
        {
            m_controls = new PlayerControls();
            m_controls.Menu.LeftTrigger.canceled += context => PreviousCharacter();
            m_controls.Menu.RightTrigger.canceled += context => NextCharacter();
            m_controls.Menu.Enable();
            m_characterCountDot = new List<Image> { m_defaultCountDotItem };
            m_characterServices = ClientSyncState.CharacterService;
            m_socialServices = ClientSyncState.SocialServices;

            RefreshCharacters();
        }

        private void Update()
        {
            if (Time.frameCount % 69 == 0)
            {
                if(m_cachedCharacters.Length > 0)
                {
                    m_characterName.text = m_cachedCharacters[m_selectedCharacterIndex]?.Name;
                    m_noCharacter.gameObject.SetActive(false);
                    m_characterImage.gameObject.SetActive(true);
                }
                else
                {
                    m_characterName.text = "";
                    m_noCharacter.gameObject.SetActive(true);
                    m_characterImage.gameObject.SetActive(false);
                }
                SetSelectionDotColor();
            }
        }

        public void RefreshCharacters(bool selectLast = false)
        {
            m_selectLast = selectLast;
            if (m_onlyAlive)
            {
                m_characterServices.Request(new GetCharactersAliveFromUserRequest(m_socialServices.CurrentUser.ID, OnCharactersFetchedFromService));
            }
            else
            {
                m_characterServices.Request(new GetCharactersFromUserRequest(m_socialServices.CurrentUser.ID, OnCharactersFetchedFromService));
            }
            
        }

        private void OnCharactersFetchedFromService(CharacterData[] characters)
        {
            m_cachedCharacters = characters;
            if (m_awake)
            {
                int index = Array.FindIndex(m_cachedCharacters, character => character.ID == ubv.client.data.LoadingData.ActiveCharacterID);
                if(index > -1)
                {
                    m_selectedCharacterIndex = index;
                }
                m_awake = false;
            }
            SetSelectedCharacter();
        }

        public CharacterData GetActiveCharacter()
        {
            if(m_cachedCharacters.Length == 0)
            {
                return null;
            }
            return m_cachedCharacters[m_selectedCharacterIndex];
        }

        public bool AsCharacters()
        {
            return m_cachedCharacters.Length > 0;
        }

        public void NextCharacter()
        {
            if (m_cachedCharacters.Length > 0)
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
        }

        public void PreviousCharacter()
        {
            if (m_cachedCharacters.Length > 0)
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
        }

        public void SetSelectedCharacter()
        {
            if (m_selectLast || m_selectedCharacterIndex > m_cachedCharacters.Length - 1)
            {
                m_selectedCharacterIndex = m_cachedCharacters.Length - 1;
                m_selectLast = false;
            }
            else if (m_selectedCharacterIndex < 0)
            {
                m_selectedCharacterIndex = 0;
            }

            CharacterData character = m_cachedCharacters[m_selectedCharacterIndex];
            ubv.client.data.LoadingData.ActiveCharacterID = character.ID;
        }

        private void SetSelectionDotColor()
        {
            while (m_characterCountDot.Count != m_cachedCharacters.Length || m_characterCountDot.Count != 1)
            {
                if (m_characterCountDot.Count > m_cachedCharacters.Length && m_cachedCharacters.Length > 0)
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
