using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubv.common.world.cellType
{
    class FloorCell : LogicCell
    {
        // TODO: floor item and if there's one
        
        public FloorCell() : base()
        {
            IsWalkable = true;
        }

        protected override byte SerializationID()
        {
            return (byte)serialization.ID.BYTE_TYPE.LOGIC_CELL_FLOOR;
        }
    }
}
