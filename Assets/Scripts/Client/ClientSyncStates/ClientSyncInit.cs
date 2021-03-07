using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;

namespace ubv.client.logic
{
    public class ClientSyncInit : ClientSyncState, tcp.client.ITCPClientReceiver
    {
        protected override void StateAwake()
        {
            ClientSyncState.m_initState = this;
            ClientSyncState.m_currentState = this;
        }

        protected override void StateStart()
        {
            m_TCPClient.Subscribe(this);

#if NETWORK_SIMULATE
            m_clientSync.ConnectButtonEvent.AddListener(SendConnectionRequestToServer);
#endif // NETWORK_SIMULATE

        }

        public void SendConnectionRequestToServer()
        {
            m_TCPClient.Connect();

            m_TCPClient.Send(new IdentificationMessage(0).GetBytes()); // sends a ping to the server
        }

        public void ReceivePacket(tcp.TCPToolkit.Packet packet)
        {
            // receive auth message and set player id
            IdentificationMessage auth = common.serialization.IConvertible.CreateFromBytes<IdentificationMessage>(packet.Data);
            if (auth != null)
            {
                int playerID = auth.PlayerID.Value;
#if DEBUG_LOG
                Debug.Log("Received connection confirmation, player ID is " + playerID);
#endif // DEBUG_LOG

                // send a ping to the server to make it known that the player received its ID
                m_UDPClient.Send(UDPToolkit.Packet.PacketFromBytes(auth.GetBytes()).RawBytes);
                ClientSyncState.m_lobbyState.Init(playerID);
                ClientSyncState.m_currentState = ClientSyncState.m_lobbyState;
                m_TCPClient.Unsubscribe(this);
            }
        }
    }   
}
