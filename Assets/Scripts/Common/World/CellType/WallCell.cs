using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubv.common.world.cellType
{
    class WallCell : LogicCell
    {
        public WallCell()
        {
            IsWalkable = false;
        }

        protected override byte SerializationID()
        {
            return (byte)serialization.ID.BYTE_TYPE.LOGIC_CELL_WALL;
        }
    }
}
