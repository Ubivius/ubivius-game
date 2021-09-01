using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace ubv.ui.client
{
    [RequireComponent(typeof(Button))]
    public class LinkBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        TMP_Text m_text;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color hoverColor;

        private void Awake()
        {
            m_text = this.GetComponentInChildren<TMP_Text>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_text.color = hoverColor;
        }

        public void OnSelect(BaseEventData eventData)
        {
            m_text.color = hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_text.color = normalColor;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_text.color = normalColor;
        }
    }
}
