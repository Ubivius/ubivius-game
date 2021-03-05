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
    public class ClientSyncLoadWorld : ClientSyncState
    {
        [SerializeField] private world.WorldRebuilder m_worldRebuilder;

        private List<PlayerState> m_playerStates;
        private int? m_playerID;
        private int? m_simulationBuffer;

        protected override void StateAwake()
        {
            ClientSyncState.m_loadWorldState = this;
        }

        public void Init(common.world.cellType.CellInfo[,] cellInfos, int playerID, int simulationBuffer, List<PlayerState> playerStates)
        {
            m_worldRebuilder.OnWorldBuilt(SetupPlayState);

            m_playerID = playerID;
            m_simulationBuffer = simulationBuffer;
            m_playerStates = playerStates;
            m_worldRebuilder.BuildWorldFromCellInfo(cellInfos);
        }

        protected override void StateUpdate()
        {
            Debug.Log("World loaded at : " + GetWorldLoadProgress()*100 + "%");
        }

        public float GetWorldLoadProgress()
        {
            return m_worldRebuilder.GetWorldBuildProgress();
        }

        private void SetupPlayState()
        {
            ClientSyncState.m_playState.Init(m_playerID.Value, m_simulationBuffer.Value, m_playerStates);
            m_currentState = ClientSyncState.m_playState;
        }
    }  
}
