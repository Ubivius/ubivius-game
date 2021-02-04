using UnityEngine;
using System.Collections;

namespace ubv
{
    namespace common
    {
        namespace data
        {
            public class GameStartMessage : udp.Serializable
            {
                public udp.SerializableTypes.List<common.data.PlayerState> Players;

                protected override void InitSerializableMembers()
                {
                    Players = new udp.SerializableTypes.List<PlayerState>(this, new System.Collections.Generic.List<PlayerState>());
                }

                protected override byte SerializationID()
                {
                    return (byte)udp.Serialization.BYTE_TYPE.START_MESSAGE;
                }
            }
        }
    }
}
