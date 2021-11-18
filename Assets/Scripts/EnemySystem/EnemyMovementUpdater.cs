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
        [SerializeField]
        private float m_reachedTargetDistance = 0.1f;

        private PathfindingGridManager m_pathfindingGridManager;
        private List<Vector2> m_pathVectorList;
        private int m_currentPathIndex;

        private bool m_isDoneMoving;

        private void Awake()
        {
            m_currentPathIndex = 0;
            m_isDoneMoving = true;
        }
        
        public Vector2 GetMovementDirection()
        {
            if (!ReachedPosition())
            {
                return (GetNextPosition() - GetPosition()).normalized;
            }

            return Vector2.zero;
        }

        private bool ReachedPosition()
        {
            Vector2 delta = GetNextPosition() - GetPosition();
            return (delta.sqrMagnitude < m_reachedTargetDistance * m_reachedTargetDistance);
        }

        public bool IsDoneMoving()
        {
            return m_isDoneMoving;
        }

        public void Update()
        {
            if (m_pathVectorList != null && m_pathVectorList.Count > 0)
            {
                if (ReachedPosition())
                {
                    transform.position = GetNextPosition();
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
            m_isDoneMoving = true;
        }

        public bool SetTargetPosition(Vector2 targetPosition)
        {
            m_currentPathIndex = 0;
            m_pathVectorList = m_pathfindingGridManager.GetPathRoute(GetPosition(), targetPosition)?.PathVectorList;
            m_isDoneMoving = m_pathVectorList == null;
            return !m_isDoneMoving;
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }
        
        public Vector2 GetPosition()
        {
            return new Vector2(transform.position.x, transform.position.y);
        }

        public Vector2 GetNextPosition()
        {
            if (m_pathVectorList == null || m_currentPathIndex >= m_pathVectorList.Count)
            {
                return GetPosition();
            }

            return m_pathVectorList[m_currentPathIndex];
        }

        public void SetPathfinding(PathfindingGridManager pathfindingGridManager)
        {
            m_pathfindingGridManager = pathfindingGridManager;
        }
    }
}
