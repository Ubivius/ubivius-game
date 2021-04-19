using System.Collections;
using System.Collections.Generic;
using ubv.client;
using UnityEngine;
using UnityEngine.Events;

namespace ubv.client.logic
{
    public class PlayerAnimator : MonoBehaviour
    {   

        [SerializeField] public GameObject m_player;
        [SerializeField] public PlayerGameObjectUpdater m_playerGameObjectUpdater;
        [SerializeField] public bool m_standAlone;

        private Rigidbody2D m_body;
        private Animator m_animator;

        // Start is called before the first frame update
        void Start() {
            if (m_standAlone) {
                m_animator = m_player.GetComponent<Animator>();
                m_body = m_player.GetComponent<Rigidbody2D>();
            }
        }

        // Update is called once per frame
        void Update() {
            if (m_standAlone) {
                m_animator.SetFloat("Speed", m_body.velocity.magnitude);
                m_animator.SetFloat("Horizontal", m_body.velocity.x);
                m_animator.SetFloat("Vertical", m_body.velocity.y);
            }
            else {
                foreach (var body in m_playerGameObjectUpdater.Bodies) {
                    m_animator = body.Value.gameObject.GetComponent<Animator>();

                    m_animator.SetFloat("Speed", body.Value.velocity.magnitude);
                    m_animator.SetFloat("Horizontal", body.Value.velocity.x);
                    m_animator.SetFloat("Vertical", body.Value.velocity.y);

                }
            }
        }
    }
}



