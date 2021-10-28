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
        [SerializeField] private PathfindingGridManager m_pathfindingGridManager;

        private float m_timeSinceLastGoal;

        public Dictionary<int, Rigidbody2D> Bodies { get; private set; }
        public Dictionary<int, EnemyState> EnemyState { get; private set; }
        public Dictionary<int, Vector2> GoalPosition { get; private set; }
        private Dictionary<int, EnemyPathFindingMovement> EnemyPathfindingMovement;

        private Dictionary<int, EnemyStateData> m_goalStates;

        public override void Init(WorldState clientState, int localID)
        {
            m_goalStates = new Dictionary<int, EnemyStateData>();
            Bodies = new Dictionary<int, Rigidbody2D>();
            EnemyState = new Dictionary<int, EnemyState>();
            EnemyPathfindingMovement = new Dictionary<int, EnemyPathFindingMovement>();
            GoalPosition = new Dictionary<int, Vector2>();
        }

        public override bool NeedsCorrection(WorldState localState, WorldState remoteState)
        {
            bool err = false;
            // mettre un bool pour IsAlreadyCorrecting ?
            // check correction on goalStates au lieu du current position TODO

            if (localState.Enemies().Count != remoteState.Enemies().Count)
            {
                return true;
            }

            foreach (EnemyStateData enemyStateData in remoteState.Enemies().Values)
            {
                err = ((enemyStateData.Position.Value - localState.Enemies()[enemyStateData.GUID.Value].Position.Value).sqrMagnitude > m_correctionTolerance * m_correctionTolerance)
                     && (enemyStateData.EnemyState != localState.Enemies()[enemyStateData.GUID.Value].EnemyState)
                     && (enemyStateData.GoalPosition.Value != localState.Enemies()[enemyStateData.GUID.Value].GoalPosition.Value);

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
            foreach (EnemyStateData enemy in state.Enemies().Values)
            {
                enemy.Position.Value = m_goalStates[enemy.GUID.Value].Position.Value;
                enemy.Rotation.Value = m_goalStates[enemy.GUID.Value].Rotation.Value;
                enemy.EnemyState = m_goalStates[enemy.GUID.Value].EnemyState;
                enemy.GoalPosition = m_goalStates[enemy.GUID.Value].GoalPosition;
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
            m_timeSinceLastGoal = 0;

            Debug.Log("nb of enemy in remote:"+remoteState.Enemies().Count.ToString());
            foreach (EnemyStateData enemy in remoteState.Enemies().Values)
            {
                if (!Bodies.ContainsKey(enemy.GUID.Value))
                {
                    Debug.Log("Instantiate enemy client");
                    GameObject enemyGameObject = GameObject.Instantiate(m_enemySettings.SimpleEnemy, new Vector3(0, 0, 0), Quaternion.identity);
                    EnemyPathfindingMovement[enemy.GUID.Value] = enemyGameObject.GetComponent<EnemyPathFindingMovement>();
                    EnemyPathfindingMovement[enemy.GUID.Value].SetManager(m_pathfindingGridManager);

                    Bodies[enemy.GUID.Value] = enemyGameObject.GetComponent<Rigidbody2D>();
                    Bodies[enemy.GUID.Value].name = "Client enemy " + enemy.GUID.Value.ToString();
                }

                Debug.Log("INputtt Outputtttttt position avant changement" + enemy.Position.Value);
                Debug.Log("enemy Positionnnnnnnnnnnnnnnnnnnnnnn" + enemy.Position.Value);

                Bodies[enemy.GUID.Value].position = enemy.Position.Value;
                Bodies[enemy.GUID.Value].rotation = enemy.Rotation.Value;

                GoalPosition[enemy.GUID.Value] = enemy.GoalPosition.Value;
                if (GoalPosition[enemy.GUID.Value] != null)
                {
                    EnemyPathfindingMovement[enemy.GUID.Value].MoveTo(GoalPosition[enemy.GUID.Value]);
                }

                EnemyState[enemy.GUID.Value] = enemy.EnemyState;

                m_goalStates[enemy.GUID.Value] = enemy;
            }

            //Destroy enemy
            List<int> destroyElements = new List<int>();
            foreach (int enemyKey in Bodies.Keys)
            {
                if(!remoteState.Enemies().ContainsKey(enemyKey))
                {
                    destroyElements.Add(enemyKey);
                }
            }

            foreach(int enemyKey in destroyElements)
            {
                Destroy(Bodies[enemyKey]);
                Bodies.Remove(enemyKey);
                EnemyState.Remove(enemyKey);
                EnemyPathfindingMovement.Remove(enemyKey);
                GoalPosition.Remove(enemyKey);
                m_goalStates.Remove(enemyKey);
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

            if (EnemyState[enemy.GUID.Value] != m_goalStates[enemy.GUID.Value].EnemyState)
            {
                EnemyState[enemy.GUID.Value] = m_goalStates[enemy.GUID.Value].EnemyState;
            }

            if (GoalPosition[enemy.GUID.Value] != m_goalStates[enemy.GUID.Value].GoalPosition.Value)
            {
                GoalPosition[enemy.GUID.Value] = m_goalStates[enemy.GUID.Value].GoalPosition.Value;
            }
        }
    }
}
