using UnityEngine;
using System.Collections;

namespace ubv
{
    namespace common
    {
        namespace data
        {
            public class GameStartMessage : serialization.Serializable
            {
                public serialization.types.Int32 SimulationBuffer;
                public serialization.types.List<common.data.PlayerState> Players;

                protected override void InitSerializableMembers()
                {
                    Players = new serialization.types.List<PlayerState>(this, new System.Collections.Generic.List<PlayerState>());
                    SimulationBuffer = new serialization.types.Int32(this, 0);
                }

                protected override byte SerializationID()
                {
                    return (byte)serialization.ID.BYTE_TYPE.START_MESSAGE;
                }
            }
        }
    }
}
