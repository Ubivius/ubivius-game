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

        private const int m_updateInverseRate = 10;
        private readonly int m_updateOffset;

        private const float m_suspiciousTime = 5.0f;
        private float m_suspiciousTimer;
        
        public ChasingState(RoamingState roaming, float detectionRange,
            PlayerMovementUpdater playerMovement, 
            EnemyMovementUpdater enemyMovement, 
            PathfindingGridManager pathfinding)
            : base(enemyMovement, playerMovement, pathfinding)
        {
            m_playerDetectionRange = detectionRange;
            m_lastRoamingState = roaming;
            m_updateOffset = Random.Range(0, m_updateInverseRate);
            m_suspiciousTimer = 0;
        }

        public override EnemyBehaviorState Update()
        {
            if (Time.frameCount % m_updateInverseRate == m_updateOffset)
            {
                float updateTime = Time.fixedDeltaTime * m_updateInverseRate;
                if (!SpotPlayer())
                {
                    m_suspiciousTimer += updateTime;
                }
                else
                {
                    bool reachable = m_enemyMovement.SetTargetPosition(m_lastSeenPlayerPosition);
                    if (reachable)
                    {
                        m_suspiciousTimer = 0;
                    }
                    else
                    {
                        m_suspiciousTimer += updateTime;
                    }
                }
            }

            if(m_suspiciousTimer > m_suspiciousTime)
            {
                m_suspiciousTimer = 0;
                return m_lastRoamingState;
            }

            return this;
        }

        private bool SpotPlayer()
        {
            var playerGameObjects = m_playerMovement.GetPlayersGameObject();
            Vector2 closestPlayer = new Vector2(-100, -100);
            float closestPlayerDist = 1000;
            bool detectedPlayer = false;
            foreach (int id in playerGameObjects.Keys)
            {
                PlayerPrefab player = playerGameObjects[id];
                if (m_playerMovement.IsPlayerAlive(id))
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
            }

            if (detectedPlayer)
            {
                m_lastSeenPlayerPosition = closestPlayer;
            }

            return detectedPlayer;
        }
    }
}
