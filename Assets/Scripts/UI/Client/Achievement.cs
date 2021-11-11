using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ubv.microservices.CharacterDataService;

namespace ubv.ui.client {
    public class Achievement: MonoBehaviour {
        [SerializeField] private TextMeshProUGUI m_description;
        [SerializeField] private TextMeshProUGUI m_name;
        [SerializeField] private Image m_icon;
        [SerializeField] private Transform m_descrition;

        private bool m_isActive;

        private bool m_isVisible = false;

        public void ToggleDescrition() {
            m_isActive != m_isActive;
        }

        public bool IsVisible()
        {
            return m_isVisible;
        }
    }
}