using TMPro;
using UnityEngine;

namespace ubv.ui.client
{
    public class ConfirmationModalUI : ModalUI
    {
        [SerializeField] private TextMeshProUGUI m_headerText;
        [SerializeField] private TextMeshProUGUI m_bodyText;

        public void SetBodyText(string text)
        {
            m_bodyText.text = text;
        }
    }
}
