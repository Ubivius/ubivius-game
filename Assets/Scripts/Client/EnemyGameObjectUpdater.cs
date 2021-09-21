using UnityEngine;
using System.Collections;
using ubv.common.data;
using System.Collections.Generic;
using ubv.common;

namespace ubv.client.logic
{
    /// <summary>
    /// Instantiate players and moves them according to their player states
    /// </summary>
    public class EnemyGameObjectUpdater : ClientStateUpdater
    {
        [SerializeField] private EnemySettings m_enemySettings;

        private Dictionary<int, Rigidbody2D> m_bodies;
        private Rigidbody2D m_localEnemyBody;

        private int m_enemyGUID;

        private Dictionary<int, EnemyStateData> m_goalStates;

        public override void Init(List<PlayerState> playerStates, int localID)
        {
        }

        public override bool NeedsCorrection(ClientState localState, ClientState remoteState)
        {
            bool err = false;
            foreach (EnemyStateData enemy in remoteState.Enemies().Values)
            {
                err = (enemy.Position.Value - localState.Enemies()[enemy.GUID.Value].Position.Value).sqrMagnitude > 0.01f;
                if (err)
                {
                    return true;
                }
            }
            return err;
        }

        public override void UpdateStateFromWorld(ref ClientState state)
        {
        }

        public override void Step(InputFrame input, float deltaTime)
        {
        }

        public override void UpdateWorldFromState(ClientState state)
        {
        }

        //public override void SetStateAndStep(ref ClientState state, InputFrame input, float deltaTime)
        //{
        //    foreach (EnemyStateData enemy in state.Enemies().Values)
        //    {
        //        enemy.Position.Value = m_bodies[enemy.GUID.Value].position;
        //        enemy.Rotation.Value = m_bodies[enemy.GUID.Value].rotation;
        //    }
        //}

        //public override void UpdateFromState(ClientState state)
        //{
        //    foreach (EnemyStateData enemy in state.Enemies().Values)
        //    {
        //        if (enemy.GUID.Value != m_enemyGUID)
        //        {
        //            m_goalStates[enemy.GUID.Value] = enemy;
        //        }
        //        else
        //        {
        //            m_bodies[enemy.GUID.Value].position = enemy.Position.Value;
        //            m_bodies[enemy.GUID.Value].rotation = enemy.Rotation.Value;
        //        }
        //    }
        //}

        public override void FixedStateUpdate(float deltaTime)
        {
            foreach (EnemyStateData enemy in m_goalStates.Values)
            {
                if (enemy.GUID.Value != m_enemyGUID)
                {
                    m_bodies[enemy.GUID.Value].position = Vector2.LerpUnclamped(m_bodies[enemy.GUID.Value].position, m_goalStates[enemy.GUID.Value].Position.Value, 0.25f);
                    if ((m_bodies[enemy.GUID.Value].position - m_goalStates[enemy.GUID.Value].Position.Value).sqrMagnitude < 0.01f)
                    {
                        m_bodies[enemy.GUID.Value].position = m_goalStates[enemy.GUID.Value].Position.Value;
                    }

                    m_bodies[enemy.GUID.Value].rotation = enemy.Rotation.Value;
                }
            }
        }
    }
}
