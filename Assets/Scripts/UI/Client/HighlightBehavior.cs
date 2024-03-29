﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace ubv.ui.client
{
    [RequireComponent(typeof(Selectable))]
    public class HighlightBehavior : MonoBehaviour, IPointerEnterHandler, IDeselectHandler, ISelectHandler
    {
        private TMP_Text m_text;
        private Selectable m_selectable;
        [SerializeField] Color normalColor;
        [SerializeField] Color hoverColor;
        [SerializeField] Color disableColor;

        private void Awake()
        {
            m_text = this.GetComponentInChildren<TMP_Text>();
            m_selectable = this.GetComponent<Selectable>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!EventSystem.current.alreadySelecting)
            {
                EventSystem.current.SetSelectedGameObject(this.gameObject);
            }
        }


        public void OnSelect(BaseEventData eventData)
        {
            if(m_text)
            {
                if(m_selectable.interactable)
                {
                    m_text.color = hoverColor;
                }
                else
                {
                    m_text.color = disableColor;
                }    
            }    
        }


        public void OnDeselect(BaseEventData eventData)
        {
            m_selectable.OnPointerExit(null);
            if (m_text)
            {
                if (m_selectable.interactable)
                {
                    m_text.color = normalColor;
                }
                else
                {
                    m_text.color = disableColor;
                }
            }
        }
    }
}
