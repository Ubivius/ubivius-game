using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;
using UnityEngine.Events;

namespace ubv.client.logic
{
    public class ClientListUpdateEvent : UnityEvent<List<int>>  { }

    public class ClientSyncLobby : ClientSyncState, tcp.client.ITCPClientReceiver
    {
        private int? m_playerID; // TODO maybe : set it static/global because accessed by everything

        private GameInitMessage m_awaitedInitMessage;

        public ClientListUpdateEvent ClientListUpdate { get; private set; } 

        protected override void StateAwake()
        {
            ClientListUpdate = new ClientListUpdateEvent();
            DontDestroyOnLoad(this);
            m_awaitedInitMessage = null;
            ClientSyncState.m_lobbyState = this;
        }
        
        public void ReceivePacket(tcp.TCPToolkit.Packet packet)
        {
            // TODO : see why createfrombytes takes SO LONG
            GameInitMessage start = common.serialization.IConvertible.CreateFromBytes<GameInitMessage>(packet.Data);
            if (start != null)
            {
                m_awaitedInitMessage = start;
            }

            // loads other players in lobby, receives message from server indicating a new player joined

            ClientListMessage clientList = common.serialization.IConvertible.CreateFromBytes<ClientListMessage>(packet.Data);
            if (clientList != null)
            {
                List<PlayerState> playerStates = clientList.Players.Value;
                List<int> playerIDs = new List<int>();
                foreach(PlayerState state in playerStates)
                {
                    playerIDs.Add(state.GUID.Value);
                }

                ClientListUpdate.Invoke(playerIDs);
            }
        }

        protected override void StateUpdate()
        {
            if(m_awaitedInitMessage != null)
            {
                List<PlayerState> playerStates = m_awaitedInitMessage.Players.Value;
                int simulationBuffer = m_awaitedInitMessage.SimulationBuffer.Value;
#if DEBUG_LOG
                Debug.Log("Client received confirmation that server is about to start game with " + playerStates.Count + " players and " + simulationBuffer + " simulation buffer ticks");
#endif // DEBUG_LOG


                m_TCPClient.Unsubscribe(this);
                StartCoroutine(LoadLobbyCoroutine(m_awaitedInitMessage.CellInfo2DArray.Value, simulationBuffer, playerStates));
                m_awaitedInitMessage = null;
            }
        }

        public void Init(int playerID)
        {
            m_playerID = playerID;
            m_TCPClient.Subscribe(this);
            ClientListUpdate.Invoke(new List<int> { m_playerID.Value });
        }

        // TODO maybe later: add a reusable loading screen ?
        private IEnumerator LoadLobbyCoroutine(common.world.cellType.CellInfo[,] cellInfos, int simulationBuffer, List<PlayerState> playerStates)
        {
            AsyncOperation loadLobby = SceneManager.LoadSceneAsync("ClientGame");
            while (!loadLobby.isDone)
            {
                Debug.Log("Loading lobby : " + loadLobby.progress*100f + " %");
                yield return null;
            }

            ClientSyncState.m_currentState = ClientSyncState.m_loadWorldState;
            ClientSyncState.m_loadWorldState.Init(cellInfos, m_playerID.Value, simulationBuffer, playerStates);
        }
    }   
}
