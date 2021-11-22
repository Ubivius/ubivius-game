using Assets.Scripts.Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using ubv.common.world;
using UnityEngine;
using UnityEngine.Events;

namespace ubv.server.logic
{
    public class PathfindingGridManager : MonoBehaviour
    {
        [SerializeField] private WorldGenerator m_worldGenerator;
        [SerializeField] private float m_nodeSize = 1;

        private Vector3 m_worldOrigin = Vector3.zero;

        private LogicGrid m_logicGrid;
        private PathNode[,] m_pathNodes;
        private Pathfinding m_pathfinding;

        private Dictionary<common.world.cellType.LogicCell, PathNode> m_cellToNodes;

        private bool m_setUpDone = false;

        public UnityAction OnPathFindingManagerGenerated;

        private void Awake()
        {
            m_worldGenerator.OnWorldGenerated += OnWorldGenerated;            
        }

        public PathNode[,] GetPathNodeArray()
        {
            return m_pathNodes;
        }

        private void OnWorldGenerated()
        {
            LogicGrid logicGrid = m_worldGenerator.GetMasterLogicGrid();
            this.SetPathNodesFromLogicGrid(logicGrid);
        }

        public void SetPathNodesFromLogicGrid(LogicGrid logicGrid)
        {
            m_cellToNodes = new Dictionary<common.world.cellType.LogicCell, PathNode>();
            m_logicGrid = logicGrid;
            m_pathNodes = new PathNode[m_logicGrid.Width, m_logicGrid.Height];
            List<PathNode> pathNodeList = new List<PathNode>();

            for (int x = 0; x < m_logicGrid.Width; x++)
            {
                for (int y = 0; y < m_logicGrid.Height; y++)
                {
                    if (m_logicGrid.Grid[x, y] != null)
                    {
                        m_pathNodes[x, y] = new PathNode(x, y);
                        m_cellToNodes[m_logicGrid.Grid[x, y]] = m_pathNodes[x, y];
                        m_logicGrid.Grid[x, y].OnChange += UpdateNeighboursOnCellChange;
                        pathNodeList.Add(m_pathNodes[x, y]);
                    }
                }
            }

            //Add neighbours to each walkable pathnode
            foreach (PathNode p in m_pathNodes)
            {
                if (p != null)
                {
                    AddAllWalkableNeighbours(p);
                }
            }

            m_pathfinding = new Pathfinding(pathNodeList);
            m_setUpDone = true;

            OnPathFindingManagerGenerated?.Invoke();
        }

        public bool IsSetUpDone()
        {
            return m_setUpDone;
        }

        public PathNode GetNode(float x, float y)
        {
            int xi = Mathf.FloorToInt(x);
            int yi = Mathf.FloorToInt(y);

            if (xi >= 0 && yi >= 0 && xi < m_logicGrid.Width && yi < m_logicGrid.Height)
            {
                return m_pathNodes[xi, yi];
            }

            return null;
        }

#if UNITY_EDITOR
        public void OpenDoor(int x, int y)
        {
            common.world.cellType.LogicCell cell = m_logicGrid.Grid[x, y];
            if (cell != null)
            {
                if (cell is common.world.cellType.DoorCell door)
                {
                    door.OpenDoor();
                }
            }
        }

        public void CloseDoor(int x, int y)
        {
            common.world.cellType.LogicCell cell = m_logicGrid.Grid[x, y];
            if (cell != null)
            {
                if (cell is common.world.cellType.DoorCell door)
                {
                    door.CloseDoor();
                }
            }
        }
#endif // UNITY_EDITOR

        private void RemoveAllNeighbours(PathNode p)
        {
            // remove all neighbours from p 
            foreach(PathNode n in p.GetNeighbourList())
            {
                n.RemoveNeighbour(p);
            }
            p.RemoveAllNeighbours();
        }

        private void AddAllWalkableNeighbours(PathNode p)
        {
            if(!m_logicGrid.Grid[p.x, p.y].IsWalkable)
            {
                return;
            }
            
            for(int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    PathNode node = GetNodeIfWalkable(p.x + x, p.y + y);
                    if (node != null && node != p)
                    {
                        if (IsNeighbourPathPossible(p, node))
                            p.AddNeighbour(node);
                    }
                }
            }
            
            foreach(PathNode n in p.GetNeighbourList())
            {
                n.AddNeighbour(p);
            }
        }

        private bool IsNeighbourPathPossible(PathNode origin, PathNode goal)
        {
            if (origin.x == goal.x || origin.y == goal.y) return true;

            int dx = goal.x - origin.x;
            int dy = goal.y - origin.y;

            if (GetNodeIfWalkable(origin.x + dx, origin.y) != null &&
                GetNodeIfWalkable(origin.x, origin.y + dy) != null) return true;

            return false;
        }

        public PathNode GetNodeIfWalkable(float x, float y)
        {
            int xi = Mathf.FloorToInt(x);
            int yi = Mathf.FloorToInt(y);

            if (xi >= 0 && yi >= 0 && xi < m_logicGrid.Width && yi < m_logicGrid.Height)
            {
                common.world.cellType.LogicCell cell = m_logicGrid.Grid[xi, yi];
                return cell != null ? (cell.IsWalkable ? GetNode(xi, yi) : null) : null;
            }

            return null;
        }

        public List<PathNode> GetPath(PathNode startNode, PathNode endNode)
        {
            if (!IsSetUpDone()) return null;
            List<PathNode> path = m_pathfinding.FindPath(startNode, endNode);
            return path;
        }

        public PathRoute GetPathRoute(Vector2 start, Vector2 end)
        {
            if (!IsSetUpDone()) return null;
            PathNode startNode = this.GetNode(start.x, start.y);
            PathNode endNode = this.GetNode(end.x, end.y);

            List<PathNode> pathNodeList = this.GetPath(startNode, endNode);

            if (pathNodeList != null)
            {
                return new PathRoute(pathNodeList, m_worldOrigin, m_nodeSize);
            }
            return null;
        }

        private void UpdateNeighboursOnCellChange(common.world.cellType.LogicCell cell)
        {
            if (cell.IsWalkable)
            {
                AddAllWalkableNeighbours(m_cellToNodes[cell]);
            }
            else
            {
                RemoveAllNeighbours(m_cellToNodes[cell]);
            }
        }
    }
}
