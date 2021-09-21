using Agones;
using Agones.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ubv.http.agonesServer
{
    [RequireComponent(typeof(AgonesSdk))]
    public class HTTPAgonesServer : MonoBehaviour
    {
        private AgonesSdk m_agones = null;

        async void Start()
        {
            m_agones = gameObject.GetComponent<AgonesSdk>();
            bool ok = await m_agones.Connect();
            if (ok)
            {
                Debug.Log(("Agones Server - Connected"));
            }
            else
            {
                Debug.Log(("Agones Server - Failed to connect, exiting"));
                Application.Quit(1);
            }

            this.ReadyGameServer();
        }

        public async void ReadyGameServer()
        {
            bool ok = await m_agones.Ready();
            if (ok)
            {
                Debug.Log($"Agones Server - Ready");
            }
            else
            {
                Debug.Log($"Agones Server - Ready failed");
                Application.Quit();
            }
        }

        public async Task<GameServer> GetBackingGameServer()
        {
            var gameserver = await m_agones.GameServer();
            if (gameserver != null && gameserver.ObjectMeta != null)
            {
                Debug.Log(("Agones Server - Success to get backing game server"));
            }
            else 
            {
                Debug.Log(("Agones Server - Failed to get backing game server"));
            }
            return gameserver;

        }

        public async void ReserveGameServer(TimeSpan duration)
        {
            bool ok = await m_agones.Reserve(duration);
            if (ok)
            {
                Debug.Log($"Agones Server - Game server reserved for " + duration);
            }
            else
            {
                Debug.Log($"Agones Server - Failed to reserve game server");
            }
        }

        public async void ShutdownGameServer()
        {
            bool ok = await m_agones.Shutdown();
            if (ok)
            {
                Debug.Log($"Agones Server - Game server shutting down");
            }
            else
            {
                Debug.Log($"Agones Server - Failed to shutdown game server");
                Application.Quit();
            }
        }
        void OnDestroy()
        {
            Debug.Log("Agones Server - Close");
        }
    }
}
