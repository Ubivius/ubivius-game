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
        [SerializeField] private float m_lerpTime = 0.2f;
        [SerializeField] private float m_correctionTolerance = 0.01f;
        [SerializeField] private EnemySettings m_enemySettings;

        private float m_timeSinceLastGoal;

        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, EnemyState> m_goalStates;

        public override void Init(WorldState clientState, int localID)
        {
            m_goalStates = new Dictionary<int, EnemyState>();
            m_bodies = new Dictionary<int, Rigidbody2D>();
        }

        public override bool NeedsCorrection(WorldState localState, WorldState remoteState)
        {
            if (localState.Enemies().Count != remoteState.Enemies().Count)
            {
                return true;
            }
            
            foreach (EnemyState enemy in remoteState.Enemies().Values)
            {
                bool err = enemy.IsDifferent(localState.Enemies()[enemy.GUID.Value]);
                if (err)
                {
                    return true;
                }
            }
            return false;
        }

        public override void UpdateStateFromWorld(ref WorldState state)
        {
            foreach (EnemyState enemy in state.Enemies().Values)
            {
                enemy.Position.Value = m_bodies[enemy.GUID.Value].position;
                enemy.GoalPosition = m_goalStates[enemy.GUID.Value].GoalPosition;
            }
        }

        public override void Step(InputFrame input, float deltaTime)
        {
            m_timeSinceLastGoal += deltaTime;
            foreach (EnemyState enemy in m_goalStates.Values)
            {
                common.logic.EnemyMovement.Execute(m_bodies[enemy.GUID.Value], m_goalStates[enemy.GUID.Value].GoalPosition.Value, m_enemySettings.Velocity);
            }
        }

        public override void UpdateWorldFromState(WorldState remoteState)
        {
            m_timeSinceLastGoal = 0;

            foreach (EnemyState enemy in remoteState.Enemies().Values)
            {
                if (!m_bodies.ContainsKey(enemy.GUID.Value))
                {
                    GameObject enemyGameObject = GameObject.Instantiate(m_enemySettings.EnemyPrefab, new Vector3(0, 0, 0), Quaternion.identity);

                    m_bodies[enemy.GUID.Value] = enemyGameObject.GetComponent<Rigidbody2D>();
                    m_bodies[enemy.GUID.Value].name = "Client enemy " + enemy.GUID.Value.ToString();
                }
                
                if ((m_bodies[enemy.GUID.Value].position - enemy.Position.Value).sqrMagnitude > m_correctionTolerance * m_correctionTolerance)
                {
                    m_bodies[enemy.GUID.Value].position = enemy.Position.Value;
                }
                else
                {
                    // lerp ?
                }
                 
                m_goalStates[enemy.GUID.Value] = enemy;
            }

            // Destroy enemies that do not exist in server
            List<int> destroyElements = new List<int>();
            foreach (int enemyKey in m_bodies.Keys)
            {
                if(!remoteState.Enemies().ContainsKey(enemyKey))
                {
                    destroyElements.Add(enemyKey);
                }
            }

            foreach(int enemyKey in destroyElements)
            {
                Destroy(m_bodies[enemyKey]);
                m_bodies.Remove(enemyKey);
                m_goalStates.Remove(enemyKey);
            }
        }

        public override void FixedStateUpdate(float deltaTime)
        {
        }
    }
}
