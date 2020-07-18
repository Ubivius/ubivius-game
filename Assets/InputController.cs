using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UBV
{
    public struct InputFrame
    {
        public bool Running;
        public bool Up, Right, Left, Down;

        public byte[] ToBytes()
        {
            byte[] arr = new byte[5];
            arr[0] = (byte)(Running ? 1 : 0);
            arr[1] = (byte)(Up ? 1 : 0);
            arr[2] = (byte)(Right ? 1 : 0);
            arr[3] = (byte)(Down ? 1 : 0);
            arr[4] = (byte)(Left ? 1 : 0);
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
                Running = arr[0] == 1,
                Up = arr[1] == 1,
                Right = arr[2] == 1,
                Down = arr[3] == 1,
                Left = arr[4] == 1
            };
            return frame;
        }
    }

    public class InputController : MonoBehaviour
    {
        [Header("Movement parameters")]
        [SerializeField] private float m_velocity;
        [SerializeField] private float m_walk_velocity;
        [SerializeField] private float m_sprint_velocity;
        [SerializeField] private bool m_sprinting;
        [SerializeField] private float m_acceleration;
        [SerializeField] private UDPClient m_udpClient;

        private Rigidbody2D m_rigidBody;

        private PlayerControls m_controls;

        private Vector2 m_playerMouvement;

        // test pour réseau, à retirer.
        private bool m_envoieTest;

        private void Awake()
        {
            m_sprinting = false;
            m_envoieTest = false;
            m_rigidBody = GetComponent<Rigidbody2D>();

            m_controls = new PlayerControls();

            m_controls.Gameplay.Move.performed += context => m_playerMouvement = context.ReadValue<Vector2>();
            m_controls.Gameplay.Move.canceled += context => m_playerMouvement = Vector2.zero;

            m_controls.Gameplay.Sprint.performed += context => m_sprinting = true;
            m_controls.Gameplay.Sprint.canceled += context => m_sprinting = false;

            m_controls.Gameplay.TestServer.performed += context => m_envoieTest = true;
            m_controls.Gameplay.TestServer.canceled += context => m_envoieTest = false;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // collect input (calls to Input.GetXYZ.(...))

            // compute if needed (ex: trigo with cursor position)

            // rigibbody.addforce()            
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            // apply input to body according to rules of movement (of your choosing)
            m_rigidBody.MovePosition(m_rigidBody.position + m_playerMouvement * (m_sprinting?m_sprint_velocity:m_walk_velocity) * dt);

            // temporary test
            /*if (m_envoieTest)
            {
                byte[] bytes = new byte[1];
                bytes[0] = 7;
                m_udpClient.Send(bytes);
                m_envoieTest = false;
            }*/

            // send pertinent data to server

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