using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;
using System.Net.Http;

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
            //m_TCPClient.Subscribe(this);

#if NETWORK_SIMULATE
            m_clientSync.ConnectButtonEvent.AddListener(SendConnectionRequestToServer);
#endif // NETWORK_SIMULATE

        }

        public void SendConnectionRequestToServer()
        {
            int playerID = System.Guid.NewGuid().GetHashCode(); // for now
            m_HTTPClient.Get("dispatcher/" + playerID.ToString(), OnDispatcherResponse);
        }

        private void OnDispatcherResponse(HttpResponseMessage message)
        {
            if (message.StatusCode == HttpStatusCode.OK)
            {
                Debug.Log("Received from dispatcher : " + message.Content.ReadAsStringAsync());
                string address = "";
                int port = 0;
                m_TCPClient.Connect(address, port);

                m_TCPClient.Send(new IdentificationMessage(0).GetBytes()); // sends a ping to the server
            }
            else
            {
                Debug.Log("Dispatcher GET request was not successful");
            }
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
