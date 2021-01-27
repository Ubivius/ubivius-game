using UnityEngine;
using System.Collections;

namespace ubv.common.data
{
    public class AuthenticationMessage : serialization.Serializable
    {
        public serialization.types.Uint32 PlayerID;

        protected override void InitSerializableMembers()
        {
            PlayerID = new serialization.types.Uint32(this, 0);
        }

        protected override byte SerializationID()
        {
            return (byte)serialization.ID.AUTH_MESSAGE;
        }
    }
}
