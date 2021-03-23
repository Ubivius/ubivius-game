using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Responsible for all Enemy Movement Pathfinding
 * */
public class EnemyPathFindingMovement : MonoBehaviour
{
    [SerializeField] private const float SPEED = 20f;
    [SerializeField] private PathfindingGridManager m_pathfindingGridManager;

    private EnemyMain m_enemyMain;
    private List<Vector2> m_pathVectorList;
    private int m_currentPathIndex;
    private float m_pathfindingTimer;
    private Vector2 m_moveDir;
    private Vector2 m_lastMoveDir;

    private void Awake()
    {
        m_enemyMain = GetComponent<EnemyMain>();
    }

    private void Update()
    {
        m_pathfindingTimer -= Time.deltaTime;

        HandleMovement();
    }

    private void FixedUpdate()
    {
        m_enemyMain.EnemyRigidbody2D.velocity = m_moveDir * SPEED;
    }

    private void HandleMovement()
    {
        PrintPathfindingPath();
        if (m_pathVectorList != null)
        {
            Vector2 targetPosition = m_pathVectorList[m_currentPathIndex];
            float reachedTargetDistance = 5f;
            if (Vector2.Distance(GetPosition(), targetPosition) > reachedTargetDistance)
            {
                m_moveDir = (targetPosition - GetPosition()).normalized;
                m_lastMoveDir = m_moveDir;
                //future moving animation
                //m_enemyMain.CharacterAnims.PlayMoveAnim(moveDir);
            }
            else
            {
                m_currentPathIndex++;
                if (m_currentPathIndex >= m_pathVectorList.Count)
                {
                    StopMoving();
                    //m_enemyMain.CharacterAnims.PlayIdleAnim();
                }
            }
        }
        else
        {
            //m_enemyMain.CharacterAnims.PlayIdleAnim();
        }
    }

    public void StopMoving()
    {
        m_pathVectorList = null;
        m_moveDir = Vector2.zero;
    }

    public List<Vector2> GetPathVectorList()
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

    public void MoveTo(Vector2 targetPosition)
    {
        SetTargetPosition(targetPosition);
    }

    public void MoveToTimer(Vector2 targetPosition)
    {
        if (m_pathfindingTimer <= 0f)
        {
            SetTargetPosition(targetPosition);
        }
    }

    public void SetTargetPosition(Vector2 targetPosition)
    {
        m_currentPathIndex = 0;

        m_pathVectorList = m_pathfindingGridManager.GetPathRoute(GetPosition(), targetPosition).PathVectorList;
        m_pathfindingTimer = .2f;

        if (m_pathVectorList != null && m_pathVectorList.Count > 1)
        {
            m_pathVectorList.RemoveAt(0);
        }
    }

    public Vector2 GetPosition()
    {
        return (Vector2) transform.position;
    }

    public Vector2 GetLastMoveDir()
    {
        return m_lastMoveDir;
    }

    public void Enable()
    {
        enabled = true;
    }

    public void Disable()
    {
        enabled = false;
        m_enemyMain.EnemyRigidbody2D.velocity = Vector2.zero;
    }
}
