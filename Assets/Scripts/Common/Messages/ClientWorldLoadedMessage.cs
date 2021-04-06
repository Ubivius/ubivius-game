using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{ 
    public class ClientWorldLoadedMessage : Serializable
    {
        public ClientWorldLoadedMessage()
        {
            InitSerializableMembers();
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.CLIENT_WORLD_LOADED_MESSAGE;
        }
    }
}
