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
        private int? m_playerID;
        
        private void Awake()
        {
            ClientSyncState.InitState = this;

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
                m_playerID = auth.PlayerID.Value;
#if DEBUG_LOG
                Debug.Log("Received connection confirmation, player ID is " + m_playerID);
#endif // DEBUG_LOG

                // send a ping to the server to make it known that the player received its ID
                m_UDPClient.Send(UDPToolkit.Packet.PacketFromBytes(auth.GetBytes()).RawBytes);
            }
            else
            {
                // TODO : see why createfrombytes takes SO LONG
                GameInitMessage start = common.serialization.IConvertible.CreateFromBytes<GameInitMessage>(packet.Data);
                if (start != null)
                {
                    List<PlayerState> playerStates = start.Players.Value;
                    int simulationBuffer = start.SimulationBuffer.Value;
#if DEBUG_LOG
                    Debug.Log("Client received confirmation that server is about to start game with " + playerStates.Count + " players and " + simulationBuffer + " simulation buffer ticks");
#endif // DEBUG_LOG

                    m_TCPClient.Unsubscribe(this);
                    ClientSyncState.LoadWorldState.Init(start.CellInfo2DArray.Value, m_playerID.Value, simulationBuffer, playerStates);
                    CurrentState = ClientSyncState.LoadWorldState;
                }
            }
        }

        private void OnWorldBuilt()
        {
            m_TCPClient.Send(new GameReadyMessage(m_playerID.Value).GetBytes());
        }
    }   
}
