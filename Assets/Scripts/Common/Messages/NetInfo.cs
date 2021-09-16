using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{
    public class NetInfo : Serializable
    {
        public serialization.types.Int32 Tick;

        public NetInfo()
        {
            Tick = new serialization.types.Int32(0);

            InitSerializableMembers(Tick);
        }

        public NetInfo(int tick)
        {
            Tick = new serialization.types.Int32(tick);

            InitSerializableMembers(Tick);
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.NET_INFO;
        }
    }
}