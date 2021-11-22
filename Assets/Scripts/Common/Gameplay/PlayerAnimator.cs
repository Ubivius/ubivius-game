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
        private Vector2 m_lastXVelocity;

        void Start() {
            m_animator = GetComponent<Animator>();
        }

        public void SetSprinting(bool isSprinting) {
            m_animator.SetBool("IsSprinting", isSprinting);
        }

        void Update() {
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

       public void Kill()
        {
            m_animator.SetBool("IsDead", true);
        }

       public void Revive()
       {
           m_animator.SetBool("IsDead", false);
       }

        public void Damage() {
            m_animator.SetTrigger("Damage");
        }

        public void Attack() {
            m_animator.SetTrigger("Attack");
        }

        public void UpdateAimingDirection(Vector2 a_shootingDirection) {
            m_animator.SetFloat("Aiming X", a_shootingDirection.x);
            m_animator.SetFloat("Aiming Y", a_shootingDirection.y);
        }
    }
}



