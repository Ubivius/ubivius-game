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
        public class ClientState : udp.Serializable
        {
            private udp.SerializableTypes.HashMap<common.data.PlayerState> m_playerStates;
            public udp.SerializableTypes.Uint32 Tick;

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
                m_playerStates = new udp.SerializableTypes.HashMap<common.data.PlayerState>(this, new Dictionary<int, common.data.PlayerState>());
                Tick = new udp.SerializableTypes.Uint32(this, 0);
            }

            protected override byte SerializationID()
            {
                return (byte)udp.Serialization.BYTE_TYPE.CLIENT_STATE;
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

            public Dictionary<int, common.data.PlayerState>.ValueCollection Players()
            {
                return m_playerStates.Value.Values;
            }

#region UTILITY FUNCTIONS
            private static List<IClientStateUpdater> m_updaters = new List<IClientStateUpdater>();

            static public void RegisterUpdater(IClientStateUpdater updater)
            {
                m_updaters.Add(updater);
            }

            static public void SetToState(ClientState state)
            {
                for (int i = 0; i < m_updaters.Count; i++)
                {
                    m_updaters[i].UpdateFromState(state);
                }
            }

            public void StoreCurrentStateAndStep(common.data.InputFrame input, float deltaTime, ref PhysicsScene2D physics)
            {
                ClientState _this = this;

                for (int i = 0; i < m_updaters.Count; i++)
                {
                    m_updaters[i].SetStateAndStep(ref _this, input, deltaTime);
                }

                physics.Simulate(deltaTime);
            }

            /// <summary>
            /// Checks if any updater needs to correct its internal state
            /// </summary>
            /// <param name="remoteState">The state to compare to</param>
            /// <returns>Updaters needing to be corrected</returns>
            static public List<IClientStateUpdater> UpdatersNeedingCorrection(ClientState remoteState)
            {
                List<IClientStateUpdater> needCorrection = new List<IClientStateUpdater>();

                for (int i = 0; i < m_updaters.Count; i++)
                {
                    if (m_updaters[i].NeedsCorrection(remoteState))
                    {
                        needCorrection.Add(m_updaters[i]);
                    }
                }

                return needCorrection;
            }
#endregion
        }
    }
}
