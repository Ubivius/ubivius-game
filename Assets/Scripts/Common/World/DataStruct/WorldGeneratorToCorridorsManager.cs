using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ubv.common.world.dataStruct
{
    struct WorldGeneratorToCorridorsManager
    {
        public ubv.common.world.LogicGrid masterLogicGrid;
        public Tilemap floor;
        public TileBase tileFloor;
        public int wallThickness;

        public WorldGeneratorToCorridorsManager(LogicGrid masterLogicGrid, Tilemap floor, TileBase tileFloor, int wallThickness)
        {
            this.masterLogicGrid = masterLogicGrid;
            this.floor = floor;
            this.tileFloor = tileFloor;
            this.wallThickness = wallThickness;
        }
    }
}
