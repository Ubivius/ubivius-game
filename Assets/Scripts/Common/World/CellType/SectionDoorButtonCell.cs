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
        private IntList m_linkedDoorCellID;
        private Section m_section;

        public SectionDoorButtonCell(Section section)
        {
            m_section = section;
        }

        public void SetLinkedDoor(List<DoorCell> linkedDoor)
        {
            m_linkedDoor = linkedDoor;
            List<serialization.types.Int32> idList = new List<serialization.types.Int32>();
            foreach(DoorCell door in m_linkedDoor)
            {
                idList.Add(new serialization.types.Int32(door.GetCellID()));
            }
            m_linkedDoorCellID = new IntList(idList);
        }

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
