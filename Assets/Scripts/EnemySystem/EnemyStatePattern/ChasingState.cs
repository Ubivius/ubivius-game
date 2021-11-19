using System.Collections;
using UnityEngine;
using ubv.common.serialization;

namespace ubv.server.logic.ai
{
    public class ChasingState : EnemyBehaviorState
    {
        private float m_playerDetectionRange;
        private Vector2 m_lastSeenPlayerPosition;
        private RoamingState m_lastRoamingState;

        private const int m_updateInverseRate = 60;
        
        public ChasingState(RoamingState roaming, float detectionRange,
            PlayerMovementUpdater playerMovement, 
            EnemyMovementUpdater enemyMovement, 
            PathfindingGridManager pathfinding)
            : base(enemyMovement, playerMovement, pathfinding)
        {
            m_playerDetectionRange = detectionRange;
            m_lastRoamingState = roaming;
        }

        public override EnemyBehaviorState Update()
        {
            if (Time.frameCount % m_updateInverseRate == 0)
            {
                if (!SpotPlayer())
                {
                    return m_lastRoamingState;
                }
                else
                {
                    m_enemyMovement.SetTargetPosition(m_lastSeenPlayerPosition);
                }
            }
            return this;
        }

        private bool SpotPlayer()
        {
            var playerGameObjects = m_playerMovement.GetPlayersGameObject().Values;
            Vector2 closestPlayer = new Vector2(-100, -100);
            float closestPlayerDist = 1000;
            bool detectedPlayer = false;
            foreach (PlayerPrefab player in playerGameObjects)
            {
                Vector2 playerPosition = player.transform.position;
                float playerDist = (playerPosition - m_enemyMovement.GetPosition()).sqrMagnitude;
                if (playerDist < Mathf.Pow(m_playerDetectionRange, 2))
                {
                    detectedPlayer = true;
                }

                if (detectedPlayer)
                {
                    if (closestPlayerDist > playerDist)
                    {
                        closestPlayerDist = playerDist;
                        closestPlayer = playerPosition;
                    }
                }
            }

            if (detectedPlayer)
            {
                m_lastSeenPlayerPosition = closestPlayer;
            }

            return detectedPlayer;
        }
    }
}
