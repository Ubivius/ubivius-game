using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UBV
{
    public class InputController : MonoBehaviour
    {
        [Header("Movement parameters")]
        [SerializeField] private float m_velocity;
        [SerializeField] private float m_walk_velocity;
        [SerializeField] private float m_sprint_velocity;
        [SerializeField] private bool m_sprinting;
        [SerializeField] private float m_acceleration;
        [SerializeField] private UDPClient m_udpClient;

        public Rigidbody2D rb_player;

        Vector2 player_mouvement;

        private void Awake()
        {
            m_sprinting = false;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // collect input (calls to Input.GetXYZ.(...))
            player_mouvement.x = Input.GetAxisRaw("Horizontal");
            player_mouvement.y = Input.GetAxisRaw("Vertical");

            m_sprinting = (Input.GetKey("left shift") || Input.GetAxis("Sprint") == 1) ? true : false;

            // compute if needed (ex: trigo with cursor position)


            // rigibbody.addforce()

            // temporary test
            if (Input.GetKeyDown(KeyCode.Return))
            {
                byte[] bytes = new byte[1];
                bytes[0] = 7;
                m_udpClient.Send(bytes);
            }
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            // apply input to body according to rules of movement (of your choosing)
            rb_player.MovePosition(rb_player.position + player_mouvement * (m_sprinting?m_sprint_velocity:m_walk_velocity) * dt);


            // send pertinent data to server

            
        }
    }
}