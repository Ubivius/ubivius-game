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
            // Add here the stuff you need to share
            public udp.SerializableTypes.Vector2 Position;
            public udp.SerializableTypes.Quaternion Rotation;
            public udp.SerializableTypes.Uint32 Tick;

            protected override void InitSerializableMembers()
            {
                Tick = new udp.SerializableTypes.Uint32(this, 0);
                Position = new udp.SerializableTypes.Vector2(this, Vector2.zero);
                Rotation = new udp.SerializableTypes.Quaternion(this, Quaternion.identity);
            }

            protected override byte SerializationID()
            {
                return (byte)Serialization.BYTE_TYPE.CLIENT_STATE;
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
                    m_updaters[i].ClientStoreAndStep(ref _this, input, deltaTime);
                }

                physics.Simulate(deltaTime);
            }

            /// <summary>
            /// Checks if any updater needs to correct its internal state
            /// </summary>
            /// <param name="remoteState">The state to compare to</param>
            /// <returns>Updaters needing to be corrected</returns>
            static public List<IClientStateUpdater> StatesNeedingCorrection(ClientState remoteState)
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