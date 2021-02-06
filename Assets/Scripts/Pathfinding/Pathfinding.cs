using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding {

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private List<PathNode> m_pathNodeList;

    public Pathfinding(List<PathNode> pathNodeList) 
    {
        this.m_pathNodeList = new List<PathNode>();
        this.m_pathNodeList = pathNodeList;
    }

    public List<PathNode> FindPath(PathNode startNode, PathNode endNode) 
    {
        List<PathNode> openList;
        List<PathNode> closedList;

        if (startNode == null || endNode == null) 
        {
            // Invalid Path
            return null;
        }

        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        foreach (PathNode pathNode in this.m_pathNodeList)
        {
            pathNode.gCost = 99999999;
            pathNode.CalculateFCost();
            pathNode.cameFromNode = null;
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

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

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost) 
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

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
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList) 
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++) 
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost) 
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}
