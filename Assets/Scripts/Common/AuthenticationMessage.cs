using UnityEngine;
using System.Collections;

namespace ubv
{
    namespace common
    {
        namespace data
        {
            public class AuthenticationMessage : udp.Serializable
            {
                public udp.SerializableTypes.Uint32 PlayerID;

                protected override void InitSerializableMembers()
                {
                    PlayerID = new udp.SerializableTypes.Uint32(this, 0);
                }

                protected override byte SerializationID()
                {
                    return (byte)udp.Serialization.BYTE_TYPE.AUTH_MESSAGE;
                }
            }
        }
    }
}
