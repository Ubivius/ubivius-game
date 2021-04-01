using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Tilemaps;

namespace ubv.common.world.dataStruct
{
    struct WolrdGeneratorToWallManager
    {
        public LogicGrid masterLogicGrid;
        public Tilemap wall;
        public TileBase tilewall;

        public WolrdGeneratorToWallManager(LogicGrid masterLogicGrid, Tilemap wall, TileBase tilewall)
        {
            this.masterLogicGrid = masterLogicGrid;
            this.wall = wall;
            this.tilewall = tilewall;
        }
    }
}
