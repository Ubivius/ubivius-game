using Assets.Scripts.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using ubv.common.world;
using UnityEngine;

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

        private void Start()
        {
            m_worldGenerator.OnWorldGenerated += OnWorldGenerated;            
        }

        private void OnWorldGenerated()
        {
            LogicGrid logicGrid = m_worldGenerator.GetMasterLogicGrid();
            if (logicGrid != null)
            {
                this.SetPathNodesFromLogicGrid(logicGrid);
            }
        }

        private void SetPathNodesFromLogicGrid(LogicGrid logicGrid)
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
        }

        public bool IsSetUpDone()
        {
            return m_setUpDone;
        }

        public PathNode GetNode(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < m_logicGrid.Width && y < m_logicGrid.Height)
            {
                return m_pathNodes[x, y];
            }

            return null;
        }
        
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
            
            p.AddNeighbour(GetNodeIfWalkable(p.x - 1, p.y));
            p.AddNeighbour(GetNodeIfWalkable(p.x - 1, p.y - 1));
            p.AddNeighbour(GetNodeIfWalkable(p.x - 1, p.y + 1));
            
            p.AddNeighbour(GetNodeIfWalkable(p.x + 1, p.y));
            p.AddNeighbour(GetNodeIfWalkable(p.x + 1, p.y - 1));
            p.AddNeighbour(GetNodeIfWalkable(p.x + 1, p.y + 1));
            
            p.AddNeighbour(GetNodeIfWalkable(p.x, p.y - 1));
            p.AddNeighbour(GetNodeIfWalkable(p.x, p.y + 1));

            foreach(PathNode n in p.GetNeighbourList())
            {
                n.AddNeighbour(p);
            }
        }

        private PathNode GetNodeIfWalkable(int x, int y)
        {
            common.world.cellType.LogicCell cell = m_logicGrid.Grid[x, y];
            return cell != null ? (cell.IsWalkable ? GetNode(x, y) : null) : null;
        }

        public List<PathNode> GetPath(PathNode startNode, PathNode endNode)
        {
            List<PathNode> path = m_pathfinding.FindPath(startNode, endNode);
            if (path == null) Debug.Log("No path found!");
            return path;
        }

        public PathRoute GetPathRoute(Vector2 start, Vector2 end)
        {
            PathNode startNode = this.GetNode(Mathf.RoundToInt(start.x), Mathf.RoundToInt(start.y));
            PathNode endNode = this.GetNode(Mathf.RoundToInt(end.x), Mathf.RoundToInt(end.y));

            List<PathNode> pathNodeList = this.GetPath(startNode, endNode);

            return new PathRoute(pathNodeList, m_worldOrigin, m_nodeSize);
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
