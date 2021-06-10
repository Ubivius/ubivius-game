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
        public void OnPointerEnter(PointerEventData eventData)
        {
            this.GetComponentInChildren<TMP_Text>().color = Color.black;
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            this.GetComponentInChildren<TMP_Text>().color = new Color(56f / 255f, 56f / 255f, 56f / 255f);
        }
    }
}