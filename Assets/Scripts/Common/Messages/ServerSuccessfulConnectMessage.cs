using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{ 
    public class ServerSuccessfulTCPConnectMessage : Serializable
    {
        public ServerSuccessfulTCPConnectMessage()
        {
            InitSerializableMembers();
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.SERVER_SUCCESSFUL_TCP_CONNECT_MESSAGE;
        }
    }

    public class ServerSuccessfulUDPConnectMessage : Serializable
    {
        public ServerSuccessfulUDPConnectMessage()
        {
            InitSerializableMembers();
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.SERVER_SUCCESSFUL_UDP_CONNECT_MESSAGE;
        }
    }
}
