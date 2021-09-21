using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{
    public class RTTMessage : Serializable
    {
        public serialization.types.Int64 SentTimeStamp;
        public serialization.types.Int32 Tick;

        public RTTMessage()
        {
            SentTimeStamp = new serialization.types.Int64(0);
            Tick = new serialization.types.Int32(0);

            InitSerializableMembers(SentTimeStamp, Tick);
        }

        public RTTMessage(long time, int tick)
        {
            SentTimeStamp = new serialization.types.Int64(time);
            Tick = new serialization.types.Int32(tick);

            InitSerializableMembers(SentTimeStamp, Tick);
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.RTT_MSG;
        }
    }
}
