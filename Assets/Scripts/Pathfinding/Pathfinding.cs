using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding {

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private class Cost
    {
        public int gCost;
        public int hCost;
        public int fCost;
        public int terrainCost;
    }

    Dictionary<PathNode, Cost> m_pathNodeDict;

    public Pathfinding(List<PathNode> pathNodeList) 
    {
        this.m_pathNodeDict = new Dictionary<PathNode, Cost>();

        foreach (PathNode pathNode in pathNodeList)
        {
            m_pathNodeDict.Add(pathNode, new Cost());
        }
        
    }

    public List<PathNode> FindPath(PathNode startNode, PathNode endNode) 
    {
        List<PathNode> openList;
        List<PathNode> closedList;

        if (startNode == null || endNode == null || !m_pathNodeDict.ContainsKey(startNode) || !m_pathNodeDict.ContainsKey(startNode)) 
        {
            // Invalid Path
            return null;
        }

        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        foreach (PathNode pathNode in this.m_pathNodeDict.Keys)
        {
            m_pathNodeDict[pathNode].gCost = int.MaxValue;
            m_pathNodeDict[pathNode].fCost = CalculateFCost(pathNode);
            pathNode.cameFromNode = null;
        }

        m_pathNodeDict[startNode].gCost = 0;
        m_pathNodeDict[startNode].hCost = CalculateDistanceCost(startNode, endNode);
        m_pathNodeDict[startNode].fCost = CalculateFCost(startNode);

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                // Reached final node
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in currentNode.GetNeighbourList()) 
            {
                if (closedList.Contains(neighbourNode)) continue;

                int tentativeGCost = m_pathNodeDict[currentNode].gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < m_pathNodeDict[neighbourNode].gCost) 
                {
                    neighbourNode.cameFromNode = currentNode;
                    m_pathNodeDict[neighbourNode].gCost = tentativeGCost;
                    m_pathNodeDict[neighbourNode].hCost = CalculateDistanceCost(neighbourNode, endNode);
                    m_pathNodeDict[neighbourNode].fCost = CalculateFCost(neighbourNode);

                    if (!openList.Contains(neighbourNode)) 
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        // Out of nodes on the openList
        return null;
    }

    public int CalculateFCost(PathNode pathnode)
    {
        int fCost = m_pathNodeDict[pathnode].gCost + m_pathNodeDict[pathnode].hCost + m_pathNodeDict[pathnode].terrainCost;
        return fCost;

    }

    private List<PathNode> CalculatePath(PathNode endNode) 
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null) 
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b) 
    {
        int xDistance = Mathf.Abs(a.m_x - b.m_x);
        int yDistance = Mathf.Abs(a.m_y - b.m_y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList) 
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++) 
        {
            if (m_pathNodeDict[pathNodeList[i]].fCost < m_pathNodeDict[lowestFCostNode].fCost) 
            {
                lowestFCostNode = pathNodeList[i];
            }
        }

        return lowestFCostNode;
    }
}
