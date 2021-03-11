using Assets.Scripts.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using ubv.common.world;
using UnityEngine;

public class PathfindingGridManager: MonoBehaviour
{
    [SerializeField] private WorldGenerator m_worldGenerator;

    [SerializeField] private float m_nodeSize = 1;
    private Vector3 m_worldOrigin = Vector3.zero;

    private LogicGrid m_logicGrid;
    private PathNode[,] m_pathNodes;
    private Pathfinding m_pathfinding;

    private void Start()
    {
        LogicGrid logicGrid = m_worldGenerator.GetMasterLogicGrid();
        if (logicGrid != null)
        {
            this.SetPathfindingGridManager(logicGrid);
        }
    }

    private void SetPathfindingGridManager(LogicGrid logicGrid)
    {
        m_logicGrid = logicGrid;
        m_pathNodes = new PathNode[m_logicGrid.Width, m_logicGrid.Height];
        List<PathNode> pathNodeList = new List<PathNode>();

        for (int x = 0; x < m_logicGrid.Width; x++)
        {
            for (int y = 0; y < m_logicGrid.Height; y++)
            {
                if (m_logicGrid.Grid[x, y] != null)
                {
                    if (m_logicGrid.Grid[x, y].IsWalkable)
                    {
                        m_pathNodes[x, y] = (new PathNode(x, y));
                        pathNodeList.Add(m_pathNodes[x, y]);
                    }
                }
            }
        }

        //Add neighbours to each pathnode
        foreach(PathNode pathnode in m_pathNodes)
        {
            if (pathnode != null)
            {
                if (pathnode.X - 1 >= 0)
                {
                    // Left
                    pathnode.AddNeighbour(GetNode(pathnode.X - 1, pathnode.Y));
                    // Left Down
                    if (pathnode.Y - 1 >= 0) pathnode.AddNeighbour(GetNode(pathnode.X - 1, pathnode.Y - 1));
                    // Left Up
                    if (pathnode.Y + 1 < m_logicGrid.Height) pathnode.AddNeighbour(GetNode(pathnode.X - 1, pathnode.Y + 1));
                }

                if (pathnode.X + 1 < m_logicGrid.Width)
                {
                    // Right
                    pathnode.AddNeighbour(GetNode(pathnode.X + 1, pathnode.Y));
                    // Right Down
                    if (pathnode.Y - 1 >= 0) pathnode.AddNeighbour(GetNode(pathnode.X + 1, pathnode.Y - 1));
                    // Right Up
                    if (pathnode.Y + 1 < m_logicGrid.Height) pathnode.AddNeighbour(GetNode(pathnode.X + 1, pathnode.Y + 1));
                }

                // Down
                if (pathnode.Y - 1 >= 0) pathnode.AddNeighbour(GetNode(pathnode.X, pathnode.Y - 1));
                // Up
                if (pathnode.Y + 1 < m_logicGrid.Height) pathnode.AddNeighbour(GetNode(pathnode.X, pathnode.Y + 1));

            }
        }

        m_pathfinding = new Pathfinding(pathNodeList);
        

        GameObject testing = new GameObject("PathfindingTesting");
        PathTesting Pathfindingtesting = testing.AddComponent<PathTesting>();
        Pathfindingtesting.Init(this);
    }

    public PathNode GetNode(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < m_logicGrid.Width && y < m_logicGrid.Height)
        {
            return m_pathNodes[x, y];
        }
        
        return null;
    }

    public  List<PathNode> GetPath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> path = m_pathfinding.FindPath(startNode, endNode);
        if (path == null) Debug.Log("Not path found!");
        return path;
    }

    public PathRoute GetPathRoute(Vector3 start, Vector3 end)
    {
        PathNode startNode = this.GetNode(Mathf.RoundToInt(start.x), Mathf.RoundToInt(start.y));
        PathNode endNode = this.GetNode(Mathf.RoundToInt(end.x), Mathf.RoundToInt(end.y));

        List<PathNode> pathNodeList = this.GetPath(startNode, endNode);

        return new PathRoute(pathNodeList, m_worldOrigin, m_nodeSize);
    }

}
