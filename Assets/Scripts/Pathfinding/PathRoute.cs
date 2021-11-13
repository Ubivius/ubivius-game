using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public class PathRoute
    {
        public List<PathNode> PathNodeList { get; private set; }
        public List<Vector2> PathVectorList { get; private set; }

        public PathRoute(List<PathNode> pathNodeList, Vector3 worldOrigin, float nodeSize)
        {
            this.PathNodeList = pathNodeList ?? new List<PathNode>();
            PathVectorList = new List<Vector2>();
            foreach (PathNode pathNode in PathNodeList)
            {
                PathVectorList.Add(pathNode.GetWorldVector(worldOrigin, nodeSize));
            }
        }

        public void AddVector(Vector2 vector)
        {
            PathVectorList.Add(vector);
        }
    }
}
