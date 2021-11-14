using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ubv.ui.client {
    public class PopupUI : MonoBehaviour
    {
        [SerializeField] private GameObject m_popup;
        [SerializeField] private TextMeshProUGUI m_title;
        [SerializeField] private TextMeshProUGUI m_message;

        public PopupUI(string a_title, Color a_titleColor, string a_message) {
            m_title.text = a_title;
            m_title.color = a_titleColor;
            m_message.text = a_message;
        }

        public void PopupDestroy() {
            Destroy(m_popup);
        }
    }
}
