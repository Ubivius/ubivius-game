using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using ubv.microservices;
using UnityEngine;
using static ubv.microservices.CharacterDataService;

namespace ubv.ui.client
{
    public class PlayerInLobby : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_character;
        [SerializeField] private TextMeshProUGUI m_username;
        [SerializeField] private TextMeshProUGUI m_status;
        [SerializeField] private Transform m_placeholder;

        public void AddPlayer(UserInfo user, CharacterData character)
        {
            m_character.text = character.Name;
            m_username.text = user.UserName;
            m_status.text = "NOT READY";

            m_character.gameObject.SetActive(true);
            m_username.gameObject.SetActive(true);
            m_status.gameObject.SetActive(true);
            if(m_placeholder)
            {
                m_placeholder.gameObject.SetActive(false);
            }
        }

        public void RemovePlayer()
        {
            m_character.gameObject.SetActive(false);
            m_username.gameObject.SetActive(false);
            m_status.gameObject.SetActive(false);
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
