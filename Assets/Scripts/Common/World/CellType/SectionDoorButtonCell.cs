using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.serialization;

namespace ubv.common.world.cellType
{
    class SectionDoorButtonCell : LogicCell
    {
        private List<DoorCell> m_linkedDoor = new List<DoorCell>();
        private serialization.types.List<Int32> m_linkedDoorCellID;

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.LOGIC_CELL_SECTIONDOORBUTTON;
        }

        public override CellInfo.CellType GetCellType()
        {
            return CellInfo.CellType.CELL_SECTIONDOORBUTTON;
        }
    }
}
