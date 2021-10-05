using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ubv.common.world.dataStruct
{
    struct WorldGeneratorToDeadEndManager
    {
        public LogicGrid masterLogicGrid;
        public Tilemap floor;
        public Tilemap door;
        public List<Vector2Int> ends;

        public WorldGeneratorToDeadEndManager(LogicGrid masterLogicGrid, Tilemap floor, Tilemap door, List<Vector2Int> ends)
        {
            this.masterLogicGrid = masterLogicGrid;
            this.floor = floor;
            this.door = door;
            this.ends = ends;
        }
    }
}
