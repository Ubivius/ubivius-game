
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Diagnostics;

public class PathTesting : MonoBehaviour
{
    [SerializeField] private PathfindingGridManager m_pathfindingGridManager;

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
            for (int i=0; i< m_path.Count-1; i++)
            {
                UnityEngine.Debug.DrawLine(new Vector2(m_path[i].X, m_path[i].Y), new Vector2(m_path[i + 1].X, m_path[i + 1].Y));
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

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        float cellSize = 1;
        //Serait intéressant d'Avoir un cell size
        x = Mathf.FloorToInt((worldPosition - Vector3.zero).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - Vector3.zero).y / cellSize);
    }
}
