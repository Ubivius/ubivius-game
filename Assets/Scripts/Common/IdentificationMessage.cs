using UnityEngine;
using System.Collections;

namespace ubv
{
    namespace common
    {
        namespace data
        {
            public class IdentificationMessage : udp.Serializable
            {
                public udp.SerializableTypes.Int32 PlayerID;

                protected override void InitSerializableMembers()
                {
                    PlayerID = new udp.SerializableTypes.Int32(this, 0);
                }

                protected override byte SerializationID()
                {
                    return (byte)udp.Serialization.BYTE_TYPE.AUTH_MESSAGE;
                }
            }
        }
    }
}
