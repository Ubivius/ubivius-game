using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ubv.common.world.cellType;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ubv.client.world
{
    public class WorldRebuilder : MonoBehaviour
    {
        [SerializeField] private Tilemap m_floor;
        [SerializeField] private Tilemap m_walls;
        [SerializeField] private Tilemap m_doors;
        [SerializeField] private Tilemap m_interactable;
        [SerializeField] private Tilemap m_playerSpawn;

        // list of tiles 
        [SerializeField] private Tile m_floorCenterTile;
        [SerializeField] private Tile m_floorBottomLeftTile;
        [SerializeField] private Tile m_floorTopLeftTile;
        [SerializeField] private Tile m_floorBottomRightTile;
        [SerializeField] private Tile m_floorTopRightTile;

        [SerializeField] private Tile m_defaultDoorTile;
        [SerializeField] private Tile m_interactableDefaultTile;
        [SerializeField] private Tile m_interactableTopRightTile;
        [SerializeField] private Tile m_interactableBottomRightTile;
        [SerializeField] private Tile m_interactableBottomLeftTile;
        [SerializeField] private Tile m_interactableTopLeftTile;
        [SerializeField] private Tile m_defaultPlayerSpawnTile;

        [SerializeField] private Tile m_defaultWallTile;

        [SerializeField] private Tile m_wallCenterTile;
        [SerializeField] private Tile m_wallUUpTile;
        [SerializeField] private Tile m_wallUDownTile;
        [SerializeField] private Tile m_wallULefTile;
        [SerializeField] private Tile m_wallURightTile;
        [SerializeField] private Tile m_wallVerticalTile;
        [SerializeField] private Tile m_wallHorizontalTile;
        [SerializeField] private Tile m_wallSideTile;

        [SerializeField] private Tile m_wallLeftTile;
        [SerializeField] private Tile m_wallRightTile;
        [SerializeField] private Tile m_wallUpTile;
        [SerializeField] private Tile m_wallDownTile;

        [SerializeField] private Tile m_cornerInsideUpLeftTile;
        [SerializeField] private Tile m_cornerInsideUpRightTile;
        [SerializeField] private Tile m_cornerInsideDownLeftTile;
        [SerializeField] private Tile m_cornerInsideDownRightTile;

        [SerializeField] private Tile m_cornerOutsideUpLeftTile;
        [SerializeField] private Tile m_cornerOutsideUpRightTile;
        [SerializeField] private Tile m_cornerOutsideDownLeftTile;
        [SerializeField] private Tile m_cornerOutsideDownRightTile;


        public List<Vector2Int> FinalDoor;
        public List<Vector2Int> DoorNorth;
        public List<Vector2Int> DoorEast;
        public List<Vector2Int> DoorSouth;
        public List<Vector2Int> DoorWest;
        public List<Vector2Int> DoorSection0NorthEast;
        public List<Vector2Int> DoorSection0SouthEast;
        public List<Vector2Int> DoorSection0SouthWest;
        public List<Vector2Int> DoorSection0NorthWest;

        private void Awake()
        {
            FinalDoor = new List<Vector2Int>();
            DoorNorth = new List<Vector2Int>();
            DoorEast = new List<Vector2Int>();
            DoorSouth = new List<Vector2Int>();
            DoorWest = new List<Vector2Int>();
            DoorSection0NorthEast = new List<Vector2Int>();
            DoorSection0SouthEast = new List<Vector2Int>();
            DoorSection0SouthWest = new List<Vector2Int>();
            DoorSection0NorthWest = new List<Vector2Int>();
            BuildWorldFromCellInfo(data.LoadingData.ServerInit.CellInfo2DArray.Value);
        }

        private void BuildWorldFromCellInfo(CellInfo[,] cellInfos)
        {
            // voir https://docs.unity3d.com/ScriptReference/Tilemaps.Tilemap.SetTiles.html
            // créer arrays de tile ensuite call setTiles ?
            Vector3Int pos = new Vector3Int(0, 0, 0);

            int mapWidth = cellInfos.GetLength(0);
            int mapHeight = cellInfos.GetLength(1);

            int xHalf = mapWidth / 2;
            int xOneThird = mapWidth / 3;
            int xTwoThird = 2 * mapWidth / 3;

            int yHalf = mapHeight / 2;
            int yOneThird = mapHeight / 3;
            int yTwoThird = 2 * mapHeight / 3;

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

            int sectionButtonCounter = 0;

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    LogicCell cell = cellInfos[x, y].CellFromBytes();
                    LogicCell leftCell = cellInfos[x, y].CellFromBytes();
                    LogicCell rightCell = cellInfos[x, y].CellFromBytes();
                    LogicCell topCell = cellInfos[x, y].CellFromBytes();
                    LogicCell bottomCell = cellInfos[x, y].CellFromBytes();

                    LogicCell diagoUpLeftCell = cellInfos[x, y].CellFromBytes();
                    LogicCell diagoUpRightCell = cellInfos[x, y].CellFromBytes();
                    LogicCell diagoBottomLeftCell = cellInfos[x, y].CellFromBytes();
                    LogicCell diagoBottomRightCell = cellInfos[x, y].CellFromBytes();

                    


                    if (x <= 0 ) {
                        leftCell = cellInfos[x, y].CellFromBytes();
                        rightCell = cellInfos[x + 1, y].CellFromBytes();
                    }
                    else if (x >= cellInfos.GetLength(0) - 1) {
                        rightCell = cellInfos[x, y].CellFromBytes();
                        leftCell = cellInfos[x - 1, y].CellFromBytes();
                    }
                    else {
                        leftCell = cellInfos[x - 1, y].CellFromBytes();
                        rightCell = cellInfos[x + 1, y].CellFromBytes();
                    }

                    if (y <= 0)
                        bottomCell = cellInfos[x, y].CellFromBytes();
                    else if (y >= cellInfos.GetLength(1) - 1)
                        topCell = cellInfos[x, y].CellFromBytes();
                    else {
                        topCell = cellInfos[x, y + 1].CellFromBytes();
                        bottomCell = cellInfos[x, y - 1].CellFromBytes();
                    }

                    if (x <= 0) {
                        if (y >= cellInfos.GetLength(1) - 1) {
                            diagoUpLeftCell = cellInfos[x, y].CellFromBytes();
                            diagoUpRightCell = cellInfos[x + 1, y].CellFromBytes();
                        }
                        else {
                            diagoUpLeftCell = cellInfos[x, y + 1].CellFromBytes();
                            diagoUpRightCell = cellInfos[x + 1, y + 1].CellFromBytes();
                        }

                        if (y <= 0) {
                            diagoBottomLeftCell = cellInfos[x, y].CellFromBytes();
                            diagoBottomRightCell = cellInfos[x + 1, y].CellFromBytes();
                        }
                        else {
                            diagoBottomLeftCell = cellInfos[x, y - 1].CellFromBytes();
                            diagoBottomRightCell = cellInfos[x + 1, y - 1].CellFromBytes();
                        }
                    }
                    else if (x >= cellInfos.GetLength(0) - 1) {
                        if (y >= cellInfos.GetLength(1) - 1) {
                            diagoUpRightCell = cellInfos[x, y].CellFromBytes();
                            diagoUpLeftCell = cellInfos[x - 1, y].CellFromBytes();
                        }
                        else {
                            diagoUpRightCell = cellInfos[x, y + 1].CellFromBytes();
                            diagoUpLeftCell = cellInfos[x - 1, y + 1].CellFromBytes();
                        }

                        if (y <= 0) {
                            diagoBottomRightCell = cellInfos[x, y].CellFromBytes();
                            diagoBottomLeftCell = cellInfos[x - 1, y].CellFromBytes();
                        }
                        else {
                            diagoBottomRightCell = cellInfos[x, y - 1].CellFromBytes();
                            diagoBottomLeftCell = cellInfos[x - 1, y - 1].CellFromBytes();
                        }
                    }
                    else {
                        if (y >= cellInfos.GetLength(1) - 1) {
                            diagoUpRightCell = cellInfos[x + 1, y].CellFromBytes();
                            diagoUpLeftCell = cellInfos[x - 1, y].CellFromBytes();
                        }
                        else {
                            diagoUpRightCell = cellInfos[x + 1, y + 1].CellFromBytes();
                            diagoUpLeftCell = cellInfos[x - 1, y + 1].CellFromBytes();
                        }

                        if (y <= 0) {
                            diagoBottomRightCell = cellInfos[x + 1, y].CellFromBytes();
                            diagoBottomLeftCell = cellInfos[x - 1, y].CellFromBytes();
                        }
                        else {
                            diagoBottomRightCell = cellInfos[x + 1, y - 1].CellFromBytes();
                            diagoBottomLeftCell = cellInfos[x - 1, y - 1].CellFromBytes();
                        }
                    }

                    pos.x = x;
                    pos.y = y;

                    if (cell is WallCell)
                    {
                        wallPos.Add(pos);

                        if (leftCell is WallCell && rightCell is WallCell && topCell is WallCell && bottomCell is WallCell && diagoUpLeftCell is WallCell && diagoUpRightCell is WallCell && diagoBottomLeftCell is WallCell && diagoBottomRightCell is WallCell)
                            wallCells.Add(m_defaultWallTile);

                        else if (leftCell is WallCell && rightCell is WallCell == false && topCell is WallCell == false && bottomCell is WallCell == false)
                            wallCells.Add(m_wallULefTile);
                        else if (leftCell is WallCell == false && rightCell is WallCell && topCell is WallCell == false && bottomCell is WallCell == false)
                            wallCells.Add(m_wallURightTile);
                        else if (leftCell is WallCell == false && rightCell is WallCell == false && topCell is WallCell && bottomCell is WallCell == false)
                            wallCells.Add(m_wallUUpTile);
                        else if (leftCell is WallCell == false && rightCell is WallCell == false && topCell is WallCell == false && bottomCell is WallCell)
                            wallCells.Add(m_wallUDownTile);
    
                        // BASIC SIDE WALLS
                        else if (leftCell is WallCell && rightCell is WallCell == false && topCell is WallCell && bottomCell is WallCell && diagoUpLeftCell is WallCell && diagoBottomLeftCell is WallCell)
                            wallCells.Add(m_wallLeftTile);
                        else if (leftCell is WallCell == false && rightCell is WallCell && topCell is WallCell && bottomCell is WallCell && diagoUpRightCell is WallCell && diagoBottomRightCell is WallCell)
                            wallCells.Add(m_wallRightTile);
                        else if (leftCell is WallCell && rightCell is WallCell && topCell is WallCell == false && bottomCell is WallCell && diagoBottomLeftCell is WallCell && diagoBottomRightCell is WallCell)
                            wallCells.Add(m_wallDownTile);
                        else if (leftCell is WallCell && rightCell is WallCell && topCell is WallCell && bottomCell is WallCell == false && diagoUpLeftCell is WallCell && diagoUpRightCell is WallCell)
                            wallCells.Add(m_wallUpTile);

                        // BASIC DOUBLE WALLS
                        else if (leftCell is WallCell && rightCell is WallCell && topCell is WallCell == false && bottomCell is WallCell == false)
                            wallCells.Add(m_wallHorizontalTile);
                        else if (leftCell is WallCell == false && rightCell is WallCell == false && topCell is WallCell && bottomCell is WallCell)
                            wallCells.Add(m_wallVerticalTile);

                        // BASIC OUTSIDE CORNERS
                        else if (leftCell is WallCell == false && rightCell is WallCell && topCell is WallCell == false && bottomCell is WallCell && diagoUpLeftCell is WallCell == false && diagoBottomRightCell is WallCell)
                            wallCells.Add(m_cornerOutsideUpLeftTile);
                        else if (leftCell is WallCell && rightCell is WallCell == false && topCell is WallCell == false && bottomCell is WallCell && diagoUpRightCell is WallCell == false && diagoBottomLeftCell is WallCell)
                            wallCells.Add(m_cornerOutsideUpRightTile);
                        else if (leftCell is WallCell == false && rightCell is WallCell && topCell is WallCell && bottomCell is WallCell == false && diagoUpRightCell is WallCell && diagoBottomLeftCell is WallCell == false)
                            wallCells.Add(m_cornerOutsideDownLeftTile);
                        else if (leftCell is WallCell && rightCell is WallCell == false && topCell is WallCell && bottomCell is WallCell == false && diagoUpLeftCell is WallCell && diagoBottomRightCell is WallCell == false)
                            wallCells.Add(m_cornerOutsideDownRightTile);

                        // BASIC INSIDE CORNERS
                        else if (leftCell is WallCell && rightCell is WallCell && topCell is WallCell && bottomCell is WallCell && diagoUpLeftCell is WallCell && diagoUpRightCell is WallCell && diagoBottomLeftCell is WallCell && diagoBottomRightCell is WallCell == false)
                            wallCells.Add(m_cornerInsideUpLeftTile);
                        else if (leftCell is WallCell && rightCell is WallCell && topCell is WallCell && bottomCell is WallCell && diagoUpLeftCell is WallCell && diagoUpRightCell is WallCell && diagoBottomLeftCell is WallCell == false && diagoBottomRightCell is WallCell)
                            wallCells.Add(m_cornerInsideUpRightTile);
                        else if (leftCell is WallCell && rightCell is WallCell && topCell is WallCell && bottomCell is WallCell && diagoUpLeftCell is WallCell && diagoUpRightCell is WallCell == false && diagoBottomLeftCell is WallCell && diagoBottomRightCell is WallCell)
                            wallCells.Add(m_cornerInsideDownLeftTile);
                        else if (leftCell is WallCell && rightCell is WallCell && topCell is WallCell && bottomCell is WallCell && diagoUpLeftCell is WallCell == false && diagoUpRightCell is WallCell && diagoBottomLeftCell is WallCell && diagoBottomRightCell is WallCell)
                            wallCells.Add(m_cornerInsideDownRightTile);
                        
                        else
                            wallCells.Add(m_wallCenterTile);

                    }
                    else if (cell is FloorCell)
                    {
                        if (pos.x > xOneThird && pos.x < xTwoThird && pos.y > yOneThird && pos.y < yTwoThird)
                            floorCells.Add(m_floorCenterTile);
                        else if (pos.x < xHalf && pos.y < yHalf) 
                            floorCells.Add(m_floorBottomLeftTile);
                        else if (pos.x < xHalf && pos.y >= yHalf)
                            floorCells.Add(m_floorTopLeftTile);
                        else if (pos.x >= xHalf && pos.y < yHalf)
                            floorCells.Add(m_floorBottomRightTile);
                        else if (pos.x >= xHalf && pos.y >= yHalf)
                            floorCells.Add(m_floorTopRightTile);

                        floorPos.Add(pos);
                    }
                    else if (cell is DoorCell door)
                    {
                        doorCells.Add(m_defaultDoorTile);
                        doorPos.Add(pos);

                        if (pos.x > xOneThird && pos.x < xTwoThird && pos.y > yOneThird && pos.y < yTwoThird)
                            floorCells.Add(m_floorCenterTile);
                        else if (pos.x < xHalf && pos.y < yHalf)
                            floorCells.Add(m_floorBottomLeftTile);
                        else if (pos.x < xHalf && pos.y >= yHalf)
                            floorCells.Add(m_floorTopLeftTile);
                        else if (pos.x >= xHalf && pos.y < yHalf)
                            floorCells.Add(m_floorBottomRightTile);
                        else if (pos.x >= xHalf && pos.y >= yHalf)
                            floorCells.Add(m_floorTopRightTile);

                        floorPos.Add(pos);
                        AddToDoorList(door, pos);
                    }
                    else if (cell is DoorButtonCell)
                    {
                        doorButtonCells.Add(m_interactableDefaultTile);
                        doorButtonPos.Add(pos);
                    }
                    else if (cell is SectionButton)
                    {
                        doorButtonCells.Add(m_interactableDefaultTile);
                        doorButtonPos.Add(pos);
                    }
                    else if (cell is SectionDoorButtonCell)
                    {
                        if (cellInfos[x - 3, y].CellFromBytes() is SectionDoorButtonCell == false &&
                            cellInfos[x + 3, y].CellFromBytes() is SectionDoorButtonCell)
                        {
                            doorButtonCells.Add(m_interactableTopRightTile);
                            sectionButtonCounter++;
                        }

                        else if (cellInfos[x - 3, y].CellFromBytes() is SectionDoorButtonCell &&
                                 cellInfos[x + 3, y].CellFromBytes() is SectionDoorButtonCell && sectionButtonCounter == 1)
                        {
                            doorButtonCells.Add(m_interactableBottomRightTile);
                            sectionButtonCounter++;
                        }
                        else if (cellInfos[x - 3, y].CellFromBytes() is SectionDoorButtonCell &&
                                 cellInfos[x + 3, y].CellFromBytes() is SectionDoorButtonCell && sectionButtonCounter == 2)
                        {
                            doorButtonCells.Add(m_interactableBottomLeftTile);
                            sectionButtonCounter++;
                        }
                        else if (cellInfos[x - 3, y].CellFromBytes() is SectionDoorButtonCell &&
                                 cellInfos[x + 3, y].CellFromBytes() is SectionDoorButtonCell == false)
                        {
                            doorButtonCells.Add(m_interactableTopLeftTile);
                            sectionButtonCounter++;
                        }
                        else
                            doorButtonCells.Add(m_interactableDefaultTile);
                        doorButtonPos.Add(pos);
                    }
                    else if (cell is FinalButtonCell)
                    {
                        doorButtonCells.Add(m_interactableDefaultTile);
                        doorButtonPos.Add(pos);
                    }
                    else if (cell is PlayerSpawnCell)
                    {
                        playerSpawnCells.Add(m_defaultPlayerSpawnTile);
                        playerSpawnPos.Add(pos);
                    }
                }
            }


            

            m_walls.SetTiles(wallPos.ToArray(), wallCells.ToArray());
            m_floor.SetTiles(floorPos.ToArray(), floorCells.ToArray());
            m_doors.SetTiles(doorPos.ToArray(), doorCells.ToArray());
            m_interactable.SetTiles(doorButtonPos.ToArray(), doorButtonCells.ToArray());
            m_playerSpawn.SetTiles(playerSpawnPos.ToArray(), playerSpawnCells.ToArray());
        }

        private void AddToDoorList(DoorCell cell, Vector3Int pos)
        {
            switch (cell.DoorType)
            {
                case DoorType.Section_North:
                    DoorNorth.Add(new Vector2Int(pos.x, pos.y));
                    break;
                case DoorType.Section_East:
                    DoorEast.Add(new Vector2Int(pos.x, pos.y));
                    break;
                case DoorType.Section_South:
                    DoorSouth.Add(new Vector2Int(pos.x, pos.y));
                    break;
                case DoorType.Section_West:
                    DoorWest.Add(new Vector2Int(pos.x, pos.y));
                    break;
                case DoorType.Section0_NorthEast:
                    DoorSection0NorthEast.Add(new Vector2Int(pos.x, pos.y));
                    break;
                case DoorType.Section0_SouthEast:
                    DoorSection0SouthEast.Add(new Vector2Int(pos.x, pos.y));
                    break;
                case DoorType.Section0_SouthWest:
                    DoorSection0SouthWest.Add(new Vector2Int(pos.x, pos.y));
                    break;
                case DoorType.Section0_NorthWest:
                    DoorSection0NorthWest.Add(new Vector2Int(pos.x, pos.y));
                    break;
                case DoorType.FinalDoor:
                    FinalDoor.Add(new Vector2Int(pos.x, pos.y));
                    break;
                default:
                    break;
            }
        }

    }
}
