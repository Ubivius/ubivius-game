using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.data;
using ubv.server.logic;

namespace ubv.server.logic
{
    public class EndGameState : ServerState
    {
        public void init(List<int> playerList)
        {
            foreach (int id in playerList)
            {
                m_serverConnection.TCPServer.Send(new ServerEndsMessage().GetBytes(), id);
            }
        }

        protected override void StateStart() 
        {
            //m_serverConnection.TCPServer.Send();
        }


        protected override void StateAwake()
        {
            ServerState.m_endGameState = this;
        }

        protected override void OnPlayerConnect(int playerID)
        {
            
        }

        protected override void OnPlayerDisconnect(int playerID)
        {
            
        }
    }
}
