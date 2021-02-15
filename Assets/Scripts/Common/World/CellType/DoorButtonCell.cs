using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.serialization;

namespace ubv.common.world.cellType
{
    class DoorButtonCell : LogicCell
    {
        private ubv.common.world.cellType.DoorCell m_linkedDoor;
        private serialization.types.Int32 m_linkedDoorCellID;

        public DoorButtonCell(DoorCell linkedDoor) : base()
        {
            IsWalkable = false;
            m_linkedDoor = linkedDoor;
            m_linkedDoorCellID = new serialization.types.Int32(m_linkedDoor.CellID);
        }
        
        protected override ID.BYTE_TYPE SerializationID()
        {
            return  ID.BYTE_TYPE.LOGIC_CELL_INTERACTABLE;
        }

        public DoorButtonCell()
        {
            IsWalkable = false;
        }

        public void CloseDoor()
        {
            m_linkedDoor.CloseDoor();
        }

        public void OpenDoor()
        {
            m_linkedDoor.OpenDoor();
        }
    }
}
