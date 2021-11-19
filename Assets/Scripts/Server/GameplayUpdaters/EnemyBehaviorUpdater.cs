using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.data;
using System.Collections.Generic;
using UnityEngine.Events;
using ubv.server.logic.ai;
using ubv.common.world;

namespace ubv.server.logic
{
    public class EnemyBehaviorUpdater : ServerGameplayStateUpdater
    {
        [SerializeField] private PlayerMovementUpdater m_playerMovementUpdater;
        [SerializeField] private WorldGenerator m_worldGenerator;
        [SerializeField] private EnemySettings m_enemySettings;
        [SerializeField] private PathfindingGridManager m_pathfindingGridManager;
        [SerializeField] private int m_enemyCount;

        private PathNode[,] m_mapNodes;

        private Dictionary<int, EnemyState> m_enemies;

        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, EnemyMovementUpdater> m_enemyMovementUpdaters;

        public override void Setup()
        {
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_enemies = new Dictionary<int, EnemyState>();
            m_enemyMovementUpdaters = new Dictionary<int, EnemyMovementUpdater>();
        }

        public override void InitWorld(WorldState state)
        {
            m_mapNodes = m_pathfindingGridManager.GetPathNodeArray();
            EnemySpawn(state);
        }
        
        public override void FixedUpdateFromClient(WorldState client, Dictionary<int, InputFrame> frames, float deltaTime)
        {
            foreach(int id in m_enemies.Keys)
            {
                EnemyState enemy = m_enemies[id];
                Rigidbody2D body = m_bodies[id];
                
                common.logic.EnemyMovement.Execute(body, m_enemyMovementUpdaters[id].GetNextPosition(), m_enemySettings.Velocity);
            }
        }

        public override void UpdateWorld(WorldState client)
        {
            foreach (int id in client.Enemies().Keys)
            {
                Rigidbody2D body = m_bodies[id];
                EnemyState enemy = m_enemies[id];

                enemy.Position.Value = m_enemyMovementUpdaters[id].GetPosition();
            }
        }

        private void EnemySpawn(WorldState state)
        {
            int i = 0;
            while (i < m_enemyCount)
            {
                GameObject enemyGameObject = Instantiate(m_enemySettings.EnemyPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                Rigidbody2D body = enemyGameObject.GetComponent<Rigidbody2D>();
                EnemyMovementUpdater enemyPathFindingMovement = enemyGameObject.GetComponent<EnemyMovementUpdater>();
                enemyPathFindingMovement.SetPathfinding(m_pathfindingGridManager);

                EnemyStateMachine stateMachine = enemyGameObject.GetComponent<EnemyStateMachine>();
                stateMachine.Init(m_playerMovementUpdater, m_worldGenerator, m_pathfindingGridManager);

                int id = System.Guid.NewGuid().GetHashCode();
                body.name = "Server enemy " + id.ToString();

                m_bodies.Add(id, body);
                m_enemyMovementUpdaters.Add(id, enemyPathFindingMovement);

                EnemyState enemyStateData = new EnemyState(id);
                enemyStateData.Position.Value = m_enemyMovementUpdaters[id].GetNextPosition();

                m_enemies.Add(id, enemyStateData);
                state.AddEnemy(enemyStateData);
                ++i;
            }
        }
    }
}
