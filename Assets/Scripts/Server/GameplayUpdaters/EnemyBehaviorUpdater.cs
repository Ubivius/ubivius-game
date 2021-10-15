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
        [SerializeField] private PathfindingGridManager m_pathfindingGridManager;
        [SerializeField] private int m_nbOfEnemy;

        private Dictionary<int, EnemyStateData> m_enemyStatesData;

        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, EnemyState> m_states;

        public override void Setup()
        {
            //instantier un seul ennemy pur le moment
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_states = new Dictionary<int, EnemyState>();
        }

        public override void InitWorld(WorldState state)
        {
            Debug.Log("ENEMy INIT world");

            GameObject enemyGameObject = GameObject.Instantiate(m_enemySettings.SimpleEnemy);
            Rigidbody2D body = enemyGameObject.GetComponent<Rigidbody2D>();
            // EnemyPathFindingMovement enemyPathFindingMovement = enemyGameObject.GetComponent<EnemyPathFindingMovement>();
            //EnemyStateMachine enemyStateMachine = enemyGameObject.GetComponent<EnemyStateMachine>();
            //enemyPathFindingMovement.SetManager(m_pathfindingGridManager);

            int id = System.Guid.NewGuid().GetHashCode();
            body.position = new Vector2(10, 10); // enemyPathFindingMovement.GetPosition();
            body.name = "Server enemy " + id.ToString(); // + enemyStateMachine.CurrentEnemyState;

            //m_states.Add(id, enemyStateMachine.CurrentEnemyState);
            m_bodies.Add(id, body);

            EnemyStateData enemyStateData = new EnemyStateData(id);

            enemyStateData.Position.Value = m_bodies[id].position;

            m_enemyStatesData.Add(id, enemyStateData);
            //Je pense que le state est a null
            state.AddEnemy(enemyStateData);

            Debug.Log("GGGGGGGGGGGGGGGGGGGGGGGOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOODDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            //m_pathfindingGridManager.OnPathFindingManagerGenerated += () => { OnPathFindingManagerGenerated(state); };
        }

        private void OnPathFindingManagerGenerated(WorldState state)
        {
            Debug.Log("Pathfinding grid manager");
            foreach (int id in state.Enemies().Keys)
            {
                GameObject enemyGameObject = GameObject.Instantiate(m_enemySettings.SimpleEnemy);
                Rigidbody2D body = enemyGameObject.GetComponent<Rigidbody2D>();
                EnemyPathFindingMovement enemyPathFindingMovement = enemyGameObject.GetComponent<EnemyPathFindingMovement>();
                EnemyStateMachine enemyStateMachine = enemyGameObject.GetComponent<EnemyStateMachine>();
                enemyPathFindingMovement.SetManager(m_pathfindingGridManager);

                body.position = enemyPathFindingMovement.GetPosition();
                body.name = "Server enemy " + id.ToString() + enemyStateMachine.CurrentEnemyState;

                m_states.Add(id, enemyStateMachine.CurrentEnemyState);
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
            // movement
        }

        public override void UpdateWorld(WorldState client)
        {
            foreach (int id in client.Enemies().Keys)
            {
                Rigidbody2D body = m_bodies[id];
                EnemyStateData enemy = m_enemyStatesData[id];
                enemy.Position.Value = body.position;
                enemy.Rotation.Value = body.rotation;
                enemy.EnemyState = m_states[id];
            }
        }
    }
}
