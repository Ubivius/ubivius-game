using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.EnemySystem.EnemyStatePattern
{
    public class EnemyPathFindingMovement : MonoBehaviour
    {

        [SerializeField] private float m_speed;
        [SerializeField] private PathfindingGridManager m_pathfindingGridManager;

        private EnemyMain enemyMain;
        private List<Vector3> m_pathVectorList;
        private int currentPathIndex;
        private float pathfindingTimer;
        private Vector3 moveDir;
        private Vector3 lastMoveDir;

        private void Awake()
        {
            enemyMain = GetComponent<EnemyMain>();
        }

        private void Update()
        {
            pathfindingTimer -= Time.deltaTime;

            HandleMovement();
        }

        private void FixedUpdate()
        {
            enemyMain.EnemyRigidbody2D.velocity = moveDir * m_speed;
        }

        private void HandleMovement()
        {
            PrintPathfindingPath();
            if (m_pathVectorList != null)
            {
                Vector3 targetPosition = m_pathVectorList[currentPathIndex];
                float reachedTargetDistance = 5f;
                if (Vector3.Distance(GetPosition(), targetPosition) > reachedTargetDistance)
                {
                    moveDir = (targetPosition - GetPosition()).normalized;
                    lastMoveDir = moveDir;
                    //future moving animation
                    //enemyMain.CharacterAnims.PlayMoveAnim(moveDir);
                }
                else
                {
                    currentPathIndex++;
                    if (currentPathIndex >= m_pathVectorList.Count)
                    {
                        StopMoving();
                        //enemyMain.CharacterAnims.PlayIdleAnim();
                    }
                }
            }
            else
            {
                //enemyMain.CharacterAnims.PlayIdleAnim();
            }
        }

        public void StopMoving()
        {
            m_pathVectorList = null;
            moveDir = Vector3.zero;
        }

        public List<Vector3> GetPathVectorList()
        {
            return m_pathVectorList;
        }

        private void PrintPathfindingPath()
        {
            if (m_pathVectorList != null)
            {
                for (int i = 0; i < m_pathVectorList.Count - 1; i++)
                {
                    Debug.DrawLine(m_pathVectorList[i], m_pathVectorList[i + 1]);
                }
            }
        }

        public void MoveTo(Vector3 targetPosition)
        {
            SetTargetPosition(targetPosition);
        }

        public void MoveToTimer(Vector3 targetPosition)
        {
            if (pathfindingTimer <= 0f)
            {
                SetTargetPosition(targetPosition);
            }
        }

        public void SetTargetPosition(Vector3 targetPosition)
        {
            currentPathIndex = 0;

            m_pathVectorList = m_pathfindingGridManager.GetPathRoute(GetPosition(), targetPosition).pathVectorList;
            //pathVectorList = GridPathfinding.instance.GetPathRouteWithShortcuts(GetPosition(), targetPosition).pathVectorList;
            pathfindingTimer = .2f;
            //pathVectorList = new List<Vector3> { targetPosition };

            if (m_pathVectorList != null && m_pathVectorList.Count > 1)
            {
                m_pathVectorList.RemoveAt(0);
            }
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public Vector3 GetLastMoveDir()
        {
            return lastMoveDir;
        }

        public void Enable()
        {
            enabled = true;
        }

        public void Disable()
        {
            enabled = false;
            enemyMain.EnemyRigidbody2D.velocity = Vector3.zero;
        }
    }
}