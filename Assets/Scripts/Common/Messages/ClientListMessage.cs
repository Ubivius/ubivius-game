using UnityEngine;
using System.Collections;
using ubv.common.serialization;
using System.Collections.Generic;

namespace ubv.common.data
{
    public class ClientListMessage : serialization.Serializable
    {
        public PlayerStateList Players { get; protected set; }

        public ClientListMessage()
        {
            Players = new PlayerStateList(null);

            InitSerializableMembers(Players);
        }

        public ClientListMessage(List<PlayerState> players) : base()
        {
            Players = new PlayerStateList(players);

            InitSerializableMembers(Players);
        }
                
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.CLIENT_LIST_MESSAGE;
        }
    }
}