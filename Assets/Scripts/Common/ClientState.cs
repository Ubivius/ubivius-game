using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ubv.common.serialization.types;
using ubv.common.serialization;

namespace ubv.common
{
    /// <summary>
    /// Class reprensenting local client state, which will be simulated locally and synced with an authoritative server.
    /// Add here everything that needs to be shared with the server (and the other players).
    /// </summary>
    public class ClientState : Serializable
    {
        public class PlayerHashMap : HashMap<common.data.PlayerState>
        {
            public PlayerHashMap(Dictionary<int, common.data.PlayerState> players) : base(players)
            { }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.HASHMAP_INT_PLAYERSTATE;
            }
        }

        private PlayerHashMap m_playerStates;

        public int PlayerGUID;

        public ClientState() : base()
        {
            m_playerStates = new PlayerHashMap(new Dictionary<int, common.data.PlayerState>());
            InitSerializableMembers(m_playerStates);
        }

        public ClientState(System.Collections.Generic.List<common.data.PlayerState> playerStates) : base()
        {
            Dictionary<int, common.data.PlayerState> dictStates = new Dictionary<int, data.PlayerState>();
            foreach(data.PlayerState state in playerStates)
            {
                dictStates.Add(state.GUID.Value, state);
            }
            m_playerStates = new PlayerHashMap(dictStates);
            InitSerializableMembers(m_playerStates);
        }

        public ClientState(ref ClientState state) : base()
        {
            m_playerStates = new PlayerHashMap(new Dictionary<int, common.data.PlayerState>());
            SetPlayers(state.m_playerStates.Value);
            InitSerializableMembers(m_playerStates);
        }

        public ClientState(int id) : base()
        {
            m_playerStates = new PlayerHashMap(new Dictionary<int, common.data.PlayerState>());
            PlayerGUID = id;
            InitSerializableMembers(m_playerStates);
        }
            
        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.CLIENT_STATE;
        }
            
        public data.PlayerState GetPlayer()
        {
            return m_playerStates.Value[PlayerGUID];
        }

        public void AddPlayer(data.PlayerState playerState)
        {
            m_playerStates.Value[playerState.GUID.Value] = playerState;
        }

        public void SetPlayers(Dictionary<int, common.data.PlayerState> playerStates)
        {
            m_playerStates.Value.Clear();
            foreach (common.data.PlayerState player in playerStates.Values)
            {
                m_playerStates.Value[player.GUID.Value] = new common.data.PlayerState(player);
            }
        }

        public Dictionary<int, common.data.PlayerState> Players()
        {
            return m_playerStates.Value;
        }
    }
}
