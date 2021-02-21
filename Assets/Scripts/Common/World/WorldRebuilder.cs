using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ubv.client.world
{
    public class WorldRebuilder : MonoBehaviour
    {
        // TODO : check RoomInfo and merge compatible behaviour ?
        UnityEngine.Events.UnityAction m_onWorldBuilt;
        
        [SerializeField] private Tilemap m_floor;
        [SerializeField] private Tilemap m_walls;
        [SerializeField] private Tilemap m_doors;
        [SerializeField] private Tilemap m_interactable;

        // list of tiles 

        [SerializeField] private Tile m_defaultWallTile;
        [SerializeField] private Tile m_defaultFloorTile;
        [SerializeField] private Tile m_defaultDoorTile;
        [SerializeField] private Tile m_defaultInteractableTile;

        common.world.cellType.CellInfo[,] m_cellInfos;
        private bool m_isBuildingWorld;

        private void Awake()
        {
            m_cellInfos = null;
            m_isBuildingWorld = false;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(m_cellInfos != null && !m_isBuildingWorld)
            {
                m_isBuildingWorld = true;
                StartCoroutine(RebuildWorldCoroutine(m_cellInfos));
            }
        }

        public void BuildWorldFromCellInfo(common.world.cellType.CellInfo[,] cellInfos)
        {
            m_cellInfos = cellInfos;
        }

        public void OnWorldBuilt(UnityEngine.Events.UnityAction action)
        {
            m_onWorldBuilt += action;
        }
        
        // every new cell created must also be added here
        // not pretty but it'll work for now
        private IEnumerator RebuildWorldCoroutine(common.world.cellType.CellInfo[,] cellInfos)
        {
            m_isBuildingWorld = true;
            Vector3Int pos = new Vector3Int(0, 0, 0);
            for (int x = 0; x < cellInfos.GetLength(0); x++)
            {
                for (int y = 0; y < cellInfos.GetLength(1); y++)
                {
                    common.world.cellType.LogicCell cell = cellInfos[x, y].CellFromBytes();
                    pos.x = x;
                    pos.y = y;
                    if (cell is common.world.cellType.WallCell)
                    {
                        m_walls.SetTile(pos, m_defaultWallTile);
                    }
                    else if (cell is common.world.cellType.FloorCell)
                    {
                        m_floor.SetTile(pos, m_defaultFloorTile);
                    }
                    else if (cell is common.world.cellType.DoorCell)
                    {
                        m_doors.SetTile(pos, m_defaultDoorTile);
                    }
                    else if (cell is common.world.cellType.DoorButtonCell)
                    {
                        m_interactable.SetTile(pos, m_defaultInteractableTile);
                    }

                    yield return null;
                }
            }
            m_cellInfos = null;
            m_isBuildingWorld = false;
            m_onWorldBuilt.Invoke();
        }
    }
}
