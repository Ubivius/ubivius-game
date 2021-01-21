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
            private udp.SerializableTypes.List<common.PlayerState> m_allPlayerStates;
            public udp.SerializableTypes.Uint32 Tick;
            
            protected override void InitSerializableMembers()
            {
                m_allPlayerStates = new udp.SerializableTypes.List<common.PlayerState>(this, new List<common.PlayerState>());
                Tick = new udp.SerializableTypes.Uint32(this, 0);
            }

            protected override byte SerializationID()
            {
                return (byte)udp.Serialization.BYTE_TYPE.CLIENT_STATE;
            }
            
            public common.PlayerState GetPlayer(uint playerID)
            {
                for(int i = 0; i < m_allPlayerStates.Value.Count; i++)
                {
                    if(playerID == m_allPlayerStates.Value[i].ID.Value)
                    {
                        return m_allPlayerStates.Value[i];
                    }
                }
                return null;
            }

            public void AddPlayer(common.PlayerState playerState)
            {
                // TODO: ensure a unique player ID
                m_allPlayerStates.Value.Add(playerState);
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
