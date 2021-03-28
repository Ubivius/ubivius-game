using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{
    public class ServerStartsMessage : Serializable
    {
        public ServerStartsMessage()
        {
            InitSerializableMembers();
        }
        
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.SERVER_STARTS_MESSAGE;
        }
    }
}
