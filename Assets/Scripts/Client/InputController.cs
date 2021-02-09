using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ubv.client
{ 
    public class InputController : MonoBehaviour, IClientStateUpdater
    {
<<<<<<< HEAD
        // http://sbcgames.io/share-your-common-code-between-multiple-unity-projects/
        // check http://devleader.ca/2015/02/08/multiple-c-projects-unity-3d-solution/ ?
        // TODO: make data and  behaviour available to server (to make it symetrical)
        [Header("Movement parameters")]
        [SerializeField] private common.StandardMovementSettings m_movementSettings;

        private Rigidbody2D m_rigidBody;

        private PlayerControls m_controls;

        private common.data.InputFrame m_currentInputFrame;
=======
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
>>>>>>> origin/master

        private Vector2 m_move = Vector2.zero;
        private bool m_IsSprinting = false;

<<<<<<< HEAD
        private void Awake()
        {
            m_currentInputFrame = new common.data.InputFrame();
            m_rigidBody = GetComponent<Rigidbody2D>();
=======
            private void Awake()
            {
#if DEBUG
                Debug.Assert(m_instance == null, "The InputController may only exist once.");
#endif 
                m_instance = this;

                m_currentInputFrame = new common.data.InputFrame();
>>>>>>> origin/master

            m_controls = new PlayerControls();

<<<<<<< HEAD
            m_controls.Gameplay.Move.performed += context => MoveCaracter(context.ReadValue<Vector2>());
            m_controls.Gameplay.Move.canceled += context => MoveCaracter(Vector2.zero);

            m_controls.Gameplay.Sprint.performed += context => SetSprinting(true);
            m_controls.Gameplay.Sprint.canceled += context => SetSprinting(false);

            client.ClientState.RegisterUpdater(this);
        }
=======
                m_controls.Gameplay.Move.performed += context => MoveCharacter(context.ReadValue<Vector2>());
                m_controls.Gameplay.Move.canceled += context => MoveCharacter(Vector2.zero);

                m_controls.Gameplay.Sprint.performed += context => SetSprinting(true);
                m_controls.Gameplay.Sprint.canceled += context => SetSprinting(false);
            }
>>>>>>> origin/master

        // Start is called before the first frame update
        void Start()
        {

        }

<<<<<<< HEAD
        void MoveCaracter(Vector2 movement)
        {
=======
            private void MoveCharacter(Vector2 movement)
            {
>>>>>>> origin/master
#if DEBUG_LOG
            Debug.Log("Trying to apply this move -> " + movement.ToString());
#endif //DEBUG_LOG
            m_move = movement;
        }

<<<<<<< HEAD
        void SetSprinting(bool isSprinting)
        {
=======
            private void SetSprinting(bool isSprinting)
            {
>>>>>>> origin/master
#if DEBUG_LOG
            Debug.Log("Trying to apply this isSprinting -> " + isSprinting.ToString());
#endif //DEBUG_LOG
            m_IsSprinting = isSprinting;
        }

        // Update is called once per frame
        void Update()
        {
            m_currentInputFrame.Movement.Set(m_move);
            m_currentInputFrame.Sprinting.Set(m_IsSprinting);
        }
        
<<<<<<< HEAD
        public common.data.InputFrame CurrentFrame()
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

        public void SetStateAndStep(ref client.ClientState state, common.data.InputFrame input, float deltaTime)
        {
            SetState(ref state);
            common.logic.PlayerMovement.Execute(ref m_rigidBody, m_movementSettings, input, deltaTime);
        }

        public void UpdateFromState(client.ClientState state)
        {
            m_rigidBody.position = state.GetPlayer().Position;
        }

        public bool NeedsCorrection(client.ClientState remoteState)
        {
            return (m_rigidBody.position - remoteState.GetPlayer().Position).sqrMagnitude > 0.01f;
        }

        private void SetState(ref client.ClientState state)
        {
            state.GetPlayer().Position.Set(m_rigidBody.position);
=======
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
>>>>>>> origin/master
        }
    }
}
