using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{ 
    public class ServerConnectionInfoMessage : Serializable
    {
        public serialization.types.String Address;
        public serialization.types.Int32 Port;
                
        public ServerConnectionInfoMessage()
        {
            Address = new serialization.types.String(string.Empty);
            Port = new serialization.types.Int32(0);
            InitSerializableMembers(Address, Port);
        }

        public ServerConnectionInfoMessage(string address, int port)
        {
            Address = new serialization.types.String(address);
            Port = new serialization.types.Int32(port);
            InitSerializableMembers(Address, Port);
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.SERVER_CONNECTION_INFO_MESSAGE;
        }
    }
}
