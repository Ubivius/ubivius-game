using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ubv.common.data;
using ubv.tcp;
using System.Threading;

namespace ubv.client.logic
{
    public class ClientSyncLoadWorld : ClientSyncState, tcp.client.ITCPClientReceiver
    {
        [SerializeField] private world.WorldRebuilder m_worldRebuilder;

        private List<PlayerState> m_playerStates;
        private int? m_playerID;
        private int? m_simulationBuffer;

        private bool m_serverSentSignal;

        protected override void StateAwake()
        {
            m_serverSentSignal = false;
            ClientSyncState.m_loadWorldState = this;
        }

        public void Init(common.world.cellType.CellInfo[,] cellInfos, int playerID, int simulationBuffer, List<PlayerState> playerStates)
        {
            m_worldRebuilder.OnWorldBuilt(OnWorldBuilt);

            m_playerID = playerID;
            m_simulationBuffer = simulationBuffer;
            m_playerStates = playerStates;
            m_worldRebuilder.BuildWorldFromCellInfo(cellInfos);
            m_TCPClient.Subscribe(this);
        }

        protected override void StateUpdate()
        {
            float progress = GetWorldLoadProgress();
            // to print only every 10 increments
            if (((progress * 100) % 10) < 0.1)
            {
                Debug.Log("World loaded at : " + progress * 100 + "%");
            }
            if (m_serverSentSignal)
            {
                m_serverSentSignal = false;
                ClientSyncState.m_playState.Init(m_playerID.Value, m_simulationBuffer.Value, m_playerStates);
                m_currentState = ClientSyncState.m_playState;
                m_TCPClient.Unsubscribe(this);
            }
        }

        public float GetWorldLoadProgress()
        {
            return m_worldRebuilder.GetWorldBuildProgress();
        }

        private void OnWorldBuilt()
        {
            // TODO : modify code here for Client Ready Confirmation (UBI-350)
            GameReadyMessage ready = new GameReadyMessage(m_playerID.Value);
            m_TCPClient.Send(ready.GetBytes());
        }

        public void ReceivePacket(TCPToolkit.Packet packet)
        {
            GameReadyMessage message = common.serialization.Serializable.CreateFromBytes<GameReadyMessage>(packet.Data);
            if (message != null)
            {
                m_serverSentSignal = true;
            }
        }
    }  
}
