using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ubv.ui.client
{
    public class ClientCharacterSelectUI : TabBehaviour
    {
        [SerializeField] private ubv.client.logic.ClientCharacterSelectState m_initState;
        [SerializeField] private Button m_searchButton;
        [SerializeField] private AddCharacterUI m_addCharacterForm;
        private EventSystem system;

        private void Awake()
        {
            system = EventSystem.current;
        }

        protected override void Update()
        {
            base.Update();

            if (Time.frameCount % 69 == 0)
            {
                if (system.currentSelectedGameObject == null)
                {
                    m_searchButton.Select();
                }
            }
        }


        public void Back()
        {
            m_initState.Back();
        }
    }
}
