﻿using UnityEngine;
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
        // when dispatcher is ready
        // [SerializeField] EndPoint m_dispatcherEndpoint;

        [SerializeField] string m_serverTCPAddress;
        [SerializeField] int m_serverTCPPort;

        private struct JSONServerInfo
        {
            public string Address;
            public int Port;
        }

        private int? m_playerID;

        protected override void StateAwake()
        {
            ClientSyncState.m_initState = this;
            ClientSyncState.m_currentState = this;
        }
        
        public void SendConnectionRequestToServer()
        {
            int playerID = System.Guid.NewGuid().GetHashCode(); // for now
            m_playerID = playerID;

            // mock dispatcher response for now
            HttpResponseMessage msg = new HttpResponseMessage();
            string jsonString = JsonUtility.ToJson(new JSONServerInfo
            {
                Address = m_serverTCPAddress,
                Port = m_serverTCPPort
            }).ToString();
            msg.Content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
            msg.StatusCode = HttpStatusCode.OK;
            OnDispatcherResponse(msg);

            // uncomment when dispatcher ready
            // m_HTTPClient.Get("dispatcher/" + playerID.ToString(), OnDispatcherResponse);
        }

        private void OnDispatcherResponse(HttpResponseMessage message)
        {
            if (message.StatusCode == HttpStatusCode.OK)
            {
                string JSON = message.Content.ReadAsStringAsync().Result;
                JSONServerInfo serverInfo = JsonUtility.FromJson<JSONServerInfo>(JSON);
                Debug.Log("Received from dispatcher : " + JSON);
                string address = serverInfo.Address;
                int port = serverInfo.Port;

                m_TCPClient.Connect(address, port);
                m_TCPClient.Subscribe(this);
                m_TCPClient.Send(new IdentificationMessage(m_playerID.Value).GetBytes()); // sends a ping to the server
            }
            else
            {
                Debug.Log("Dispatcher GET request was not successful");
            }
        }

        public void ReceivePacket(tcp.TCPToolkit.Packet packet)
        {
            // receive auth message and set player id
            ServerConnectionInfoMessage udpInfo = common.serialization.IConvertible.CreateFromBytes<ServerConnectionInfoMessage>(packet.Data);
            if (udpInfo != null)
            {
#if DEBUG_LOG
                Debug.Log("Received TCP connection confirmation, sending UDP confirmation back to " + udpInfo.Address.Value + ":" + udpInfo.Port.Value.ToString() + ".");
#endif // DEBUG_LOG

                // send a ping to the server to make it known that the player received its ID
                IdentificationMessage identificationMessage = new IdentificationMessage(m_playerID.Value);

                m_UDPClient.SetTargetServer(udpInfo.Address.Value, udpInfo.Port.Value);

                // TODO : make sure server receives UDP ping
                m_UDPClient.Send(identificationMessage.GetBytes());
                ClientSyncState.m_lobbyState.Init(m_playerID.Value);
                ClientSyncState.m_currentState = ClientSyncState.m_lobbyState;
                m_TCPClient.Unsubscribe(this);
            }
        }
    }   
}
