using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{

    private List<PathNode> neighbourList;

    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    public PathNode cameFromNode;

    public PathNode(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public List<PathNode> GetNeighbourList()
    {
        return neighbourList;
    }
}
