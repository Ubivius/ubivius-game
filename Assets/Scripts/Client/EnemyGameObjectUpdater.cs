using UnityEngine;
using System.Collections;
using ubv.common.data;
using System.Collections.Generic;
using ubv.common;
using UnityEngine.Events;
using ubv.common.serialization;

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
        private Rigidbody2D m_localEnemyBody;

        private int m_enemyGUID;
        public Dictionary<int, Rigidbody2D> Bodies { get; private set; }
        private float m_timeSinceLastGoal;

        private Dictionary<int, EnemyStateData> m_goalStates;

        public UnityAction OnInitialized;

        public override void Init(List<Serializable> enemyStateData, int localID)
        {
            m_timeSinceLastGoal = 0;
            Bodies = new Dictionary<int, Rigidbody2D>();
            m_goalStates = new Dictionary<int, EnemyStateData>();
            int id = 0;
            foreach (EnemyStateData state in enemyStateData)
            {
                id = state.GUID.Value;
                GameObject playerGameObject = GameObject.Instantiate(m_enemySettings.SimpleEnemy);
                Bodies[id] = playerGameObject.GetComponent<Rigidbody2D>();
                Bodies[id].name = "Client enemy " + id.ToString();

                if (id != localID)
                {
                    Bodies[id].bodyType = RigidbodyType2D.Kinematic;
                }

                m_goalStates[id] = state;
            }

            m_enemyGUID = localID;
            m_localEnemyBody = Bodies[localID];
            OnInitialized?.Invoke();
        }

        public override bool NeedsCorrection(ClientState localState, ClientState remoteState)
        {
            bool err = false;
            // mettre un bool pour IsAlreadyCorrecting ?
            // check correction on goalStates au lieu du current position TODO
            foreach (EnemyStateData enemyStateData in remoteState.Enemies().Values)
            {
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
            //FUTUR iMPLEMENTATION
            //m_timeSinceLastGoal += deltaTime;
            //foreach (EnemyStateData enemy in m_goalStates.Values)
            //{
            //    if (enemy.GUID.Value != m_enemyGUID)
            //    {
            //        LerpTowardGoalState(enemy, m_timeSinceLastGoal);
            //    }
            //}
            //common.logic.PlayerMovement.Execute(ref m_localPlayerBody, PlayerControllers[m_playerGUID].GetStats(), input, deltaTime);
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

        //private void LerpTowardGoalState(PlayerState player, float time)
        //{
        //    Bodies[player.GUID.Value].position = Vector2.Lerp(Bodies[player.GUID.Value].position, m_goalStates[player.GUID.Value].Position.Value, time / m_lerpTime);
        //    if ((Bodies[player.GUID.Value].position - m_goalStates[player.GUID.Value].Position.Value).sqrMagnitude < 0.01f)
        //    {
        //        Bodies[player.GUID.Value].position = m_goalStates[player.GUID.Value].Position.Value;
        //    }

        //    Bodies[player.GUID.Value].rotation = Mathf.Lerp(Bodies[player.GUID.Value].rotation, m_goalStates[player.GUID.Value].Rotation.Value, time / m_lerpTime);
        //    if (Bodies[player.GUID.Value].rotation - m_goalStates[player.GUID.Value].Rotation.Value < 0.01f)
        //    {
        //        Bodies[player.GUID.Value].rotation = m_goalStates[player.GUID.Value].Rotation.Value;
        //    }
        //}
    }
}
