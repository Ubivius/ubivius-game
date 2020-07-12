using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UBV
{
    public class InputController : MonoBehaviour
    {
        [Header("Movement parameters")]
        [SerializeField] private float m_velocity;
        [SerializeField] private float m_acceleration;
        [SerializeField] private UDPClient m_udpClient;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // collect input (calls to Input.GetXYZ.(...))
            // compute if needed (ex: trigo with cursor position)

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

            // send pertinent data to server

            
        }
    }
}