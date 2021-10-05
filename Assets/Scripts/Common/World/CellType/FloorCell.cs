using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.serialization;

namespace ubv.common.world.cellType
{
    class FloorCell : LogicCell
    {
        // TODO: floor item and if there's one
        
        public FloorCell() : base()
        {
            IsWalkable = true;
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.LOGIC_CELL_FLOOR;
        }

        public override CellInfo.CellType GetCellType()
        {
            return CellInfo.CellType.CELL_FLOOR;
        }
    }
}
