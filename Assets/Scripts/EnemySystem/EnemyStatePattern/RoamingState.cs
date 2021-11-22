using System.Collections;
using UnityEngine;
using ubv.common.serialization;
using System.Collections.Generic;

namespace ubv.server.logic.ai
{
    public class RoamingState : EnemyBehaviorState
    {
        private bool m_inMotion;

        private const float m_playerDetectionRange = 10f;

        private const float m_minimumRoamDistance = 5f;
        private const float m_maximumRoamDistance = 20f;
        private List<Vector2> m_roamPositions;
        private int m_currentRoamPositionIndex;
        private const int m_totalRoamPositions = 3;
        
        public RoamingState(Vector2 startPosition, 
            EnemyMovementUpdater enemyMovement, 
            PlayerMovementUpdater playerMovement, 
            PathfindingGridManager pathfinding) : 
            base(enemyMovement, playerMovement, pathfinding)
        {
            m_currentRoamPositionIndex = 0;
            GenerateRandomRoamingPositions(startPosition);
        }

        private void GenerateRandomRoamingPositions(Vector2 startPosition)
        {
            m_currentRoamPositionIndex = 0;
            m_roamPositions = new List<Vector2>
            {
                startPosition
            };
            m_inMotion = false;
            for (int i = 1; i < m_totalRoamPositions; i++)
            {
                Vector2 pos;
                do
                {
                    pos = GenerateRandomEndPositionFromStart(m_roamPositions[i - 1]);
                }
                while (m_pathfinding.GetPathRoute(m_roamPositions[i - 1], pos) == null);
                m_roamPositions.Add(pos);
            }
            m_enemyMovement.SetPosition(startPosition);
            m_enemyMovement.SetTargetPosition(CurrentRoamPosition());
        }
        
        private Vector2 CurrentRoamPosition()
        {
            return m_roamPositions[m_currentRoamPositionIndex % m_roamPositions.Count];
        }

        public override EnemyBehaviorState Update()
        {
            if (m_enemyMovement.IsDoneMoving())
            {
                int start = m_currentRoamPositionIndex;
                ++m_currentRoamPositionIndex;
                if (!m_enemyMovement.SetTargetPosition(CurrentRoamPosition()))
                {
                    Debug.LogError("Roaming position is unreachable, wtf ?");
                }
            }

            if (DetectsPlayer())
            {
                return new ChasingState(this, m_playerDetectionRange, m_playerMovement, m_enemyMovement, m_pathfinding);
            }

            return this;
        }

        public bool DetectsPlayer()
        {
            var playerGameObjects = m_playerMovement.GetPlayersGameObject().Values;
            foreach (PlayerPrefab player in playerGameObjects)
            {
                Vector2 playerPosition = player.transform.position;
                float playerDist = (playerPosition - m_enemyMovement.GetPosition()).sqrMagnitude;
                if (playerDist < Mathf.Pow(m_playerDetectionRange, 2))
                {
                    return true;
                }
            }

            return false;
        }

        private Vector2 GenerateRandomEndPositionFromStart(Vector2 start)
        {
            Vector2 pos;
            do
            {
                pos = start + Utils.GetRandomDir() * Random.Range(m_minimumRoamDistance, m_maximumRoamDistance);
            }
            while (m_pathfinding.GetPathRoute(start, pos) == null);

            return pos;
        }
    }
}
