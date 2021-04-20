using System.Collections;
using System.Collections.Generic;
using ubv.client;
using UnityEngine;
using UnityEngine.Events;

namespace ubv.client.logic
{
    public class PlayerAnimator : MonoBehaviour
    {  
        [SerializeField] public PlayerGameObjectUpdater m_playerGameObjectUpdater;

        // To test the animation without the whole game and server setup
        [SerializeField] public GameObject m_player;
        [SerializeField] public bool m_standAlone;

        private Rigidbody2D m_body;
        private Animator m_animator;

        void Start() {
            if (m_standAlone) {
                m_animator = m_player.GetComponent<Animator>();
                m_body = m_player.GetComponent<Rigidbody2D>();
            }
        }

        void Update() {
            if (m_standAlone) {
                m_animator.SetFloat("Speed", m_body.velocity.magnitude);
                m_animator.SetFloat("Horizontal", m_body.velocity.x);
                m_animator.SetFloat("Vertical", m_body.velocity.y);
            }
            else {
                // TODO: Change the Bodies dict to the players dict
                foreach (var body in m_playerGameObjectUpdater.Bodies) {
                    // TODO: Update this line to get the animator from the player object and
                    //       not from the body
                    m_animator = body.Value.gameObject.GetComponent<Animator>();

                    m_animator.SetFloat("Speed", body.Value.velocity.magnitude);
                    m_animator.SetFloat("Horizontal", body.Value.velocity.x);
                    m_animator.SetFloat("Vertical", body.Value.velocity.y);
                }
            }
        }
    }
}



