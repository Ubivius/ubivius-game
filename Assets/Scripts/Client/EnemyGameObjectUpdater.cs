using UnityEngine;
using System.Collections;
using ubv.common.data;
using System.Collections.Generic;
using ubv.common;
using UnityEngine.Events;
using ubv.common.serialization;
using ubv.server.logic.ai;


// a faire, spawn serverside
// debug log
// ajoute au worldstate
// client :instance en checkant le iff entre le client et serveur
// 

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

        private int m_nbOfEnemy;
        public Dictionary<int, Rigidbody2D> Bodies { get; private set; }
        public Dictionary<int, EnemyStateMachine> EnemyStateMachine { get; private set; }

        private float m_timeSinceLastGoal;

        private Dictionary<int, EnemyStateData> m_goalStates;

        public UnityAction OnInitialized;

        public override void Init(WorldState clientState, int localID)
        {
            Debug.Log("BUUUULLLLLLLLLLLLLLLLLLLLLLLLLLLLLLSSSSSSSSSSSSSSSSSSSSSHHHHHHHHHHHITTT");

            int id = 0;
            /*foreach (EnemyStateData state in clientState.Enemies().Values)
            {
                id = state.GUID.Value;
                m_goalStates[id] = state;
            }*/

            m_goalStates = new Dictionary<int, EnemyStateData>();
            Bodies = new Dictionary<int, Rigidbody2D>();
            m_nbOfEnemy = 0;

        }

        public override bool NeedsCorrection(WorldState localState, WorldState remoteState)
        {
            bool err = false;
            // mettre un bool pour IsAlreadyCorrecting ?
            // check correction on goalStates au lieu du current position TODO
            foreach (EnemyStateData enemyStateData in remoteState.Enemies().Values)
            {
                err = ((enemyStateData.Position.Value - localState.Enemies()[enemyStateData.GUID.Value].Position.Value).sqrMagnitude > m_correctionTolerance * m_correctionTolerance) &&
                    (enemyStateData.NbOfEnemy.Value != localState.Enemies()[enemyStateData.GUID.Value].NbOfEnemy.Value);/*&&
                    (enemyStateData.EnemyState != localState.Enemies()[enemyStateData.GUID.Value].EnemyState)*/

                if (err)
                {
                    //Debug.Log("Needing correction");
                    return true;
                }
            }
            return err;
        }

        public override void UpdateStateFromWorld(ref WorldState state)
        {
            int id = 0;
            //mettre le need correcton 
            foreach (EnemyStateData enemy in state.Enemies().Values)
            {
                //if (enemy.NbOfEnemy.Value != m_nbOfEnemy)
                //{
                //    // va falloir gérer pour les enemies mort
                //    for (int i=0; i<(enemy.NbOfEnemy.Value); i++)
                //    {
                //        id = enemy.GUID.Value;
                //        GameObject enemyGameObject = GameObject.Instantiate(m_enemySettings.SimpleEnemy);
                //        Bodies[id] = enemyGameObject.GetComponent<Rigidbody2D>();
                //        Bodies[id].name = "Client enemy " + id.ToString();
                //        EnemyStateMachine[id] = enemyGameObject.GetComponent<EnemyStateMachine>();
                //    }

                //}

                enemy.Position.Value = m_goalStates[enemy.GUID.Value].Position.Value;
                enemy.Rotation.Value = m_goalStates[enemy.GUID.Value].Rotation.Value;
                //enemy.EnemyState = m_goalStates[enemy.GUID.Value].EnemyState;
            }
        }

        public override void Step(InputFrame input, float deltaTime)
        {
            m_timeSinceLastGoal += deltaTime;
            foreach (EnemyStateData enemy in m_goalStates.Values)
            {
                LerpTowardGoalState(enemy, m_timeSinceLastGoal);
            }
        }

        public override void UpdateWorldFromState(WorldState remoteState)
        {
            int id = 0;
            m_timeSinceLastGoal = 0;
            foreach (EnemyStateData enemy in remoteState.Enemies().Values)
            {
                Debug.Log("Instantiate enemy client");
                // va falloir gérer pour les enemies mort
                id = enemy.GUID.Value;
                GameObject enemyGameObject = GameObject.Instantiate(m_enemySettings.SimpleEnemy);
                Bodies[id] = enemyGameObject.GetComponent<Rigidbody2D>();
                Bodies[id].name = "Client enemy " + id.ToString();
                //EnemyStateMachine[id] = enemyGameObject.GetComponent<EnemyStateMachine>();

                m_goalStates[enemy.GUID.Value] = enemy;

                Bodies[enemy.GUID.Value].position = enemy.Position.Value;
                Bodies[enemy.GUID.Value].rotation = enemy.Rotation.Value;
                //EnemyStateMachine[enemy.GUID.Value].CurrentEnemyState = enemy.EnemyState;
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

            //if (EnemyStateMachine[enemy.GUID.Value].CurrentEnemyState != m_goalStates[enemy.GUID.Value].EnemyState)
            //{
            //    EnemyStateMachine[enemy.GUID.Value].CurrentEnemyState = m_goalStates[enemy.GUID.Value].EnemyState;
            //}
        }
    }
}
