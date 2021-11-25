using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.data;
using ubv.server.logic;
using UnityEngine;

namespace ubv.server.logic
{
    public class EndGameState : ServerState
    {
        [SerializeField]
        private http.agonesServer.HTTPAgonesServer m_agones;
        private HashSet<int> m_activeClients;

        public void Init(List<int> playerList)
        {
            m_activeClients = new HashSet<int>(playerList);
            foreach (int id in playerList)
            {
                ServerEndsMessage msg = new ServerEndsMessage();
                msg.GameStats = GameplayState.PlayerStats[id];
                m_serverConnection.TCPServer.Send(msg.GetBytes(), id);
            }
            m_agones.ShutdownGameServer();
        }

        protected override void StateStart() 
        {
        }


        protected override void StateAwake()
        {
            ServerState.m_endGameState = this;
        }

        protected override void StateUpdate()
        {
        }

        protected override void OnPlayerConnect(int playerID)
        {
            
        }

        protected override void OnPlayerDisconnect(int playerID)
        {
            
        }
    }
}
