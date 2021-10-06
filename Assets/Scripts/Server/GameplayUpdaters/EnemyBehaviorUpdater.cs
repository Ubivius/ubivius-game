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

        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, EnemyStateMachine> m_states;

        public override void Setup()
        {
            //instantier un seul ennemy pur le moment
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_states = new Dictionary<int, EnemyStateMachine>();
        }

        public override void InitClient(WorldState state)
        {
            m_pathfindingGridManager.OnPathFindingManagerGenerated += () => { OnPathFindingManagerGenerated(state); };
        }

        private void OnPathFindingManagerGenerated(WorldState state)
        {
            int id = state.EnemyGUID;
            GameObject enemyGameObject = GameObject.Instantiate(m_enemySettings.SimpleEnemy);
            Rigidbody2D body = enemyGameObject.GetComponent<Rigidbody2D>();
            EnemyPathFindingMovement enemyPathFindingMovement = enemyGameObject.GetComponent<EnemyPathFindingMovement>();
            EnemyStateMachine enemyStateMachine = enemyGameObject.GetComponent<EnemyStateMachine>();
            enemyPathFindingMovement.SetManager(m_pathfindingGridManager);
            body.position = enemyPathFindingMovement.GetPosition();
            body.name = "Server enemy " + id.ToString() + enemyStateMachine.CurrentEnemyState;

            m_states.Add(id, enemyStateMachine);
            m_bodies.Add(id, body);
        }


        public override void InitEnemy(EnemyStateData enemy)
        {
            enemy.Position.Value = m_bodies[enemy.GUID.Value].position;
        }

        public override void FixedUpdateFromClient(WorldState client, InputFrame frame, float deltaTime)
        {
            Rigidbody2D body = m_bodies[client.PlayerGUID];
        }

        public override void UpdateClient(ref WorldState client)
        {//state change et deplacement

            Rigidbody2D body = m_bodies[client.EnemyGUID];
            EnemyStateMachine enemyStateMachine = m_states[client.EnemyGUID];
            EnemyStateData enemy = client.GetEnemy();
            enemy.Position.Value = body.position;
            enemy.Rotation.Value = body.rotation;
            //Aide Murphy serializxation
            enemy.EnemyState = enemyStateMachine.CurrentEnemyState;
        }
    }
}
