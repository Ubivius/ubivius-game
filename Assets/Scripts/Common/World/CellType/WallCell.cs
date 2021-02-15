using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.serialization;

namespace ubv.common.world.cellType
{
    class WallCell : LogicCell
    {
        public WallCell() : base()
        {
            IsWalkable = false;
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return  ID.BYTE_TYPE.LOGIC_CELL_WALL;
        }
    }
}
