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
        [SerializeField] private int m_enemyCount;

        private PathNode[,] m_mapNodes;

        private Dictionary<int, EnemyState> m_enemies;

        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, EnemyMovementUpdater> m_enemyMovementUpdaters;
        private Dictionary<int, EnemyMain> m_enemyMain;

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
                enemy.HealthPoint.Value = m_enemyMain[id].HealthSystem.GetHealthPoint();
            }
        }

        private void EnemySpawn(WorldState state)
        {
            int i = 0;
            int xPos;
            int yPos;
            while (i < m_enemyCount)
            {
                xPos = Random.Range(0, m_mapNodes.GetLength(0) - 1);
                yPos = Random.Range(0, m_mapNodes.GetLength(1) - 1);

                if (m_pathfindingGridManager.GetNodeIfWalkable(xPos, yPos) != null)
                {
                    GameObject enemyGameObject = Instantiate(m_enemySettings.EnemyPrefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
                    Rigidbody2D body = enemyGameObject.GetComponent<Rigidbody2D>();
                    EnemyMovementUpdater enemyPathFindingMovement = enemyGameObject.GetComponent<EnemyMovementUpdater>();
                    enemyPathFindingMovement.SetPathfinding(m_pathfindingGridManager);

                    EnemyMain enemyMain = enemyGameObject.GetComponent<EnemyMain>();

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
        }
    }
}
