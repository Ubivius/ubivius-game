using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubv.common.world.cellType
{
    class VoidCell : LogicCell
    {
        public VoidCell()
        {
            IsWalkable = false;
        }

        public override CellInfo.CellType GetCellType()
        {
            return CellInfo.CellType.CELL_NONE;
        }
        
        protected override serialization.ID.BYTE_TYPE SerializationID()
        {
            return serialization.ID.BYTE_TYPE.LOGIC_CELL_VOID;
        }
    }
}
