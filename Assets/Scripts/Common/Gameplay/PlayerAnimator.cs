using System.Collections;
using System.Collections.Generic;
using ubv.client;
using UnityEngine;
using UnityEngine.Events;

namespace ubv.client.logic
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] public float testSpeed;
        [SerializeField] public Animator m_animator;
        [SerializeField] public StandalonePlayerMovement m_standalonePlayerController;
        [SerializeField] public PlayerGameObjectUpdater m_playerGameObjectUpdater;
        [SerializeField] public bool m_standAlone;


        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            if (m_standAlone) {
                m_animator.SetFloat("Speed", m_standalonePlayerController.GetSpeed());
                m_animator.SetFloat("Horizontal", m_standalonePlayerController.GetOrientationX());
                m_animator.SetFloat("Vertical", m_standalonePlayerController.GetOrientationY());
            }
            else {

            }
        }
    }
}



