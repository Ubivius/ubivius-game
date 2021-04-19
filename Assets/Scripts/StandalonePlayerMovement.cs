using System.Collections;
using System.Collections.Generic;
using ubv.common.gameplay;
using ubv.common.logic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ubv.client
{
    public class StandalonePlayerMovement : MonoBehaviour
    {
        [SerializeField] Rigidbody2D m_body;
        [SerializeField] PlayerController m_player;

        [SerializeField] private string m_scene = "proto_art";

        private PhysicsScene2D m_physics;



        private void Awake()
        {
            m_physics = UnityEngine.SceneManagement.SceneManager.GetSceneByName(m_scene).GetPhysicsScene2D();
        }
        
        private void FixedUpdate()
        {
            PlayerMovement.Execute(ref m_body, m_player.GetStats(), InputController.CurrentFrame(), Time.fixedDeltaTime);
            m_physics.Simulate(Time.fixedDeltaTime);
        }
    }
}
