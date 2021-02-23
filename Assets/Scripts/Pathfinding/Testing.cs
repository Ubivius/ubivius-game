using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    private PathNode m_startNode;
    private PathNode m_endNode;
    private List<PathNode> path;

    private PathfindingGridManager m_pathfindingGridManager;
    private List<PathNode> m_pathNodeList;

    public Testing(List<PathNode> pathNodeList, PathfindingGridManager pathfindingGridManager)
    {
        m_pathNodeList = pathNodeList;
        m_pathfindingGridManager = pathfindingGridManager;
    }

    private void Start()
    {
        //pathfindingGridManager = new PathfindingGridManager()
    }

    private void Update()
    {
        path = null;

        if (Input.GetMouseButtonDown(0))
        {
            //Point de départ
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            this.GetXY(mouseWorldPosition, out int x1, out int y1);
            m_startNode = m_pathfindingGridManager.GetNode(x1, y1);

            //Point d'arriver
            mouseWorldPosition = GetMouseWorldPosition();
            this.GetXY(mouseWorldPosition, out int x2, out int y2);
            m_endNode = m_pathfindingGridManager.GetNode(x2, y2);

            path = m_pathfindingGridManager.GetPath(m_startNode, m_endNode);
        }

        if (path != null)
        {
            for (int i=0; i< path.Count-1; i++)
            {
                Debug.DrawLine(new Vector3(path[i].X, path[i].Y) * 10f + Vector3.one * 5f, new Vector3(path[i + 1].X, path[i + 1].Y));
            }
        }
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
