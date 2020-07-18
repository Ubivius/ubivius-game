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
        
        public byte[] ToBytes()
        {
            byte[] move_x = System.BitConverter.GetBytes(Movement.x);
            byte[] move_y = System.BitConverter.GetBytes(Movement.y);
            byte[] arr = new byte[4 + 4 + 1]; // 4 Bytes per float
            arr[0] = (byte)(Sprinting ? 1 : 0);
            
            for(ushort i = 0; i < 4; i++)
                arr[i + 1] = move_x[i];

            for (ushort i = 0; i < 4; i++)
                arr[i + 1 + 4] = move_y[i];

            return arr;
        }
        
        static public byte[] ToBytes(InputFrame frame)
        {
            return frame.ToBytes();
        }

        static public InputFrame FromBytes(byte[] arr)
        {
            InputFrame frame = new InputFrame
            {
                Sprinting = arr[0] == 1,
                Movement = new Vector2(System.BitConverter.ToSingle(arr, 1), System.BitConverter.ToSingle(arr, 4 + 1))
            };
            return frame;
        }
    }

    public class InputController : MonoBehaviour, IClientStateUpdater
    {
        [Header("Movement parameters")]
        [SerializeField] private float m_velocity;
        [SerializeField] private float m_walk_velocity;
        [SerializeField] private float m_sprint_velocity;
        [SerializeField] private bool m_sprinting;
        [SerializeField] private float m_acceleration;
        [SerializeField] private ClientSync m_clientSync;

        private Rigidbody2D m_rigidBody;

        private PlayerControls m_controls;

        private Vector2 m_playerMouvement;
        private InputFrame m_currentInputFrame;

        // test pour réseau, à retirer.
        private bool m_envoieTest;

        private void Awake()
        {
            m_currentInputFrame = new InputFrame();
            m_sprinting = false;
            m_envoieTest = false;
            m_rigidBody = GetComponent<Rigidbody2D>();

            m_controls = new PlayerControls();

            m_controls.Gameplay.Move.performed += context => m_currentInputFrame.Movement = context.ReadValue<Vector2>();
            m_controls.Gameplay.Move.canceled += context => m_currentInputFrame.Movement = Vector2.zero;

            m_controls.Gameplay.Sprint.performed += context => m_currentInputFrame.Sprinting = true;
            m_controls.Gameplay.Sprint.canceled += context => m_currentInputFrame.Sprinting = false;

            m_controls.Gameplay.TestServer.performed += context => m_envoieTest = true;
            m_controls.Gameplay.TestServer.canceled += context => m_envoieTest = false;
            
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
            m_rigidBody.MovePosition(m_rigidBody.position + 
                input.Movement * (input.Sprinting ? m_sprint_velocity : m_walk_velocity) * deltaTime);

            state.Position = m_rigidBody.position;
        }
    }
}