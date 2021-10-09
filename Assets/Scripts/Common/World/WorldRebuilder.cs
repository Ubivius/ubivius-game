using System.Collections;
using System.Collections.Generic;
using ubv.common.world.cellType;
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
        [SerializeField] private Tilemap m_playerSpawn;

        // list of tiles 
        [SerializeField] private Tile m_defaultWallTile;
        [SerializeField] private Tile m_defaultFloorTile;
        [SerializeField] private Tile m_defaultDoorTile;
        [SerializeField] private Tile m_defaultInteractableTile;
        [SerializeField] private Tile m_defaultPlayerSpawnTile;
        
        private int m_totalTiles;
        private int m_loadedTiles;

        public bool IsRebuilt { get; private set; }

        private void Awake()
        {
            m_totalTiles = 0;
            m_loadedTiles = 0;
            LoadingData.WorldRebuilder = this;
            IsRebuilt = false;
        }

        private IEnumerator BuildWorldFromCellInfoCoroutine(CellInfo[,] cellInfos)
        {
            // voir https://docs.unity3d.com/ScriptReference/Tilemaps.Tilemap.SetTiles.html
            // créer arrays de tile ensuite call setTiles ?
            Vector3Int pos = new Vector3Int(0, 0, 0);
            m_totalTiles = cellInfos.GetLength(0) * cellInfos.GetLength(1);

            List<Tile> wallCells =          new List<Tile>();
            List<Tile> floorCells =         new List<Tile>();
            List<Tile> doorCells =          new List<Tile>();
            List<Tile> doorButtonCells =    new List<Tile>();
            List<Tile> playerSpawnCells =   new List<Tile>();

            List<Vector3Int> wallPos =        new List<Vector3Int>();
            List<Vector3Int> floorPos =       new List<Vector3Int>();
            List<Vector3Int> doorPos =        new List<Vector3Int>();
            List<Vector3Int> doorButtonPos =  new List<Vector3Int>();
            List<Vector3Int> playerSpawnPos = new List<Vector3Int>();

            for (int x = 0; x < cellInfos.GetLength(0); x++)
            {
                for (int y = 0; y < cellInfos.GetLength(1); y++)
                {
                    LogicCell cell = cellInfos[x, y].CellFromBytes();
                    pos.x = x;
                    pos.y = y;
                    if (cell is WallCell)
                    {
                        wallCells.Add(m_defaultWallTile);
                        wallPos.Add(pos);
                    }
                    else if (cell is FloorCell)
                    {
                        floorCells.Add(m_defaultFloorTile);
                        floorPos.Add(pos);
                    }
                    else if (cell is DoorCell)
                    {
                        doorCells.Add(m_defaultDoorTile);
                        doorPos.Add(pos);
                        floorCells.Add(m_defaultFloorTile);
                        floorPos.Add(pos);
                    }
                    else if (cell is DoorButtonCell)
                    {
                        doorButtonCells.Add(m_defaultInteractableTile);
                        doorButtonPos.Add(pos);
                    }
                    else if (cell is SectionButton)
                    {
                        doorButtonCells.Add(m_defaultInteractableTile);
                        doorButtonPos.Add(pos);
                    }
                    else if (cell is SectionDoorButtonCell)
                    {
                        doorButtonCells.Add(m_defaultInteractableTile);
                        doorButtonPos.Add(pos);
                    }
                    else if (cell is PlayerSpawnCell)
                    {
                        playerSpawnCells.Add(m_defaultPlayerSpawnTile);
                        playerSpawnPos.Add(pos);
                    }
                    m_loadedTiles++;
                }
                yield return null;
            }

            m_walls.SetTiles(wallPos.ToArray(), wallCells.ToArray());
            m_floor.SetTiles(floorPos.ToArray(), floorCells.ToArray());
            m_doors.SetTiles(doorPos.ToArray(), doorCells.ToArray());
            m_interactable.SetTiles(doorButtonPos.ToArray(), doorButtonCells.ToArray());
            m_playerSpawn.SetTiles(playerSpawnPos.ToArray(), playerSpawnCells.ToArray());

            IsRebuilt = true;
            m_onWorldBuilt.Invoke();
        }

        public void BuildWorldFromCellInfo(common.world.cellType.CellInfo[,] cellInfos)
        {
            StartCoroutine(BuildWorldFromCellInfoCoroutine(cellInfos));
        }

        public float GetWorldBuildProgress()
        {
            if (m_totalTiles == 0)
            {
                return 0;
            }
            else
            {
                return (float)m_loadedTiles / (float)m_totalTiles;
            }
        }

        public void OnWorldBuilt(UnityEngine.Events.UnityAction action)
        {
            m_onWorldBuilt += action;
        }
    }
}
