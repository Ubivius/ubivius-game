using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ubv
{
    public class InputFrame : Serializable
    {
        public Serializable.Cachable<bool> Sprinting;
        public Serializable.Cachable<Vector2> Movement;
        public Serializable.Cachable<uint> Tick;
        
        protected override byte[] InternalToBytes() 
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
        }

        public void SetToNeutral()
        {
            Movement.Set(Vector2.zero);
            Sprinting.Set(false);
        }

        public InputFrame()
        {
            InitCachables();
            SetToNeutral();
        }

        private void InitCachables()
        {
            Sprinting = new Cachable<bool>(this);
            Movement = new Cachable<Vector2>(this);
            Tick = new Cachable<uint>(this);
        }

        public InputFrame(byte[] arr)
        {
            InitCachables();
            Debug.Assert(arr[0] == SerializationID());

            Sprinting.Set(arr[1] == 1);
            Movement.Set(new Vector2(System.BitConverter.ToSingle(arr, 2), System.BitConverter.ToSingle(arr, 4 + 1 + 1)));
            Tick.Set(System.BitConverter.ToUInt32(arr, 4 + 4 + 1 + 1));
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

        private Rigidbody2D m_rigidBody;

        private PlayerControls m_controls;
        
        private InputFrame m_currentInputFrame;
        
        private void Awake()
        {
            m_currentInputFrame = new InputFrame();
            m_rigidBody = GetComponent<Rigidbody2D>();

            m_controls = new PlayerControls();

            m_controls.Gameplay.Move.performed += context => m_currentInputFrame.Movement.Set(context.ReadValue<Vector2>());
            m_controls.Gameplay.Move.canceled += context => m_currentInputFrame.Movement.Set(Vector2.zero);

            m_controls.Gameplay.Sprint.performed += context => m_currentInputFrame.Sprinting.Set(true);
            m_controls.Gameplay.Sprint.canceled += context => m_currentInputFrame.Sprinting.Set(false);
            
            ClientState.RegisterUpdater(this);
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