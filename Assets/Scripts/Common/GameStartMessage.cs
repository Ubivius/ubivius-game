using UnityEngine;
using System.Collections;

namespace ubv.common.data
{
    public class GameStartMessage : serialization.Serializable
    {
        public serialization.types.List<PlayerState> Players;

        protected override void InitSerializableMembers()
        {
            Players = new serialization.types.List<PlayerState>(this, new System.Collections.Generic.List<PlayerState>());
        }

        protected override byte SerializationID()
        {
            return (byte)serialization.ID.START_MESSAGE;
        }
    }
}
