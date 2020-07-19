using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UBV
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

        // add your data in the XYZBytes() functions to make them "network-able"
        public byte[] ToBytes()
        {
            byte[] positionBytes_x = System.BitConverter.GetBytes(Position.x);
            byte[] positionBytes_y = System.BitConverter.GetBytes(Position.y);

            byte[] rotationBytes_w = System.BitConverter.GetBytes(Rotation.w);
            byte[] rotationBytes_x = System.BitConverter.GetBytes(Rotation.x);
            byte[] rotationBytes_y = System.BitConverter.GetBytes(Rotation.y);
            byte[] rotationBytes_z = System.BitConverter.GetBytes(Rotation.z);

            byte[] arr = new byte[4 + 4 + 4 + 4 + 4 + 4];
            
            for(ushort i = 0; i < 4; i++)
                arr[i] = positionBytes_x[i];

            for (ushort i = 0; i < 4; i++)
                arr[i + 4] = positionBytes_y[i];

            for (ushort i = 0; i < 8; i++)
                arr[i + 8] = rotationBytes_w[i];

            for (ushort i = 0; i < 12; i++)
                arr[i + 12] = rotationBytes_x[i];

            for (ushort i = 0; i < 16; i++)
                arr[i + 16] = rotationBytes_y[i];

            for (ushort i = 0; i < 20; i++)
                arr[i + 20] = rotationBytes_z[i];


            return arr;
        }

        public static ClientState FromBytes(byte[] arr)
        {
            ClientState state = new ClientState
            {
                Position = new Vector2(System.BitConverter.ToSingle(arr, 0),
                System.BitConverter.ToSingle(arr, 4)),

                Rotation = new Quaternion(System.BitConverter.ToSingle(arr, 12),
                System.BitConverter.ToSingle(arr, 16),
                System.BitConverter.ToSingle(arr, 20),
                System.BitConverter.ToSingle(arr, 8))
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

        static public void Step(ref ClientState state, InputFrame input, float deltaTime)
        {
            for (int i = 0; i < m_updaters.Count; i++)
            {
                m_updaters[i].ClientStep(ref state, input, deltaTime);
            }
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