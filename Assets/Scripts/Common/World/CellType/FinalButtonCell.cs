using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.serialization;

namespace ubv.common.world.cellType
{
    class FinalButtonCell : LogicCell
    {
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.LOGIC_CELL_FINALBUTTON;
        }

        public override CellInfo.CellType GetCellType()
        {
            return CellInfo.CellType.CELL_FINALBUTTON;
        }
    }
}
