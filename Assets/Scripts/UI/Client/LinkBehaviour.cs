using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ubv.ui.client
{
    public class LinkBehaviour : MonoBehaviour
    {
        [SerializeField] private Button m_link;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnMouseOver()
        {
            m_link.GetComponentInChildren<TMP_Text>().color = Color.black;
        }

        public void OnMouseExit()
        {
            m_link.GetComponentInChildren<TMP_Text>().color = new Color(56f / 255f, 56f / 255f, 56f / 255f);
        }
    }
}