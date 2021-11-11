using ubv.client.logic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ubv.ui.client
{
    public class ModalUI : MonoBehaviour
    {
        [SerializeField] private Selectable firstSelected;
        [SerializeField] protected ClientSyncState m_state;
        private bool m_canCloseModal = false;
        private PlayerControls m_controls;
        private EventSystem system;

        private void Awake()
        {
            m_controls = new PlayerControls();
            m_controls.Menu.Back.canceled += context => CloseModal();
            m_controls.Menu.Enable();
            system = EventSystem.current;
        }

        protected virtual void Update()
        {
            if (m_canCloseModal)
            {
                m_canCloseModal = false;
                CloseModal();
            }
        }

        public virtual void OpenModal()
        {
            m_state.SetCanBack(false);
            m_canCloseModal = false;
            gameObject.SetActive(true);
            firstSelected.Select();
        }

        public virtual void CloseModal()
        {
            gameObject.SetActive(false);
            system.SetSelectedGameObject(null);
            m_state.SetCanBack(true);
        }

        public void SetCanCloseModal(bool canCloseModal)
        {
            m_canCloseModal = canCloseModal;
        }
    }
}
