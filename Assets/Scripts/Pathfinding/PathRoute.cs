using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public class PathRoute
    {
        public List<PathNode> pathNodeList;
        public List<Vector3> pathVectorList;

        public PathRoute(List<PathNode> pathNodeList, List<Vector3> pathVectorList)
        {
            this.pathNodeList = pathNodeList;
            this.pathVectorList = pathVectorList;
        }

        public PathRoute(List<PathNode> pathNodeList, Vector3 worldOrigin, float nodeSize)
        {
            this.pathNodeList = pathNodeList;
            pathVectorList = new List<Vector3>();
            foreach (PathNode pathNode in pathNodeList)
            {
                pathVectorList.Add(pathNode.GetWorldVector(worldOrigin, nodeSize));
            }
        }

        public void AddVector(Vector3 vector)
        {
            pathVectorList.Add(vector);
        }
    }
}