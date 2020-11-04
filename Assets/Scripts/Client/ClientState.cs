using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ubv
{
    /// <summary>
    /// Class reprensenting local client state, which will be simulated locally and synced with an authoritative server.
    /// Add here everything that needs to be shared with the server (and the other players).
    /// </summary>
    public class ClientState
    {
        // Add here the stuff you need to share
        public Vector2 Position;
        public Quaternion Rotation;
        public uint Tick;

        // add your data in the XYZBytes() functions to make them "network-able"
        // TODO: cache byte arrays to avoid GC.Alloc
        public byte[] ToBytes()
        {
            byte[] positionBytes_x = System.BitConverter.GetBytes(Position.x);
            byte[] positionBytes_y = System.BitConverter.GetBytes(Position.y);

            byte[] rotationBytes_w = System.BitConverter.GetBytes(Rotation.w);
            byte[] rotationBytes_x = System.BitConverter.GetBytes(Rotation.x);
            byte[] rotationBytes_y = System.BitConverter.GetBytes(Rotation.y);
            byte[] rotationBytes_z = System.BitConverter.GetBytes(Rotation.z);

            byte[] tickBytes = System.BitConverter.GetBytes(Tick);

            byte[] arr = new byte[1 + 4 + 4 + 4 + 4 + 4 + 4 + 4];
            
            arr[0] = (byte)Serialization.BYTE_TYPE.CLIENT_STATE;

            for (ushort i = 0; i < 4; i++)
            {
                arr[i + 1] = positionBytes_x[i];
                arr[i + 4 + 1] = positionBytes_y[i];
                arr[i + 8 + 1] = rotationBytes_w[i];
                arr[i + 12 + 1] = rotationBytes_x[i];
                arr[i + 16 + 1] = rotationBytes_y[i];
                arr[i + 20 + 1] = rotationBytes_z[i];
                arr[i + 24 + 1] = tickBytes[i];
            }


            return arr;
        }

        public static ClientState FromBytes(byte[] arr)
        {
            if (arr[0] != (byte)Serialization.BYTE_TYPE.CLIENT_STATE)
                return null;

            ClientState state = new ClientState
            {
                Position = new Vector2(System.BitConverter.ToSingle(arr, 1),
                System.BitConverter.ToSingle(arr, 4 + 1)),

                Rotation = new Quaternion(System.BitConverter.ToSingle(arr, 12 + 1),
                System.BitConverter.ToSingle(arr, 16 + 1),
                System.BitConverter.ToSingle(arr, 20 + 1),
                System.BitConverter.ToSingle(arr, 8 + 1)),

                Tick = System.BitConverter.ToUInt32(arr, 24 + 1)
            };

            return state;
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

        static public void Step(ref ClientState state, InputFrame input, float deltaTime, ref PhysicsScene2D physics)
        {
            for (int i = 0; i < m_updaters.Count; i++)
            {
                m_updaters[i].ClientStep(ref state, input, deltaTime);
            }

            physics.Simulate(deltaTime);

            for (int i = 0; i < m_updaters.Count; i++)
            {
                m_updaters[i].SaveClientState(ref state);
            }
        }

        static public bool NeedsCorrection(ref ClientState localState, ClientState remoteState)
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