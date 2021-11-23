using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ubv.ui.client {
    public class Achievement: MonoBehaviour
    {
        [SerializeField] private Color m_baseColor;
        [SerializeField] private Color m_activeColor;
        [SerializeField] private Color m_dropdownColor;
        [SerializeField] private Color m_lockedColor;

        [SerializeField] private GameObject m_body;

        [SerializeField] private GameObject m_header;

        [SerializeField] private Image m_icon;
        [SerializeField] private Button m_button;
        [SerializeField] private TextMeshProUGUI m_name;
        [SerializeField] private TextMeshProUGUI m_date;
        [SerializeField] private TextMeshProUGUI m_description;

        [SerializeField] private GameObject m_dropDown;

        private bool m_isActive;
        

        void Start() {
            m_header.GetComponent<Image>().color = m_baseColor;
            m_dropDown.GetComponent<Image>().color = m_dropdownColor;

            m_button = m_header.GetComponent<Button>();
            //m_button.interactable = false;
        }

        public void ToggleDropdown() {
            m_isActive = !m_isActive;

            if (m_isActive)
            {
                m_header.GetComponent<Image>().color = m_activeColor;
                m_dropDown.SetActive(true);
            }
            else
            {
                m_header.GetComponent<Image>().color = m_baseColor;
                m_dropDown.SetActive(false);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(m_body.transform.parent.gameObject.GetComponent<RectTransform>());
        }

        public void Unlock() {
            m_button.interactable = true;
            var t_color = m_name.color;
            t_color.a = 1f;
            m_name.color = t_color;
            m_date.color = t_color;
            m_icon.color = t_color;
        }

        public void InitializeAchievement(string a_name, string a_description, string a_icon) {
            m_name.text = a_name;
            m_description.text = a_description;
            m_date.text = "///";
            // m_icon.text = a_icon;
        }
    }
}
