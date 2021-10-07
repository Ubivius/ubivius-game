using UnityEngine;
using System.Collections;
using ubv.common.serialization;

namespace ubv.common.data
{
    public class ClientStateMessage : Serializable
    {
        public WorldState State;
        public NetInfo Info;

        public ClientStateMessage()
        {
            State = new WorldState();
            Info = new NetInfo();
            InitSerializableMembers(State, Info);
        }

        public ClientStateMessage(WorldState state, NetInfo info)
        {
            State = state;
            Info = info;

            InitSerializableMembers(State, Info);
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.CLIENT_STATE_MESSAGE;
        }
    }
}
