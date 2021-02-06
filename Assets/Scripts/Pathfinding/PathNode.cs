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
    
    public void SetNeighbourList(PathNode[,] pathnodeGrid, int pathNodeGridWidth, int pathNodeGridHeight) 
    {
        if (this.x - 1 >= 0)
        {
            // Left
            neighbourList.Add(pathnodeGrid[this.x - 1, this.y]);
            // Left Down
            if (this.y - 1 >= 0) neighbourList.Add(pathnodeGrid[this.x - 1, this.y - 1]);
            // Left Up
            if (this.y + 1 < pathNodeGridHeight) neighbourList.Add(pathnodeGrid[this.x - 1, this.y + 1]);
        }
        if (this.x + 1 < pathNodeGridWidth)
        {
            // Right
            neighbourList.Add(pathnodeGrid[this.x + 1, this.y]);
            // Right Down
            if (this.y - 1 >= 0) neighbourList.Add(pathnodeGrid[this.x + 1, this.y - 1]);
            // Right Up
            if (this.y + 1 < pathNodeGridHeight) neighbourList.Add(pathnodeGrid[this.x + 1, this.y + 1]);
        }
        // Down
        if (this.y - 1 >= 0) neighbourList.Add(pathnodeGrid[this.x, this.y - 1]);
        // Up
        if (this.y + 1 < pathNodeGridHeight) neighbourList.Add(pathnodeGrid[this.x, this.y + 1]);
    }

    public List<PathNode> GetNeighbourList()
    {
        return neighbourList;
    }

    public List<PathNode> SetAndGetNeighbourList(PathNode[,] pathnodeGrid, int pathNodeGridWidth, int pathNodeGridHeight)
    {
        SetNeighbourList(pathnodeGrid,  pathNodeGridWidth, pathNodeGridHeight);

        return GetNeighbourList();
    }
}
