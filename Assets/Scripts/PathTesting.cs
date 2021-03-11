
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Diagnostics;

public class PathTesting : MonoBehaviour
{
    public int x, y, endx, endy;
    private List<PathNode> path;

    private PathfindingGridManager m_pathfindingGridManager;

    public void Init(PathfindingGridManager pathfindingGridManager)
    {
        m_pathfindingGridManager = pathfindingGridManager;
        UnityEngine.Debug.Log("Testing instatiate");
    }

    private void Update()
    {
        if (path != null)
        {
            for (int i=0; i< path.Count-1; i++)
            {
                UnityEngine.Debug.DrawLine(new Vector3(path[i].X, path[i].Y), new Vector3(path[i + 1].X, path[i + 1].Y));
            }
        }
    }

    private Stopwatch stopwatch = new Stopwatch();


    public void TestRandomPath()
    {
        stopwatch.Start();

        path = m_pathfindingGridManager.GetPathRoute(
            new Vector3(x, y),
            new Vector3(endx, endy)).m_pathNodeList;
        stopwatch.Stop();
        UnityEngine.Debug.Log("Elapsed pathfinding time : " + stopwatch.ElapsedMilliseconds + " ms");
        stopwatch.Reset();
    }
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