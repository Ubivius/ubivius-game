using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{
    public class NetInfo : Serializable
    {
        public serialization.types.Int64 TimeStamp;
        public serialization.types.Int32 Tick;

        public NetInfo()
        {
            TimeStamp = new serialization.types.Int64(0);
            Tick = new serialization.types.Int32(0);

            InitSerializableMembers(TimeStamp, Tick);
        }

        public NetInfo(long time, int tick)
        {
            TimeStamp = new serialization.types.Int64(time);
            Tick = new serialization.types.Int32(tick);

            InitSerializableMembers(TimeStamp, Tick);
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.NET_INFO;
        }
    }
}