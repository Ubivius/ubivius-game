﻿using UnityEngine;
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
        private Dictionary<int, EnemyPathFindingMovement> m_enemyPathFindingMovement;
        private Dictionary<int, Vector2> m_goalPosition;

        private int id;
        public override void Setup()
        {
            m_bodies = new Dictionary<int, Rigidbody2D>();
            m_states = new Dictionary<int, EnemyState>();
            m_enemyStatesData = new Dictionary<int, EnemyStateData>();
            m_enemyPathFindingMovement = new Dictionary<int, EnemyPathFindingMovement>();
            m_goalPosition = new Dictionary<int, Vector2>();
        }

        public override void InitWorld(WorldState state)
        {
            OnPathFindingManagerGenerated(state); 
        }

        private void OnPathFindingManagerGenerated(WorldState state)
        {
            m_pathNodes = m_pathfindingGridManager.GetPathNodeArray();
            EnemySpawn(state);
        }

        public override void FixedUpdateFromClient(WorldState client, Dictionary<int, InputFrame> frames, float deltaTime)
        {
            // movement
        }

        public override void UpdateWorld(WorldState client)
        {
            foreach (int id in client.Enemies().Keys)
            {
                m_goalPosition[id] = m_enemyPathFindingMovement[id].GetNextPostion();

                Rigidbody2D body = m_bodies[id];
                EnemyStateData enemy = m_enemyStatesData[id];

                enemy.Position.Value = body.position;
                enemy.Rotation.Value = body.rotation;
                enemy.EnemyState = m_states[id];
                enemy.GoalPosition.Value = m_goalPosition[id];
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
                    GameObject enemyGameObject = Instantiate(m_enemySettings.SimpleEnemy, new Vector3(xPos, yPos, 0), Quaternion.identity);
                    Rigidbody2D body = enemyGameObject.GetComponent<Rigidbody2D>();
                    EnemyPathFindingMovement enemyPathFindingMovement = enemyGameObject.GetComponent<EnemyPathFindingMovement>();
                    EnemyStateMachine enemyStateMachine = enemyGameObject.GetComponent<EnemyStateMachine>();

                    enemyPathFindingMovement.SetManager(m_pathfindingGridManager);

                    id = System.Guid.NewGuid().GetHashCode();

                    body.position = enemyPathFindingMovement.GetPosition();
                    body.name = "Server enemy " + id.ToString();

                    m_enemyPathFindingMovement.Add(id, enemyPathFindingMovement);
                    m_goalPosition.Add(id, enemyPathFindingMovement.GetNextPostion());
                    m_states.Add(id, enemyStateMachine.CurrentEnemyState);
                    m_bodies.Add(id, body);

                    EnemyStateData enemyStateData = new EnemyStateData(id);
                    enemyStateData.Position.Value = m_bodies[id].position;
                    enemyStateData.GoalPosition.Value = m_goalPosition[id];
                    enemyStateData.EnemyState = m_states[id];

                    m_enemyStatesData.Add(id, enemyStateData);
                    state.AddEnemy(enemyStateData);
                    i++;
                }
            }
        }
    }
}
