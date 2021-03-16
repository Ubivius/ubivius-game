using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{ 
    public class ClientWorldLoadedMessage : Serializable
    {
        public serialization.types.Int32 PlayerID;
                
        public ClientWorldLoadedMessage()
        {
            PlayerID = new serialization.types.Int32(0);
            InitSerializableMembers(PlayerID);
        }

        public ClientWorldLoadedMessage(int playerID)
        {
            PlayerID = new serialization.types.Int32(playerID);
            InitSerializableMembers(PlayerID);
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.CLIENT_WORLD_LOADED_MESSAGE;
        }
    }
}
