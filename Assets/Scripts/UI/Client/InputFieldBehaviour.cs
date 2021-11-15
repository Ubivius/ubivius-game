using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ubv.ui.client
{
    public class InputFieldBehaviour : TMP_InputField, ISelectHandler
    {
        private EventSystem system;
        private PlayerControls m_controls;

        protected override void Awake()
        {
            system = EventSystem.current;
            m_controls = new PlayerControls();
            m_controls.Menu.Up.performed += context => NavigateUp();
            m_controls.Menu.Down.performed += context => NavigateDown();
            m_controls.Menu.Left.performed += context => NavigateLeft();
            m_controls.Menu.Right.performed += context => NavigateRight();
        }

        private void NavigateUp()
        {
            if (system.currentSelectedGameObject.gameObject == gameObject)
            {
                Selectable next = FindSelectableOnUp();
                if (next != null)
                {
                    StartCoroutine(SelectAfterFrame(next));
                }
            }
        }

        private void NavigateDown()
        {
            if (system.currentSelectedGameObject.gameObject == gameObject)
            {
                Selectable next = FindSelectableOnDown();
                if (next != null)
                {
                    StartCoroutine(SelectAfterFrame(next));
                }
            }
        }

        private void NavigateLeft()
        {
            if (system.currentSelectedGameObject.gameObject == gameObject)
            {
                Selectable next = FindSelectableOnLeft();
                if (next != null)
                {
                    StartCoroutine(SelectAfterFrame(next));
                }
            }
        }

        private void NavigateRight()
        {
            if (system.currentSelectedGameObject.gameObject == gameObject)
            {
                Selectable next = FindSelectableOnRight();
                if (next != null)
                {
                    StartCoroutine(SelectAfterFrame(next));
                }
            }
        }

        private IEnumerator SelectAfterFrame(Selectable next)
        {
            yield return new WaitForSeconds(0.1f);
            next.Select();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_controls.Menu.Enable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_controls.Menu.Disable();
        }
    }
}
