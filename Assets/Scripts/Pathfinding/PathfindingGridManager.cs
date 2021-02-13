using System.Collections;
using System.Collections.Generic;
using ubv.common.world;
using UnityEngine;

public class PathfindingGridManager
{
    private LogicGrid m_logicGrid;
    private List<PathNode> m_pathNodeList;
    private Pathfinding m_pathfinding;

    PathfindingGridManager(LogicGrid logicGrid)
    {
        m_logicGrid = logicGrid;
        m_pathNodeList = new List<PathNode>();

        for (int x = 0; x < m_logicGrid.GetWidth(); x++)
        {
            for (int y = 0; y < m_logicGrid.GetHeight(); y++)
            {
                if (m_logicGrid.Grid[x,y].IsWalkable)
                {   //Sinon juste metttre les nodes a nul lorsque l'on va ajouter les neighbours?
                    m_pathNodeList.Add(new PathNode(x, y));
                }
            }
        }
    }

    public PathNode GetNode(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < m_logicGrid.GetWidth() && y < m_logicGrid.GetHeight() && m_pathNodeList.Contains(new PathNode(x, y)))
        {
            return m_pathNodeList.Find(new PathNode(x, y));
        }
        else
        {
            return null;
        }
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        if (currentNode.X - 1 >= 0)
        {
            // Left
            neighbourList.Add(GetNode(currentNode.X - 1, currentNode.Y));
            // Left Down
            if (currentNode.Y - 1 >= 0) neighbourList.Add(GetNode(currentNode.X - 1, currentNode.Y - 1));
            // Left Up
            if (currentNode.Y + 1 < m_logicGrid.GetHeight()) neighbourList.Add(GetNode(currentNode.X - 1, currentNode.Y + 1));
        }
        if (currentNode.X + 1 < m_logicGrid.GetWidth())
        {
            // Right
            neighbourList.Add(GetNode(currentNode.X + 1, currentNode.Y));
            // Right Down
            if (currentNode.Y - 1 >= 0) neighbourList.Add(GetNode(currentNode.X + 1, currentNode.Y - 1));
            // Right Up
            if (currentNode.Y + 1 < m_logicGrid.GetHeight()) neighbourList.Add(GetNode(currentNode.X + 1, currentNode.Y + 1));
        }
        // Down
        if (currentNode.Y - 1 >= 0) neighbourList.Add(GetNode(currentNode.X, currentNode.Y - 1));
        // Up
        if (currentNode.Y + 1 < m_logicGrid.GetHeight()) neighbourList.Add(GetNode(currentNode.X, currentNode.Y + 1));

        return neighbourList;
    }
}
