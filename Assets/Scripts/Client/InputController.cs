using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ubv.client.logic;

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

            [SerializeField] private PlayerGameObjectUpdater m_playerUpdater;
            [SerializeField] private Camera cam;

            private Transform m_playerTransform;
            private PlayerControls m_controls;

            static private InputController m_instance;
            static private common.data.InputFrame m_currentInputFrame;

            private Vector2 m_move = Vector2.zero;
            private bool m_IsSprinting = false;
            private bool m_interact = false;
            private bool m_IsShooting = false;
            private Vector2 m_aim = Vector2.zero;

            private bool m_isAiming = false;

            private void Awake()
            {
#if DEBUG
                Debug.Assert(m_instance == null, "The InputController may only exist once.");
#endif 
                m_playerUpdater.OnInitialized += () => { m_playerTransform = m_playerUpdater.GetLocalPlayerTransform(); };

                m_instance = this;

                m_currentInputFrame = new common.data.InputFrame();

                m_controls = new PlayerControls();

                m_controls.Gameplay.Move.performed += context => MoveCharacter(context.ReadValue<Vector2>());
                m_controls.Gameplay.Move.canceled += context => MoveCharacter(Vector2.zero);

                m_controls.Gameplay.Sprint.performed += context => SetSprinting(true);
                m_controls.Gameplay.Sprint.canceled += context => SetSprinting(false);

                m_controls.Gameplay.Interact.performed += context => Interact(true);
                m_controls.Gameplay.Interact.canceled += context => Interact(false);

                m_controls.Gameplay.Shoot.performed += context => SetShooting(true);
                m_controls.Gameplay.Shoot.canceled += context => SetShooting(false);

                m_controls.Gameplay.Aim.performed += context => SetAim(context.ReadValue<Vector2>());
                m_controls.Gameplay.Aim.canceled += context => SetAim(Vector2.zero);
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

            private void Interact(bool interact)
            {
                m_interact = interact;
            }

            private void SetShooting(bool isShooting)
            {
                m_IsShooting = isShooting;
            }

            private void SetAim(Vector2 movement)
            {
                if (movement == Vector2.zero)
                {
                    m_isAiming = false;
                }
                else
                {
                    m_isAiming = true;
                }
                m_aim = movement;
            }

            // Update is called once per frame
            void Update()
            {
                m_currentInputFrame.Movement.Value = m_move;
                m_currentInputFrame.Sprinting.Value = m_IsSprinting;
                m_currentInputFrame.Interact.Value = m_interact;

                m_currentInputFrame.Shooting.Value = m_IsShooting;

                if (!m_isAiming)
                {
                    Vector2 aimDir = Vector2.zero;
                    if (m_playerTransform != null)
                    {
                        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
                        aimDir = mousePos - (Vector2)m_playerTransform.position;
                    }
                    m_currentInputFrame.ShootingDirection.Value = aimDir.normalized;
                }
                else
                {
                    m_currentInputFrame.ShootingDirection.Value = m_aim.normalized;
                }
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
