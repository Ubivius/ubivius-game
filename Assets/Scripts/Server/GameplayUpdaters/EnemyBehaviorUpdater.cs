using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.data;
using System.Collections.Generic;
using UnityEngine.Events;
using ubv.server.logic.ai;

namespace ubv.server.logic
{
    public class EnemyBehaviorUpdater : ServerGameplayStateUpdater
    {
        [SerializeField] private EnemySettings m_enemySettings;
        [SerializeField] private GameMaster m_gameMaster;
        [SerializeField] private PathfindingGridManager m_pathfindingGridManager;

        private Dictionary<int, EnemyStateData> m_enemyStatesData;

        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, EnemyStateMachine> m_states;

        public override void Setup()
        {
            //instantier un seul ennemy pur le moment
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_states = new Dictionary<int, EnemyStateMachine>();
        }

        public override void InitWorld(WorldState state)
        {
            m_pathfindingGridManager.OnPathFindingManagerGenerated += () => { OnPathFindingManagerGenerated(state); };
        }

        private void OnPathFindingManagerGenerated(WorldState state)
        {
            foreach (int id in state.Players().Keys)
            {
                GameObject enemyGameObject = GameObject.Instantiate(m_enemySettings.SimpleEnemy);
                Rigidbody2D body = enemyGameObject.GetComponent<Rigidbody2D>();
                EnemyPathFindingMovement enemyPathFindingMovement = enemyGameObject.GetComponent<EnemyPathFindingMovement>();
                EnemyStateMachine enemyStateMachine = enemyGameObject.GetComponent<EnemyStateMachine>();
                enemyPathFindingMovement.SetManager(m_pathfindingGridManager);

                body.position = enemyPathFindingMovement.GetPosition();
                body.name = "Server enemy " + id.ToString() + enemyStateMachine.CurrentEnemyState;

                m_states.Add(id, enemyStateMachine);
                m_bodies.Add(id, body);

                EnemyStateData enemyStateData = new EnemyStateData(id);

                enemyStateData.Position.Value = m_bodies[id].position;

                m_enemyStatesData.Add(id, enemyStateData);
            }

            foreach (EnemyStateData enemy in m_enemyStatesData.Values)
            {
                state.AddEnemy(enemy);
            }
        }

        public override void FixedUpdateFromClient(WorldState client, Dictionary<int, InputFrame> frames, float deltaTime)
        {
        }

        public override void UpdateWorld(WorldState client)
        {
            foreach (int id in client.Enemies().Keys)
            {
                Rigidbody2D body = m_bodies[id];
                EnemyStateData enemy = m_enemyStatesData[id];
                EnemyStateMachine enemyStateMachine = m_states[id];
                enemy.Position.Value = body.position;
                enemy.Rotation.Value = body.rotation;
                enemy.EnemyState = enemyStateMachine.CurrentEnemyState;
            }
        }
    }
}
