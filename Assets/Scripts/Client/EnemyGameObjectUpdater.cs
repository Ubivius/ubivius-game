﻿using UnityEngine;
using System.Collections;
using ubv.common.data;
using System.Collections.Generic;
using ubv.common;
using UnityEngine.Events;
using ubv.common.serialization;
using ubv.server.logic.ai;

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

        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, EnemyState> m_states;
        private Rigidbody2D m_localEnemyBody;

        private int m_enemyGUID;
        public Dictionary<int, Rigidbody2D> Bodies { get; private set; }
        private float m_timeSinceLastGoal;

        private Dictionary<int, EnemyStateData> m_goalStates;

        public UnityAction OnInitialized;

        public override void Init(ClientState clientState, int localID)
        {
            //dans le need correction corriger le nombre d,ennemie par le serveur
            //ils vont etre spawné la
            //m_timeSinceLastGoal = 0;
            //Bodies = new Dictionary<int, Rigidbody2D>();
            //m_goalStates = new Dictionary<int, EnemyStateData>();
            //int id = 0;
            //foreach (EnemyStateData state in clientState.Enemies().Values)
            //{
            //    id = state.GUID.Value;
            //    GameObject playerGameObject = GameObject.Instantiate(m_enemySettings.SimpleEnemy);
            //    Bodies[id] = playerGameObject.GetComponent<Rigidbody2D>();
            //    Bodies[id].name = "Client enemy " + id.ToString();

            //    if (id != localID)
            //    {
            //        Bodies[id].bodyType = RigidbodyType2D.Kinematic;
            //    }

            //    m_goalStates[id] = state;
            //}

            //m_enemyGUID = localID;
            //m_localEnemyBody = Bodies[localID];
            //OnInitialized?.Invoke();
        }

        public override bool NeedsCorrection(ClientState localState, ClientState remoteState)
        {
            bool err = false;
            Bodies = new Dictionary<int, Rigidbody2D>();
            m_goalStates = new Dictionary<int, EnemyStateData>();
            int id = 0;
            // mettre un bool pour IsAlreadyCorrecting ?
            // check correction on goalStates au lieu du current position TODO
            foreach (EnemyStateData enemyStateData in remoteState.Enemies().Values)
            {
                id = enemyStateData.GUID.Value;
                GameObject enemyGameObject = GameObject.Instantiate(m_enemySettings.SimpleEnemy);
                Bodies[id] = enemyGameObject.GetComponent<Rigidbody2D>();
                Bodies[id].name = "Client enemy " + id.ToString();

                m_goalStates[id] = enemyStateData;
                m_enemyGUID = id;

                OnInitialized?.Invoke();

                err = (enemyStateData.Position.Value - localState.Enemies()[enemyStateData.GUID.Value].Position.Value).sqrMagnitude > m_correctionTolerance * m_correctionTolerance;
                if (err)
                {
                    //Debug.Log("Needing correction");
                    return true;
                }
            }
            return err;
        }

        public override void UpdateStateFromWorld(ref ClientState state)
        {
            foreach (EnemyStateData enemy in state.Enemies().Values)
            {
                if (enemy.GUID.Value != m_enemyGUID)
                {
                    enemy.Position.Value = m_goalStates[enemy.GUID.Value].Position.Value;
                    enemy.Rotation.Value = m_goalStates[enemy.GUID.Value].Rotation.Value;
                }
                else
                {
                    enemy.Position.Value = Bodies[enemy.GUID.Value].position;
                    enemy.Rotation.Value = Bodies[enemy.GUID.Value].rotation;
                }
            }
        }

        public override void Step(InputFrame input, float deltaTime)
        {
            m_timeSinceLastGoal += deltaTime;
            foreach (EnemyStateData enemy in m_goalStates.Values)
            {
                if (enemy.GUID.Value != m_enemyGUID)
                {
                    LerpTowardGoalState(enemy, m_timeSinceLastGoal);
                }
            }
        }

        public override void UpdateWorldFromState(ClientState state)
        {
            m_timeSinceLastGoal = 0;
            foreach (EnemyStateData enemy in state.Enemies().Values)
            {
                m_goalStates[enemy.GUID.Value] = enemy;

                if (enemy.GUID.Value == m_enemyGUID)
                {
                    Bodies[enemy.GUID.Value].position = enemy.Position.Value;
                    Bodies[enemy.GUID.Value].rotation = enemy.Rotation.Value;
                }
            }
        }

        public override void FixedStateUpdate(float deltaTime)
        {
        }

        private void LerpTowardGoalState(EnemyStateData enemy, float time)
        {
            Bodies[enemy.GUID.Value].position = Vector2.Lerp(Bodies[enemy.GUID.Value].position, m_goalStates[enemy.GUID.Value].Position.Value, time / m_lerpTime);
            if ((Bodies[enemy.GUID.Value].position - m_goalStates[enemy.GUID.Value].Position.Value).sqrMagnitude < 0.01f)
            {
                Bodies[enemy.GUID.Value].position = m_goalStates[enemy.GUID.Value].Position.Value;
            }

            Bodies[enemy.GUID.Value].rotation = Mathf.Lerp(Bodies[enemy.GUID.Value].rotation, m_goalStates[enemy.GUID.Value].Rotation.Value, time / m_lerpTime);
            if (Bodies[enemy.GUID.Value].rotation - m_goalStates[enemy.GUID.Value].Rotation.Value < 0.01f)
            {
                Bodies[enemy.GUID.Value].rotation = m_goalStates[enemy.GUID.Value].Rotation.Value;
            }
        }
    }
}
