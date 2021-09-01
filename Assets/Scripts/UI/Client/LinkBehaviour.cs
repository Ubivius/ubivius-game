using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace ubv.ui.client
{
    [RequireComponent(typeof(Button))]
    public class LinkBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        TMP_Text m_selectable;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color hoverColor;

        private void Awake()
        {
            m_selectable = this.GetComponentInChildren<TMP_Text>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_selectable.color = hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_selectable.color = normalColor;
        }
    }
}
