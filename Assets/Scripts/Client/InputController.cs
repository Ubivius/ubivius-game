using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ubv
{
    namespace client
    {
        /// <summary>
        /// Sets the 
        /// </summary>
        public class InputController : MonoBehaviour
        {
            // http://sbcgames.io/share-your-common-code-between-multiple-unity-projects/
            // check http://devleader.ca/2015/02/08/multiple-c-projects-unity-3d-solution/ ?
            // TODO: make data and  behaviour available to server (to make it symetrical)
            
            private PlayerControls m_controls;

            static private InputController m_instance;
            static private common.data.InputFrame m_currentInputFrame;

            private Vector2 m_move = Vector2.zero;
            private bool m_IsSprinting = false;
            private bool m_interact = false;

            private void Awake()
            {
#if DEBUG
                Debug.Assert(m_instance == null, "The InputController may only exist once.");
#endif 
                m_instance = this;

                m_currentInputFrame = new common.data.InputFrame();

                m_controls = new PlayerControls();

                m_controls.Gameplay.Move.performed += context => MoveCharacter(context.ReadValue<Vector2>());
                m_controls.Gameplay.Move.canceled += context => MoveCharacter(Vector2.zero);

                m_controls.Gameplay.Sprint.performed += context => SetSprinting(true);
                m_controls.Gameplay.Sprint.canceled += context => SetSprinting(false);

                m_controls.Gameplay.Interact.performed += context => Interact();
            }

            // Start is called before the first frame update
            void Start()
            {

            }

            private void MoveCharacter(Vector2 movement)
            {
                m_move = movement;
            }

            private void SetSprinting(bool isSprinting)
            {
                m_IsSprinting = isSprinting;
            }

            private void Interact()
            {
                m_interact = true;
            }

            // Update is called once per frame
            void Update()
            {
                m_currentInputFrame.Movement.Value = m_move;
                m_currentInputFrame.Sprinting.Value = m_IsSprinting;
                m_currentInputFrame.Interact.Value = m_interact;
            }
        
            static public common.data.InputFrame CurrentFrame()
            {
                return m_currentInputFrame;
            }

            private void FixedUpdate()
            {
                // ...
            }

            private void OnEnable()
            {
                m_controls.Gameplay.Enable();
            }

            private void OnDisable()
            {
                m_controls.Gameplay.Disable();
            }
        }
    }
}
