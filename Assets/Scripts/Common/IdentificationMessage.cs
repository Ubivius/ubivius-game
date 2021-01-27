using UnityEngine;
using System.Collections;

namespace ubv.common.data
{
    public class IdentificationMessage : serialization.Serializable
    {
        public serialization.types.Int32 PlayerID;

        protected override void InitSerializableMembers()
        {
            PlayerID = new serialization.types.Int32(this, 0);
        }

        protected override byte SerializationID()
        {
            return (byte)serialization.ID.AUTH_MESSAGE;
        }
    }
}
