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
            m_linkedDoorCellID = new serialization.types.Int32(m_linkedDoor.GetCellID());

            InitSerializableMembers(m_linkedDoorCellID);
        }
        
        protected override ID.BYTE_TYPE SerializationID()
        {
            return  ID.BYTE_TYPE.LOGIC_CELL_INTERACTABLE;
        }

        public override CellInfo.CellType GetCellType()
        {
            return CellInfo.CellType.CELL_BUTTON;
        }

        public DoorButtonCell()
        {
            IsWalkable = false;
            m_linkedDoorCellID = new serialization.types.Int32(m_linkedDoor != null ? m_linkedDoor.GetCellID() : 0);
            InitSerializableMembers(m_linkedDoorCellID);
        }

        public void SetLinkedDoor(DoorCell door)
        {
            m_linkedDoor = door;
        }

        public int GetLinkedDoorID()
        {
            return m_linkedDoorCellID.Value;
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
