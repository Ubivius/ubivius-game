using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ubv.client
{
    /// <summary>
    /// Class reprensenting local client state, which will be simulated locally and synced with an authoritative server.
    /// Add here everything that needs to be shared with the server (and the other players).
    /// </summary>
    public class ClientState : serialization.Serializable
    {
        private serialization.types.HashMap<common.data.PlayerState> m_playerStates;

        public serialization.types.Uint32 Tick;

        private int m_playerID;

        public void SetPlayerID(int id)
        {
            m_playerID = id;
        }

        public ClientState() : base() { }

        public ClientState(ref ClientState state) : base()
        {
            Tick.Set(state.Tick);
            SetPlayers(state.m_playerStates);
        }
            
        protected override void InitSerializableMembers()
        {
            m_playerStates = new serialization.types.HashMap<common.data.PlayerState>(this, new Dictionary<int, common.data.PlayerState>());
            Tick = new serialization.types.Uint32(this, 0);
        }

        protected override byte SerializationID()
        {
            return (byte)serialization.ID.CLIENT_STATE;
        }
            
        public common.data.PlayerState GetPlayer()
        {
            return m_playerStates.Value[m_playerID];
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
#endregion
    }
}

