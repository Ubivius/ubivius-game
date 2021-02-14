using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ubv
{
    namespace client
    {
        /// <summary>
        /// Class reprensenting local client state, which will be simulated locally and synced with an authoritative server.
        /// Add here everything that needs to be shared with the server (and the other players).
        /// </summary>
        public class ClientState : common.serialization.Serializable
        {
            private common.serialization.types.HashMap<common.data.PlayerState> m_playerStates;
            public common.serialization.types.Uint32 Tick;

            public int PlayerGUID;

            public ClientState() : base() { }

            public ClientState(ref ClientState state) : base()
            {
                Tick.Set(state.Tick);
                SetPlayers(state.m_playerStates);
            }
            
            protected override void InitSerializableMembers()
            {
                m_playerStates = new common.serialization.types.HashMap<common.data.PlayerState>(this, new Dictionary<int, common.data.PlayerState>());
                Tick = new common.serialization.types.Uint32(this, 0);
            }

            protected override byte SerializationID()
            {
                return (byte)common.serialization.ID.BYTE_TYPE.CLIENT_STATE;
            }
            
            public common.data.PlayerState GetPlayer()
            {
                return m_playerStates.Value[PlayerGUID];
            }

            public void AddPlayer(common.data.PlayerState playerState)
            {
                m_playerStates.Value[playerState.GUID] = playerState;
            }

            public void SetPlayers(Dictionary<int, common.data.PlayerState> playerStates)
            {
                m_playerStates.Value.Clear();
                foreach (common.data.PlayerState player in playerStates.Values)
                {
                    m_playerStates.Value[player.GUID] = new common.data.PlayerState(player);
                }
            }

            public Dictionary<int, common.data.PlayerState> Players()
            {
                return m_playerStates.Value;
            }
        }
    }
}
