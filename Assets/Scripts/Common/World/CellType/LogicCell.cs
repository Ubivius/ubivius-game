using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.serialization;

namespace ubv.common.world.cellType
{
    public class CellInfo : serialization.Serializable
    {
        private serialization.types.Int32 m_cellType;

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.LOGIC_CELL_INFO;
        }
    }

    abstract public class LogicCell : serialization.Serializable
    {
        private serialization.types.Bool m_isWalkable;

        public LogicCell()
        {
            m_isWalkable = new serialization.types.Bool(false);
            InitSerializableMembers(m_isWalkable);
        }
        

        protected override ID.BYTE_TYPE SerializationID()
        {
            return  ID.BYTE_TYPE.LOGIC_CELL;
        }

        public bool IsWalkable { get => m_isWalkable.Value; protected set => m_isWalkable.Value = value; }
    }
}
