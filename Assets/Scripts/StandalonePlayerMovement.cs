﻿using System.Collections;
using System.Collections.Generic;
using ubv.client.logic;
using ubv.common.gameplay;
using ubv.common.logic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ubv.client
{
    public class StandalonePlayerMovement : MonoBehaviour
    {
        [SerializeField] private GameObject m_player;

        private Rigidbody2D m_body;

        [SerializeField] private PlayerController m_playerController;

        [SerializeField] private string m_scene = "proto_art";
        private PlayerAnimator m_playerAnimator;

        private PhysicsScene2D m_physics;

        private bool m_isSprinting;

        private UnityAction<bool> m_sprintAction;


        private void Awake() {
            m_body = m_player.GetComponent<Rigidbody2D>();
            m_playerAnimator = m_player.GetComponent<PlayerAnimator>();

            m_physics = UnityEngine.SceneManagement.SceneManager.GetSceneByName(m_scene).GetPhysicsScene2D();
            m_sprintAction += m_playerAnimator.SetSprinting;
        }
        
        private void FixedUpdate()
        {
            if (InputController.CurrentFrame().Sprinting.Value != m_isSprinting) {
                m_isSprinting = InputController.CurrentFrame().Sprinting.Value;
                m_sprintAction.Invoke(m_isSprinting);
            }

            Vector2 velocity = PlayerMovement.GetVelocity(InputController.CurrentFrame().Movement.Value,
                InputController.CurrentFrame().Sprinting.Value,
                m_playerController.GetStats());
            PlayerMovement.Execute(ref m_body, velocity);
            m_physics.Simulate(Time.fixedDeltaTime);
        }

        public void Kill()
        {
            m_playerAnimator.Kill();
        }

        public void Revive()
        {
            m_playerAnimator.Revive();
        }

        public void Damage()
        {
            m_playerAnimator.Damage();
        }

        public void Attack()
        {
            m_playerAnimator.Attack();
        }

    }
}
