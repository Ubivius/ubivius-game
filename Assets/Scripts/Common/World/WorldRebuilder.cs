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
        
        public void BuildWorldFromCellInfo(common.world.cellType.CellInfo[,] cellInfos)
        {
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
                }
            }
            m_onWorldBuilt.Invoke();
        }

        public void OnWorldBuilt(UnityEngine.Events.UnityAction action)
        {
            m_onWorldBuilt += action;
        }
    }
}
