using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private List<PathNode> m_neighbourList;

    public readonly int m_x;
    public readonly int m_y;

    public PathNode cameFromNode;

    public PathNode(int x, int y)
    {
        this.m_x = x;
        this.m_y = y;
    }

    public List<PathNode> GetNeighbourList()
    {
        return this.m_neighbourList;
    }

    public void AddNeighbour(PathNode pathNode)
    {
        this.m_neighbourList.Add(pathNode);
    }

    public void RemoveNeighbour(PathNode pathNode)
    {
        this.m_neighbourList.Remove(pathNode);
    }
}
