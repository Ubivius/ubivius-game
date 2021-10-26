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

        private PathNode[,] m_pathNodes;

        private Dictionary<int, EnemyStateData> m_enemyStatesData;

        private Dictionary<int, Rigidbody2D> m_bodies;
        private Dictionary<int, EnemyState> m_states;

        private int id;
        public override void Setup()
        {
            //instantier un seul ennemy pur le moment
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_states = new Dictionary<int, EnemyState>();
            m_enemyStatesData = new Dictionary<int, EnemyStateData>();
        }

        public override void InitWorld(WorldState state)
        {
            Debug.Log("GGGGGGGGGGGGGGGGGGGGGGGOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOODDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            Debug.Log("Is pathfinding grid manager ready? = " + m_pathfindingGridManager.IsSetUpDone());

            OnPathFindingManagerGenerated(state); 
        }

        private void OnPathFindingManagerGenerated(WorldState state)
        {
            Debug.Log("ENEMy Pathfinding grid manager is initialized");

            EnemySpawn(state);

            /*m_pathNodes = m_pathfindingGridManager.GetPathNodeArray();

            GameObject enemyGameObject = GameObject.Instantiate(m_enemySettings.SimpleEnemy, new Vector3(107, 124, 0), Quaternion.identity);
            Rigidbody2D body = enemyGameObject.GetComponent<Rigidbody2D>();
            EnemyPathFindingMovement enemyPathFindingMovement = enemyGameObject.GetComponent<EnemyPathFindingMovement>();
            EnemyStateMachine enemyStateMachine = enemyGameObject.GetComponent<EnemyStateMachine>();

            id = System.Guid.NewGuid().GetHashCode();

            enemyPathFindingMovement.SetManager(m_pathfindingGridManager);
            //body.position = new Vector2(110, 160);
            body.position = enemyPathFindingMovement.GetPosition(); //new Vector2(110, 160);
            Debug.Log("Server printing enemy position" + body.position);
            body.name = "Server enemy " + id.ToString(); // + enemyStateMachine.CurrentEnemyState;

            m_states.Add(id, enemyStateMachine.CurrentEnemyState);
            m_bodies.Add(id, body);

            EnemyStateData enemyStateData = new EnemyStateData(id);

            enemyStateData.Position.Value = m_bodies[id].position;
            enemyStateData.EnemyState = m_states[id];

            m_enemyStatesData.Add(id, enemyStateData);
            state.AddEnemy(enemyStateData);*/
        }

        public override void FixedUpdateFromClient(WorldState client, Dictionary<int, InputFrame> frames, float deltaTime)
        {
            // movement
        }

        public override void UpdateWorld(WorldState client)
        {
            Debug.Log("update world chie");
            //Debug.Log(client.Enemies().Keys);
            foreach (int id in client.Enemies().Keys) //ici ca chie
            {
                Rigidbody2D body = m_bodies[id];
                EnemyStateData enemy = m_enemyStatesData[id];
                Debug.Log("Update world enemy positon" + body.position);
                enemy.Position.Value = body.position;
                enemy.Rotation.Value = body.rotation;
                enemy.EnemyState = m_states[id];
            }
        }

        private void EnemySpawn(WorldState state)
        {
            int i = 0;
            int xPos;
            int yPos;
            while (i < m_enemyCount)
            {
                xPos = Random.Range(0, m_pathNodes.GetLength(0) - 1);
                yPos = Random.Range(0, m_pathNodes.GetLength(1) - 1);

                if (m_pathfindingGridManager.GetNodeIfWalkable(xPos, yPos) != null)
                {
                    Debug.Log("Iteration nb" + i + "Nb of enemy=" + m_enemyCount);
                    GameObject enemyGameObject = Instantiate(m_enemySettings.SimpleEnemy, new Vector3(xPos, yPos, 0), Quaternion.identity);
                    Rigidbody2D body = enemyGameObject.GetComponent<Rigidbody2D>();
                    EnemyPathFindingMovement enemyPathFindingMovement = enemyGameObject.GetComponent<EnemyPathFindingMovement>();
                    EnemyStateMachine enemyStateMachine = enemyGameObject.GetComponent<EnemyStateMachine>();

                    id = System.Guid.NewGuid().GetHashCode();

                    enemyPathFindingMovement.SetManager(m_pathfindingGridManager);
                    body.position = enemyPathFindingMovement.GetPosition();
                    Debug.Log("Server printing enemy position" + body.position);
                    body.name = "Server enemy " + id.ToString();

                    m_states.Add(id, enemyStateMachine.CurrentEnemyState);
                    m_bodies.Add(id, body);

                    EnemyStateData enemyStateData = new EnemyStateData(id);

                    enemyStateData.Position.Value = m_bodies[id].position;
                    enemyStateData.EnemyState = m_states[id];

                    m_enemyStatesData.Add(id, enemyStateData);
                    state.AddEnemy(enemyStateData);
                    i++;
                }
            }
        }
    }
}
