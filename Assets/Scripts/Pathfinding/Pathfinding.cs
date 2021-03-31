using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.server.logic
{
    public class Pathfinding
    {

        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        public class PriorityQueue<T>
        {
            // I'm using an unsorted array for this example, but ideally this
            // would be a binary heap. There's an open issue for adding a binary
            // heap to the standard C# library: https://github.com/dotnet/corefx/issues/574
            //
            // Until then, find a binary heap class:
            // * https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
            // * http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
            // * http://xfleury.github.io/graphsearch.html
            // * http://stackoverflow.com/questions/102398/priority-queue-in-net

            private List<System.Tuple<T, double>> elements = new List<Tuple<T, double>>();

            public int Count
            {
                get { return elements.Count; }
            }

            public void Enqueue(T item, double priority)
            {
                elements.Add(System.Tuple.Create(item, priority));
            }

            public T Dequeue()
            {
                int bestIndex = 0;

                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Item2 < elements[bestIndex].Item2)
                    {
                        bestIndex = i;
                    }
                }

                T bestItem = elements[bestIndex].Item1;
                elements.RemoveAt(bestIndex);
                return bestItem;
            }
        }

        private class NodeInfo
        {
            public PathNode cameFromNode;
        }

        Dictionary<PathNode, NodeInfo> m_pathNodeDict;

        public Pathfinding(IEnumerable<PathNode> pathNodeList)
        {
            this.m_pathNodeDict = new Dictionary<PathNode, NodeInfo>();

            foreach (PathNode pathNode in pathNodeList)
            {
                m_pathNodeDict.Add(pathNode, new NodeInfo());
            }
        }

        public List<PathNode> FindPath(PathNode startNode, PathNode endNode)
        {
            if (startNode == null || endNode == null || !m_pathNodeDict.ContainsKey(startNode) || !m_pathNodeDict.ContainsKey(endNode))
            {
                // Invalid Path
                return null;
            }

            PriorityQueue<PathNode> frontier = new PriorityQueue<PathNode>();
            frontier.Enqueue(startNode, 0);

            Dictionary<PathNode, float> costSoFar = new Dictionary<PathNode, float>();
            costSoFar[startNode] = 0;

            while (frontier.Count > 0)
            {
                PathNode currentNode = frontier.Dequeue();
                if (currentNode == endNode)
                {
                    // Reached final node
                    return CalculatePath(endNode); // bug live si meme place ?
                }

                foreach (PathNode next in currentNode.GetNeighbourList())
                {
                    float newCost = costSoFar[currentNode] + CalculateDistanceCost(currentNode, next);
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        float priority = newCost + CalculateDistanceCost(next, endNode);
                        frontier.Enqueue(next, priority);
                        m_pathNodeDict[next].cameFromNode = currentNode;
                    }
                }
            }

            // Out of nodes
            return null;
        }

        private List<PathNode> CalculatePath(PathNode endNode)
        {
            List<PathNode> path = new List<PathNode>
            {
                endNode
            };
            PathNode currentNode = endNode;
            while (m_pathNodeDict[currentNode].cameFromNode != null)
            {
                path.Add(m_pathNodeDict[currentNode].cameFromNode);
                currentNode = m_pathNodeDict[currentNode].cameFromNode;
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
    }
}
