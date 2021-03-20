using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{ 
    public class ServerSuccessfulConnectMessage : Serializable
    {
        public ServerSuccessfulConnectMessage()
        {
            InitSerializableMembers();
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.SERVER_SUCCESSFUL_CONNECT_MESSAGE;
        }
    }
}
