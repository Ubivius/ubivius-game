using UnityEngine;
using System.Collections;
using ubv.common.data;
using System.Collections.Generic;
using ubv.common;
using UnityEngine.Events;
using ubv.common.serialization;
using ubv.server.logic.ai;
using ubv.server.logic;

namespace ubv.client.logic
{
    /// <summary>
    /// Instantiate enemies and moves them according to their enemy states
    /// </summary>
    public class EnemyGameObjectUpdater : ClientStateUpdater
    {
        [SerializeField] private float m_correctionTolerance = 0.3f;
        [SerializeField] private EnemySettings m_enemySettings;
        
        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, EnemyState> m_enemies;
        
        public override void Init(WorldState clientState, int localID)
        {
            m_enemies = new Dictionary<int, EnemyState>();
            m_bodies = new Dictionary<int, Rigidbody2D>();
        }

        public override bool IsPredictionWrong(WorldState localState, WorldState remoteState)
        {
            return false;
        }

        public override void SaveSimulationInState(ref WorldState state)
        {
            foreach(int id in m_enemies.Keys)
            {
                if (!state.Enemies().ContainsKey(id))
                {
                    state.AddEnemy(new EnemyState(m_enemies[id]));
                }
            }

            List<int> toRemove = new List<int>();
            foreach (EnemyState enemy in state.Enemies().Values)
            {
                enemy.Position.Value = m_bodies[enemy.GUID.Value].position;

                if(!m_enemies.ContainsKey(enemy.GUID.Value))
                {
                    toRemove.Add(enemy.GUID.Value);
                }
            }
            
            foreach(int id in toRemove)
            {
                state.Enemies().Remove(id);
            }
        }

        public override void Step(InputFrame input, float deltaTime)
        {
            foreach (EnemyState enemy in m_enemies.Values)
            {
                if((m_bodies[enemy.GUID.Value].position - enemy.Position.Value).sqrMagnitude > m_enemySettings.Velocity * m_enemySettings.Velocity)
                {
                    m_bodies[enemy.GUID.Value].position = enemy.Position.Value;
                }

                common.logic.EnemyMovement.Execute(m_bodies[enemy.GUID.Value], enemy.Position.Value, m_enemySettings.Velocity);
            }
        }

        public override void ResetSimulationToState(WorldState remoteState)
        {
        }

        public override void FixedStateUpdate(float deltaTime)
        {
        }

        public override void UpdateSimulationFromState(WorldState localState, WorldState remoteState)
        {
            foreach (EnemyState enemy in remoteState.Enemies().Values)
            {
                if (!m_bodies.ContainsKey(enemy.GUID.Value))
                {
                    GameObject enemyGameObject = GameObject.Instantiate(m_enemySettings.EnemyPrefab, new Vector3(0, 0, 0), Quaternion.identity);

                    m_bodies[enemy.GUID.Value] = enemyGameObject.GetComponent<Rigidbody2D>();
                    m_bodies[enemy.GUID.Value].name = "Client enemy " + enemy.GUID.Value.ToString();
                }
                m_enemies[enemy.GUID.Value] = enemy;
            }
            // Destroy enemies that do not exist in server
            List<int> destroyElements = new List<int>();
            foreach (int enemyKey in m_bodies.Keys)
            {
                if (!remoteState.Enemies().ContainsKey(enemyKey))
                {
                    destroyElements.Add(enemyKey);
                }
            }

            foreach (int enemyKey in destroyElements)
            {
                Destroy(m_bodies[enemyKey]);
                m_bodies.Remove(enemyKey);
                m_enemies.Remove(enemyKey);
            }
        }

        public override void DisableSimulation()
        {
            foreach (Rigidbody2D body in m_bodies.Values)
            {
                body.simulated = false;
            }
        }

        public override void EnableSimulation()
        {
            foreach (Rigidbody2D body in m_bodies.Values)
            {
                body.simulated = true;
            }
        }
    }
}
