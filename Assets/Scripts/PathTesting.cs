using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Diagnostics;

namespace ubv.server.testing
{
    public class PathTesting : MonoBehaviour
    {
        [SerializeField] private logic.PathfindingGridManager m_pathfindingGridManager;

        public int x, y, endx, endy;
        private List<PathNode> m_path;
        private Stopwatch m_stopwatch;

        public void Start()
        {
            UnityEngine.Debug.Log("Testing instatiate");
            m_stopwatch = new Stopwatch();
        }

        private void Update()
        {
            if (m_path != null)
            {
                for (int i = 0; i < m_path.Count - 1; i++)
                {
                    UnityEngine.Debug.DrawLine(new Vector2(m_path[i].x + 0.5f, m_path[i].y + 0.5f), new Vector2(m_path[i + 1].x + 0.5f, m_path[i + 1].y + 0.5f), Color.green);
                }
            }
        }

        public void TestRandomPath()
        {
            if (m_pathfindingGridManager.IsSetUpDone() == true)
            {
                m_stopwatch.Start();

                m_path = m_pathfindingGridManager.GetPathRoute(
                    new Vector2(x, y),
                    new Vector2(endx, endy)).m_pathNodeList;
                m_stopwatch.Stop();
                UnityEngine.Debug.Log("Elapsed pathfinding time : " + m_stopwatch.ElapsedMilliseconds + " ms");
                m_stopwatch.Reset();
            }
        }

#if UNITY_EDITOR
        public void BlockNode()
        {
            if (m_pathfindingGridManager.IsSetUpDone() == true)
            {
                m_pathfindingGridManager.CloseDoor(x, y);
            }
        }

        public void FreeNode()
        {
            if (m_pathfindingGridManager.IsSetUpDone() == true)
            {
                m_pathfindingGridManager.OpenDoor(x, y);
            }
        }
#endif // UNITY_EDITOR

        //pt pour le faire avec la sourie
        public static Vector3 GetMouseWorldPosition()
        {
            Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
            vec.z = 0f;
            return vec;
        }

        public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
        {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }

        public void GetXY(Vector3 worldPosition, out int x, out int y)
        {
            float cellSize = 1;
            //Serait intéressant d'Avoir un cell size
            x = Mathf.FloorToInt((worldPosition - Vector3.zero).x / cellSize);
            y = Mathf.FloorToInt((worldPosition - Vector3.zero).y / cellSize);
        }
    }
}
