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
    public class ClientSyncLoadWorld : ClientSyncState
    {
        [SerializeField] private world.WorldRebuilder m_worldRebuilder;

        private List<PlayerState> m_playerStates;
        private int? m_playerID;
        private int? m_simulationBuffer;

        private void Awake()
        {
            ClientSyncState.LoadWorldState = this;

            m_playerID = null;
            m_playerStates = null;

            m_worldRebuilder.OnWorldBuilt(SetupPlayState);
        }

        public void Init(common.world.cellType.CellInfo[,] cellInfos, int playerID, int simulationBuffer, List<PlayerState> playerStates)
        {
            m_playerID = playerID;
            m_simulationBuffer = simulationBuffer;
            m_playerStates = playerStates;
            m_worldRebuilder.BuildWorldFromCellInfo(cellInfos);
        }

        private void SetupPlayState()
        {
            ClientSyncState.PlayState.Init(m_playerID.Value, m_simulationBuffer.Value, m_playerStates);
            CurrentState = ClientSyncState.PlayState;
        }
    }  
}
