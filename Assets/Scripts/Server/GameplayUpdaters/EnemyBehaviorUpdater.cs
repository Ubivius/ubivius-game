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
        private Dictionary<int, EnemyMainServer> m_enemyMain;

        public override void Setup()
        {
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_enemies = new Dictionary<int, EnemyState>();
            m_enemyMovementUpdaters = new Dictionary<int, EnemyMovementUpdater>();
            m_enemyMain = new Dictionary<int, EnemyMainServer>();
        }

        public override void InitWorld(WorldState state)
        {
            m_mapNodes = m_pathfindingGridManager.GetPathNodeArray();
            EnemySpawn(state);
        }
        
        public override void FixedUpdateFromClient(WorldState client, Dictionary<int, InputFrame> frames, float deltaTime)
        {
            foreach (int id in m_bodies.Keys)
            {
                Rigidbody2D body = m_bodies[id];

                if (IsEnemyAlive(id))
                {
                    common.logic.EnemyMovement.Execute(body, m_enemyMovementUpdaters[id].GetNextPosition(), m_enemySettings.Velocity);
                }
            }
        }

        public override void UpdateWorld(WorldState client)
        {
            List<int> toRemove = new List<int>();
            List<int> toRemoveLocal = new List<int>();
            foreach (int id in client.Enemies().Keys)
            {
                EnemyState enemy = m_enemies[id];

                if (IsEnemyAlive(id))
                {
                    enemy.Position.Value = m_enemyMovementUpdaters[id].GetPosition();
                    enemy.HealthPoint.Value = m_enemyMain[id].HealthSystem.GetHealthPoint();
                }
                else
                {
                    toRemove.Add(enemy.GUID.Value);
                    toRemoveLocal.Add(id);
                }
            }

            foreach (int id in toRemove)
            {
                client.Enemies().Remove(id);
            }

            foreach(int id in toRemoveLocal)
            {
                m_enemies.Remove(id);
                m_bodies.Remove(id);
                m_enemyMain.Remove(id);
                m_enemyMovementUpdaters.Remove(id);
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

                EnemyMainServer enemyMain = enemyGameObject.GetComponent<EnemyMainServer>();

                EnemyStateMachine stateMachine = enemyGameObject.GetComponent<EnemyStateMachine>();
                stateMachine.Init(m_playerMovementUpdater, m_worldGenerator, m_pathfindingGridManager);

                int id = System.Guid.NewGuid().GetHashCode();
                body.name = "Server enemy " + id.ToString();

                m_bodies.Add(id, body);
                m_enemyMovementUpdaters.Add(id, enemyPathFindingMovement);
                m_enemyMain.Add(id, enemyMain);

                EnemyState enemyStateData = new EnemyState(id);
                enemyStateData.Position.Value = m_enemyMovementUpdaters[id].GetNextPosition();
                enemyStateData.HealthPoint.Value = m_enemyMain[id].MaxHealthPoint;

                m_enemies.Add(id, enemyStateData);
                state.AddEnemy(enemyStateData);
                ++i;
            }
        }

        bool IsEnemyAlive(int id)
        {
            if (m_enemies[id] == null || m_bodies[id] == null || m_enemyMovementUpdaters[id] == null || m_enemyMain[id] == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
