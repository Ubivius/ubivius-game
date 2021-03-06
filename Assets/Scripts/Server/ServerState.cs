using UnityEngine;
using System.Collections;
using System.Net;
using ubv.udp;
using System.Collections.Generic;
using ubv.tcp;
using ubv.common.data;
using ubv.common.serialization;

namespace ubv.server.logic
{
    // TO READ https://fabiensanglard.net/quake3/network.php
    // and maybe https://thad.frogley.info/w/gfg08/gfg08.pdf

    abstract public class ServerState
    {
        protected readonly object m_lock = new object();
                
        public abstract ServerState Update();
        public abstract ServerState FixedUpdate();
    }
    
    public class ClientConnection
    {
        public client.ClientState State;

        public int PlayerGUID { get; private set; }

        public ClientConnection(int playerGUID)
        {
            State = new client.ClientState();
            State.PlayerGUID = playerGUID;
            PlayerGUID = playerGUID;
        }
    }
}
