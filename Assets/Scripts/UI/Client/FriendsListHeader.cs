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
    public class FriendsListHeader : MonoBehaviour
    {
        [SerializeField] private Image m_chevron;
        [SerializeField] private LayoutGroup m_content;
        private bool m_isHidden = false;

        public void Toggle()
        {
            m_content.gameObject.SetActive(m_isHidden);
            m_isHidden = !m_isHidden;

            if (m_isHidden)
            {
                m_chevron.transform.Rotate(new Vector3(0, 0, 90));
            }
            else
            {
                m_chevron.transform.Rotate(new Vector3(0, 0, -90));
            }
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
