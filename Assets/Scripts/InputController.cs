using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UBV
{
    public struct InputFrame
    {
        public bool Sprinting;
        public Vector2 Movement;
        public uint Tick;

        private byte[] m_cachedBytes;
        private byte[] m_cachedMove_x;
        private byte[] m_cachedMove_y;
        private byte[] m_cachedTick;

        static private InputFrame m_cachedFrame;

        public byte[] ToBytes()
        {
            if(m_cachedBytes == null)
               m_cachedBytes = new byte[4 + 4 + 4 + 1]; // 4 Bytes per float

            m_cachedMove_x = System.BitConverter.GetBytes(Movement.x);
            m_cachedMove_y = System.BitConverter.GetBytes(Movement.y);
            m_cachedTick = System.BitConverter.GetBytes(Tick);

            m_cachedBytes[0] = (byte)(Sprinting ? 1 : 0);

            for (ushort i = 0; i < 4; i++)
            {
                m_cachedBytes[i + 1] = m_cachedMove_x[i];
                m_cachedBytes[i + 1 + 4] = m_cachedMove_y[i];
                m_cachedBytes[i + 1 + 4 + 4] = m_cachedTick[i];
            }

            return m_cachedBytes;
        }
        
        static public byte[] ToBytes(InputFrame frame)
        {
            return frame.ToBytes();
        }

        static public InputFrame FromBytes(byte[] arr)
        {
            m_cachedFrame.Sprinting = arr[0] == 1;
            m_cachedFrame.Movement.x = System.BitConverter.ToSingle(arr, 1);
            m_cachedFrame.Movement.y = System.BitConverter.ToSingle(arr, 4 + 1);
            m_cachedFrame.Tick = System.BitConverter.ToUInt32(arr, 4 + 4 + 1);
            return m_cachedFrame;
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
            m_clientSync.SetCurrentInputBuffer(m_currentInputFrame);
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

        public void SaveClientState(ref ClientState state)
        {
            state.Position = m_rigidBody.position;
        }
    }
}