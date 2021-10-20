using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{
    public class ServerRejoinGameDemand : Serializable
    {
        public ServerRejoinGameDemand()
        {
            InitSerializableMembers();
        }
        
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.SERVER_REJOIN_GAME_DEMAND;
        }
    }
}
