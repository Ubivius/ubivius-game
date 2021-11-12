using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Responsible for all Enemy Movement Pathfinding
 * */
namespace ubv.server.logic.ai
{
    public class EnemyMovementUpdater : MonoBehaviour
    {
        private PathfindingGridManager m_pathfindingGridManager;

        private List<Vector2> m_pathVectorList;
        private int m_currentPathIndex;
        private float m_reachedTargetDistance = 0.1f;
        
        private Transform m_enemyTransform;

        public void Init(PathfindingGridManager pathfindingGridManager, Transform transform, float reachTolerance = 0.1f)
        {
            m_currentPathIndex = 0;
            m_pathfindingGridManager = pathfindingGridManager;
            m_reachedTargetDistance = reachTolerance;
            m_enemyTransform = transform;
        }
        
        public Vector2 GetMovementDirection()
        {
            Vector2 delta = GetNextPostion() - GetPosition();
            if (ReachedPosition())
            {
                return Vector2.zero;
            }

            return delta.normalized;
        }
        
        private bool ReachedPosition()
        {
            Vector2 delta = GetNextPostion() - GetPosition();
            return (delta.sqrMagnitude < m_reachedTargetDistance * m_reachedTargetDistance);
        }

        public void Update()
        {
            if (m_pathVectorList != null && m_pathVectorList.Count > 0)
            {
                if (ReachedPosition())
                {
                    m_currentPathIndex++;
                    if (m_currentPathIndex >= m_pathVectorList.Count)
                    {
                        StopMoving();
                    }
                }
            }
        }
        
        public void StopMoving()
        {
            m_pathVectorList = null;
        }

        public void SetTargetPosition(Vector2 targetPosition)
        {
            m_currentPathIndex = 0;
            m_pathVectorList = m_pathfindingGridManager.GetPathRoute(GetPosition(), targetPosition).PathVectorList;
        }
        
        public Vector2 GetPosition()
        {
            return new Vector2(m_enemyTransform.position.x, m_enemyTransform.position.y);
        }
        
        public bool IsPositionWalkable(Vector2 worldPosition)
        {
            return m_pathfindingGridManager.GetNodeIfWalkable(worldPosition.x, worldPosition.y) != null;
        }

        public Vector2 GetNextPostion()
        {
            if (m_pathVectorList == null || m_currentPathIndex >= m_pathVectorList.Count)
            {
                return GetPosition();
            }

            return m_pathVectorList[m_currentPathIndex];
        }
    }
}
