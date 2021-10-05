using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{ 
    public class ClientReadyMessage : Serializable
    {
        public ClientReadyMessage()
        {
            InitSerializableMembers();
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.CLIENT_READY_MESSAGE;
        }
    }
}
