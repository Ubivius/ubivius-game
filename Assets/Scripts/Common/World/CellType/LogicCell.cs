using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubv.common.world.cellType
{
    abstract public class LogicCell : serialization.Serializable
    {
        private serialization.types.Bool m_isWalkable;

        protected override void InitSerializableMembers()
        {
            m_isWalkable = new serialization.types.Bool(this, false);
        }

        protected override byte SerializationID()
        {
            return (byte)serialization.ID.BYTE_TYPE.LOGIC_CELL;
        }

        public bool IsWalkable { get => m_isWalkable; protected set => m_isWalkable.Set(value); }
    }
}
