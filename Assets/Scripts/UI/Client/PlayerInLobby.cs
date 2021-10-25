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

        public void AddPlayer(UserInfo user, CharacterData character)
        {
            m_character.text = character.Name;
            m_username.text = user.UserName;
            m_status.text = "NOT READY";

            m_playerInfo.gameObject.SetActive(true);
            if(m_placeholder)
            {
                m_placeholder.gameObject.SetActive(false);
            }
        }

        public void RemovePlayer()
        {
            m_playerInfo.gameObject.SetActive(false);
            if (m_placeholder)
            {
                m_placeholder.gameObject.SetActive(true);
            }
        }

        public void SetStatus(string status)
        {
            m_status.text = status; 
        }
    }
}
