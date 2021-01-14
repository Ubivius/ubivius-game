using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ubv
{
    public class InputController : MonoBehaviour, client.IClientStateUpdater
    {
        // http://sbcgames.io/share-your-common-code-between-multiple-unity-projects/
        // check http://devleader.ca/2015/02/08/multiple-c-projects-unity-3d-solution/ ?
        // TODO: make data and  behaviour available to server (to make it symetrical)
        [Header("Movement parameters")]
        [SerializeField] private StandardMovementSettings m_movementSettings;
        [SerializeField] private client.ClientSync m_clientSync;

        private Rigidbody2D m_rigidBody;

        private PlayerControls m_controls;
        
        private common.data.InputFrame m_currentInputFrame;
        
        private void Awake()
        {
            m_currentInputFrame = new common.data.InputFrame();
            m_rigidBody = GetComponent<Rigidbody2D>();

            m_controls = new PlayerControls();

            m_controls.Gameplay.Move.performed += context => m_currentInputFrame.Movement.Set(context.ReadValue<Vector2>());
            m_controls.Gameplay.Move.canceled += context => m_currentInputFrame.Movement.Set(Vector2.zero);

            m_controls.Gameplay.Sprint.performed += context => m_currentInputFrame.Sprinting.Set(true);
            m_controls.Gameplay.Sprint.canceled += context => m_currentInputFrame.Sprinting.Set(false);
            
            client.ClientState.RegisterUpdater(this);
        }

        // Start is called before the first frame update
        void Start()
        {

        }
        
        // Update is called once per frame
        void Update()
        {
            m_clientSync.AddInput(m_currentInputFrame);
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

        public void ClientStoreAndStep(ref client.ClientState state, common.data.InputFrame input, float deltaTime)
        {
            SetClientState(ref state);
            common.logic.PlayerMovement.Execute(ref m_rigidBody, m_movementSettings, input, deltaTime);
        }
        
        public void UpdateFromState(client.ClientState state)
        {
            m_rigidBody.position = state.Position;
        }

        public bool NeedsCorrection(client.ClientState remoteState)
        {
            return (m_rigidBody.position - remoteState.Position).sqrMagnitude > 0.1f;
        }

        private void SetClientState(ref client.ClientState state)
        {
            state.Position.Set(m_rigidBody.position);
        }
    }
}