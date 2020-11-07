using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ubv
{
    public class InputFrame
    {
        public bool Sprinting;
        public Vector2 Movement;
        public uint Tick;

        /*private byte[] m_cachedMove_x;
        private byte[] m_cachedMove_y;
        private byte[] m_cachedTick;*/

        public byte[] ToBytes() // TODO: make it so every first byte automatically corresponds to the type?
        {
            byte[] bytes = new byte[1 + 4 + 4 + 4 + 1]; // 4 Bytes per float

            byte[] move_x;
            byte[] move_y;
            byte[] tick;

            move_x = System.BitConverter.GetBytes(Movement.x);
            move_y = System.BitConverter.GetBytes(Movement.y);
            tick = System.BitConverter.GetBytes(Tick);

            bytes[0] = (byte)Serialization.BYTE_TYPE.INPUT_FRAME;
            bytes[1] = (byte)(Sprinting ? 1 : 0);

            for (ushort i = 0; i < 4; i++)
            {
                bytes[i + 1 + 1] = move_x[i];
                bytes[i + 1 + 4 + 1] = move_y[i];
                bytes[i + 1 + 4 + 4 + 1] = tick[i];
            }

            return bytes;
        }
        
        static public byte[] ToBytes(InputFrame frame)
        {
            return frame.ToBytes();
        }

        static public InputFrame FromBytes(byte[] arr)
        {
            if (arr[0] != (byte)Serialization.BYTE_TYPE.INPUT_FRAME)
                return null;

            InputFrame frame = new InputFrame();
            frame.Sprinting = arr[1] == 1;
            frame.Movement.x = System.BitConverter.ToSingle(arr, 2);
            frame.Movement.y = System.BitConverter.ToSingle(arr, 4 + 1 + 1);
            frame.Tick = System.BitConverter.ToUInt32(arr, 4 + 4 + 1 + 1);
            return frame;
        }
    }

    public class InputController : MonoBehaviour, IClientStateUpdater
    {
        // TODO: make data and  behaviour available to server (to make it symetrical)
        [Header("Movement parameters")]
        [SerializeField] private StandardMovementSettings m_movementSettings;
        [SerializeField] private ClientSync m_clientSync;

        private Rigidbody2D m_rigidBody;

        private PlayerControls m_controls;
        
        private InputFrame m_currentInputFrame;
        
        private void Awake()
        {
            m_currentInputFrame = new InputFrame();
            m_rigidBody = GetComponent<Rigidbody2D>();

            m_controls = new PlayerControls();

            m_controls.Gameplay.Move.performed += context => m_currentInputFrame.Movement = context.ReadValue<Vector2>();
            m_controls.Gameplay.Move.canceled += context => m_currentInputFrame.Movement = Vector2.zero;

            m_controls.Gameplay.Sprint.performed += context => m_currentInputFrame.Sprinting = true;
            m_controls.Gameplay.Sprint.canceled += context => m_currentInputFrame.Sprinting = false;
            
            ClientState.RegisterUpdater(this);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // ...
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

        public void ClientStep(ref ClientState state, InputFrame input, float deltaTime)
        {
            m_rigidBody.MovePosition(state.Position + 
                input.Movement * (input.Sprinting ? m_movementSettings.SprintVelocity : m_movementSettings.WalkVelocity) * deltaTime);
        }

        public void SetClientState(ref ClientState state)
        {
            state.Position = m_rigidBody.position;
        }

        public void UpdateFromState(ClientState state)
        {
            m_rigidBody.position = state.Position;
        }

        public bool NeedsCorrection(ClientState localState, ClientState remoteState)
        {
            return false;
        }
    }
}