
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Diagnostics;

public class PathTesting : MonoBehaviour
{
    public int x, y, endx, endy;

    private PathNode m_startNode;
    private PathNode m_endNode;
    private List<PathNode> path;

    private PathfindingGridManager m_pathfindingGridManager;
    private List<PathNode> m_pathNodeList;

    private int m_index;

    public void Init(List<PathNode> pathNodeList, PathfindingGridManager pathfindingGridManager)
    {
        m_pathNodeList = pathNodeList;
        m_pathfindingGridManager = pathfindingGridManager;
        UnityEngine.Debug.Log("Testing instatiate");
    }
    /*public Testing(List<PathNode> pathNodeList, PathfindingGridManager pathfindingGridManager)
    {
        m_pathNodeList = pathNodeList;
        m_pathfindingGridManager = pathfindingGridManager;
        Debug.Log("Testing instatiate");
    }*/

    private void Start()
    {
        //pathfindingGridManager = new PathfindingGridManager()
        /*m_pathNodeList = pathNodeList;
        m_pathfindingGridManager = pathfindingGridManager;
        Debug.Log("Testing instatiate");*/
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
        //m_index = UnityEngine.Random.Range(0, m_pathNodeList.Count - 1);
        m_startNode = m_pathfindingGridManager.GetNode(x, y);

        //m_index = UnityEngine.Random.Range(0, m_pathNodeList.Count - 1);

        m_endNode = m_pathfindingGridManager.GetNode(endx, endy);

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


/*ubv.common.world.LogicGrid testGrid = new LogicGrid(5, 5);


            for (int x= 0; x<testGrid.Width; x++)
            {
                for (int y=0; y<testGrid.Height; y++)
                {

                    testGrid.Grid[x, y] = new world.cellType.FloorCell();
                }
            }*/
//if (m_masterLogicGrid.Grid[6,6] != null)
//{
//m_pathfindingGridManager = new PathfindingGridManager(testGrid);
//}