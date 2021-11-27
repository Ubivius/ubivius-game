using UnityEngine;
using System.Collections;
using ubv.server.logic;
using ubv.common.world;
using ubv.common;
using ubv.server.logic.ai;
using System.Collections.Generic;

namespace ubv.server.logic
{

    public class EnemyInitializer : ServerInitializer
    {
        [SerializeField] private PlayerMovementUpdater m_playerMovementUpdater;
        [SerializeField] private WorldGenerator m_worldGenerator;
        [SerializeField] private EnemySettings m_enemySettings;
        [SerializeField] private PathfindingGridManager m_pathfindingGridManager;
        [SerializeField] private int m_enemyCount;

        public struct Enemy
        {
            public int id;
            public EnemyMainServer EnemyMain;
            public Rigidbody2D Body;
            public EnemyMovementUpdater EnemyMovement;
        }

        public HashSet<Enemy> Enemies;

        public override void Init()
        {
            int i = 0;
            Enemies = new HashSet<Enemy>();
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

                Enemies.Add(new Enemy() {
                    id = id,
                    EnemyMain = enemyMain,
                    Body = body,
                    EnemyMovement = enemyPathFindingMovement
                });

                ++i;
            }
        }
    }
}