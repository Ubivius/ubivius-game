﻿using UnityEngine;
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
        private bool m_serverSentSignal;
        private int? m_playerID; // TODO maybe : set it static/global because accessed by everything

        private int? m_simulationBuffer;
        private List<PlayerState> m_playerStates;

        private ServerInitMessage m_awaitedInitMessage;

        public ClientListUpdateEvent ClientListUpdate { get; private set; }
        public UnityAction OnStartLoadWorld;
        public UnityAction OnGameStart;

        public float LoadPercentage { get; private set; }

        protected override void StateAwake()
        {
            ClientListUpdate = new ClientListUpdateEvent();
            m_awaitedInitMessage = null;
            ClientSyncState.m_lobbyState = this;
            m_serverSentSignal = false;
            LoadPercentage = 0;
        }

        private void OnWorldBuilt()
        {
            ClientWorldLoadedMessage worldLoaded = new ClientWorldLoadedMessage(m_playerID.Value);
            m_TCPClient.Send(worldLoaded.GetBytes());
        }
        
        public void SendReadyToServer()
        {
            ClientReadyMessage clientReadyMessage = new ClientReadyMessage(m_playerID.Value);
            m_TCPClient.Send(clientReadyMessage.GetBytes());
        }

        public void ReceivePacket(tcp.TCPToolkit.Packet packet)
        {
            ServerStartsMessage ready = common.serialization.IConvertible.CreateFromBytes<ServerStartsMessage>(packet.Data);
            if (ready != null)
            {
                m_serverSentSignal = true;
                return;
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
                return;
            }

            // TODO : see why createfrombytes takes SO LONG
            // meanwhile maybe start in thread
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            ServerInitMessage start = common.serialization.IConvertible.CreateFromBytes<ServerInitMessage>(packet.Data);
            watch.Stop();
            Debug.Log("Time elapsed for world deserialization : " + watch.ElapsedMilliseconds + " ms");
            if (start != null)
            {
                m_awaitedInitMessage = start;
                return;
            }
        }

        protected override void StateUpdate()
        {
            if(m_awaitedInitMessage != null)
            {
                m_playerStates = m_awaitedInitMessage.Players.Value;
                m_simulationBuffer = m_awaitedInitMessage.SimulationBuffer.Value;
#if DEBUG_LOG
                Debug.Log("Client received confirmation that server is about to start game with " + m_playerStates.Count + " players and " + m_simulationBuffer + " simulation buffer ticks");
#endif // DEBUG_LOG


                StartCoroutine(LoadWorldCoroutine(m_awaitedInitMessage.CellInfo2DArray.Value));
                m_awaitedInitMessage = null;
            }

            if (m_serverSentSignal)
            {
                OnGameStart?.Invoke();
                ClientSyncState.m_playState.Init(m_playerID.Value, m_simulationBuffer.Value, m_playerStates);
                m_currentState = ClientSyncState.m_playState;
                m_TCPClient.Unsubscribe(this);
                m_serverSentSignal = false;
            }
        }

        public void Init(int playerID)
        {
            m_playerID = playerID;
            m_TCPClient.Subscribe(this);
            ClientListUpdate.Invoke(new List<int> { m_playerID.Value });
        }
        
        private IEnumerator LoadWorldCoroutine(common.world.cellType.CellInfo[,] cellInfos)
        {
            OnStartLoadWorld?.Invoke();
            AsyncOperation loadLobby = SceneManager.LoadSceneAsync("ClientGame");
            while (!loadLobby.isDone)
            {
                LoadPercentage = loadLobby.progress * 0.25f;
                yield return null;
            }

            world.WorldRebuilder worldRebuilder = LoadingData.WorldRebuilder;
            worldRebuilder.OnWorldBuilt(OnWorldBuilt);
            worldRebuilder.BuildWorldFromCellInfo(cellInfos);
            while (!worldRebuilder.IsRebuilt)
            {
                LoadPercentage = 0.25f + (worldRebuilder.GetWorldBuildProgress() * 0.75f);
                yield return null;
            }
            
        }
    }   
}
