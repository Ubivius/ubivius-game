using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ubv
{
    public class InputFrame : Serializable
    {
        public SerializableTypes.Bool Sprinting;
        public SerializableTypes.Vector2 Movement;
        public SerializableTypes.Uint32 Tick;
        
        /*protected override byte[] InternalToBytes() 
        {
            // Sprinting 1
            // Movement 8
            // tick 4
            byte[] bytes = new byte[13]; 

            byte[] move_x;
            byte[] move_y;
            byte[] tick;

            move_x = System.BitConverter.GetBytes(Movement.Value.x);
            move_y = System.BitConverter.GetBytes(Movement.Value.y);
            tick = System.BitConverter.GetBytes(Tick);
            
            bytes[0] = (byte)(Sprinting ? 1 : 0);

            for (ushort i = 0; i < 4; i++)
            {
                bytes[i + 1] = move_x[i];
                bytes[i + 1 + 4] = move_y[i];
                bytes[i + 1 + 4 + 4] = tick[i];
            }

            return bytes;
        }*/

        public void SetToNeutral()
        {
            Movement.Set(Vector2.zero);
            Sprinting.Set(false);
        }
        
        protected override void InitSerializableMembers()
        {
            Sprinting = new SerializableTypes.Bool(this, false);
            Movement = new SerializableTypes.Vector2(this, Vector2.zero);
            Tick = new SerializableTypes.Uint32(this, 0);

            SetToNeutral();
        }
        
        protected override byte SerializationID()
        {
            return (byte)Serialization.BYTE_TYPE.INPUT_FRAME;
        }
    }

    public class InputController : MonoBehaviour, IClientStateUpdater
    {
        // http://sbcgames.io/share-your-common-code-between-multiple-unity-projects/
        // check http://devleader.ca/2015/02/08/multiple-c-projects-unity-3d-solution/ ?
        // TODO: make data and  behaviour available to server (to make it symetrical)
        [Header("Movement parameters")]
        [SerializeField] private StandardMovementSettings m_movementSettings;
        [SerializeField] private ClientSync m_clientSync;
        [SerializeField] private bool m_isServerBind;

        private Rigidbody2D m_rigidBody;

        private PlayerControls m_controls;
        
        private InputFrame m_currentInputFrame;

        private Vector2 m_move = Vector2.zero;
        private bool m_IsSprinting = false;
        
        private void Awake()
        {
            m_currentInputFrame = new InputFrame();
            m_rigidBody = GetComponent<Rigidbody2D>();

            m_controls = new PlayerControls();
            Debug.Log("INPUT AWAKE");
            m_controls.Gameplay.Move.performed += context => MoveCaracter(context.ReadValue<Vector2>());
            m_controls.Gameplay.Move.canceled += context => MoveCaracter(Vector2.zero);

            m_controls.Gameplay.Sprint.performed += context => SetSprinting(true);
            m_controls.Gameplay.Sprint.canceled += context => SetSprinting(false);

            m_clientSync.IsServerBind = m_isServerBind;

            ClientState.RegisterUpdater(this);
        }

        void MoveCaracter(Vector2 movement)
        {
            Debug.Log("Trying to apply this move -> " + movement.ToString());
            m_move = movement;
        }

        void SetSprinting(bool isSprinting)
        {
            Debug.Log("Trying to apply this isSprinting -> " + isSprinting.ToString());
            m_IsSprinting = isSprinting;
        }

        // Start is called before the first frame update
        void Start()
        {

        }
        
        // Update is called once per frame
        void Update()
        {
            m_currentInputFrame.Movement.Set(m_move);
            m_currentInputFrame.Sprinting.Set(m_IsSprinting);

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

        public void ClientStoreAndStep(ref ClientState state, InputFrame input, float deltaTime)
        {
            SetClientState(ref state);

#if DEBUG_LOG

#endif // DEBUG_LOG
            //Debug.Log("Moving client at frame " + input.Tick); // + " with input " + input.Movement );

            m_rigidBody.MovePosition(m_rigidBody.position + 
                input.Movement.Value * (input.Sprinting ? m_movementSettings.SprintVelocity : m_movementSettings.WalkVelocity) * deltaTime);
        }
        
        public void UpdateFromState(ClientState state)
        {
            m_rigidBody.position = state.Position;
        }

        public bool NeedsCorrection(ClientState localState, ClientState remoteState)
        {
            return false;
        }

        private void SetClientState(ref ClientState state)
        {
            state.Position = m_rigidBody.position;
        }
    }
}