using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private List<PathNode> m_neighbourList;

    public readonly int X;
    public readonly int Y;

    public int TerrainCost;

    public PathNode(int x, int y)
    {
        this.X = x;
        this.Y = y;
        m_neighbourList = new List<PathNode>();
    }

    public List<PathNode> GetNeighbourList()
    {
        return this.m_neighbourList;
    }

    public bool AddNeighbour(PathNode pathNode)
    {
        if (pathNode != null)
        {
            this.m_neighbourList.Add(pathNode);
            return true;
        }
        else
            return false;
    }

    public void RemoveNeighbour(PathNode pathNode)
    {
        this.m_neighbourList.Remove(pathNode);
    }
}
