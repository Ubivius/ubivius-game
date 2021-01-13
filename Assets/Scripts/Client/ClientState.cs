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
        public class ClientState : Serializable
        {
            // Add here the stuff you need to share
            public SerializableTypes.Vector2 Position;
            public SerializableTypes.Quaternion Rotation;
            public SerializableTypes.Uint32 Tick;

            protected override void InitSerializableMembers()
            {
                Tick = new SerializableTypes.Uint32(this, 0);
                Position = new SerializableTypes.Vector2(this, Vector2.zero);
                Rotation = new SerializableTypes.Quaternion(this, Quaternion.identity);
            }

            protected override byte SerializationID()
            {
                return (byte)Serialization.BYTE_TYPE.CLIENT_STATE;
            }

            #region UTILITY FUNCTIONS
            private static List<IClientStateUpdater> m_updaters = new List<IClientStateUpdater>();
            private static List<IPacketReceiver> m_receivers = new List<IPacketReceiver>();

            static public void RegisterReceiver(IPacketReceiver receiver)
            {
                m_receivers.Add(receiver);
            }

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

            static public bool NeedsCorrection(ClientState localState, ClientState remoteState)
            {
                bool needed = false;
                return needed;
            }

            static public void Receive(UDPToolkit.Packet packet)
            {
                for (int i = 0; i < m_receivers.Count; i++)
                {
                    m_receivers[i].ReceivePacket(packet);
                }
            }

            #endregion
        }
    }
}