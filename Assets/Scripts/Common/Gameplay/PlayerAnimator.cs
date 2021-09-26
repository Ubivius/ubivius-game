using System.Collections;
using System.Collections.Generic;
using ubv.client;
using UnityEngine;
using UnityEngine.Events;

namespace ubv.client.logic
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D m_body;
        private Animator m_animator;
        private bool m_isSprinting;
        private Vector2 m_lastXVelocity;

        void Start() {
            m_animator = GetComponent<Animator>();
            //m_body = GetComponent<Rigidbody2D>();
        }

        public void SetSprinting(bool isSprinting) {
            m_isSprinting = isSprinting;
            m_animator.SetBool("IsSprinting", isSprinting);
            //Debug.Log("The player " + m_body.transform.parent.name + " sprinting is " + isSprinting + ".");
        }

        void Update() {
            Debug.Log("The player " + m_body.transform.name + " values are " + m_body.velocity.magnitude + ", " + m_body.velocity.x + ", " + m_body.velocity.y + "... Sprinting is: " + m_isSprinting);
            m_animator.SetFloat("Speed", m_body.velocity.magnitude);
            m_animator.SetFloat("Horizontal", m_body.velocity.x);
            m_animator.SetFloat("Vertical", m_body.velocity.y);

            if (m_body.velocity != m_lastXVelocity && m_body.velocity.magnitude > 0) {
                m_lastXVelocity = m_body.velocity;
                if (m_lastXVelocity.x < 0) {
                    m_animator.SetFloat("Last Horizontal", m_lastXVelocity.x);
                    m_animator.SetFloat("Last Vertical", m_lastXVelocity.y);
                }
                if (m_lastXVelocity.x > 0) {
                    m_animator.SetFloat("Last Horizontal", m_lastXVelocity.x);
                    m_animator.SetFloat("Last Vertical", m_lastXVelocity.y);
                }
                    
            }
        }
    }
}



