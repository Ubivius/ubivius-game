using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private List<PathNode> m_neighbourList;

    public readonly int x;
    public readonly int y;

    public int TerrainCost;

    public PathNode(int x, int y)
    {
        this.x = x;
        this.y = y;
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

    public void RemoveAllNeighbours()
    {
        m_neighbourList.Clear();
    }

    public void RemoveNeighbour(PathNode pathNode)
    {
        this.m_neighbourList.Remove(pathNode);
    }

    public Vector3 GetWorldVector(Vector3 worldOrigin, float nodeSize)
    {
        return worldOrigin + new Vector3((x + 0.5f) * nodeSize, (y + 0.5f) * nodeSize);
    }
}
