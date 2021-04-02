using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Tilemaps;

namespace ubv.common.world.dataStruct
{
    struct WorldGeneratorToDoorManager
    {
        public ubv.common.world.LogicGrid masterLogicGrid;

        public Tilemap floor;
        public Tilemap door;
        public TileBase tileFloor;
        public TileBase tileDoor;
        public List<RoomInfo> roomInMap;

        public WorldGeneratorToDoorManager(LogicGrid masterLogicGrid, Tilemap floor, Tilemap door, TileBase tileFloor, TileBase tileDoor, List<RoomInfo> roomInMap)
        {
            this.masterLogicGrid = masterLogicGrid;
            this.floor = floor;
            this.door = door;
            this.tileFloor = tileFloor;
            this.tileDoor = tileDoor;
            this.roomInMap = roomInMap;
        }
    }
}
