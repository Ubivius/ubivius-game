using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using ubv.microservices;
using UnityEngine;
using UnityEngine.UI;
using static ubv.microservices.CharacterDataService;

namespace ubv.ui.client
{
    public class PlayerInLobby : MonoBehaviour
    {
        [SerializeField] private HorizontalLayoutGroup m_playerInfo;
        [SerializeField] private TextMeshProUGUI m_character;
        [SerializeField] private TextMeshProUGUI m_username;
        [SerializeField] private TextMeshProUGUI m_status;
        [SerializeField] private Transform m_placeholder;

        private bool m_isVisible = false;

        public void ShowPlayer(UserInfo user, CharacterData character)
        {
            m_character.text = character.Name;
            m_username.text = user.UserName;
            string statusText = "NOT READY";
            if (user.Ready)
            {
                statusText = "READY";
            }
            m_status.text = statusText;

            m_playerInfo.gameObject.SetActive(true);
            if(m_placeholder)
            {
                m_placeholder.gameObject.SetActive(false);
            }
            m_isVisible = true;
        }

        public void HidePlayer()
        {
            m_playerInfo.gameObject.SetActive(false);
            if (m_placeholder)
            {
                m_placeholder.gameObject.SetActive(true);
            }
            m_isVisible = false;
        }

        public void SetStatus(string status)
        {
            m_status.text = status; 
        }

        public bool IsVisible()
        {
            return m_isVisible;
        }
    }
}
