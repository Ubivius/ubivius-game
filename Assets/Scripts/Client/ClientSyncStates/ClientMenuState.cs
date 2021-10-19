using UnityEngine;
using System.Collections;
using ubv.client.logic;
using System;

namespace ubv.client
{
    public class ClientMenuState : ClientSyncState
    {
        [SerializeField] private string m_clientGameSearch;
        [SerializeField] private string m_clientCharacterSelect;

        public void GoToPlay()
        {
            ClientStateManager.Instance.PushState(m_clientCharacterSelect);
        }

        private void Start()
        {
            data.ClientCacheData cache = ClientStateManager.Instance.FileSaveManager.LoadFromFile<data.ClientCacheData>("cache.ubv");
            if (cache != null)
            {
                if ((DateTime.UtcNow - cache.LastUpdated).TotalSeconds > 1200)
                {
                    cache.WasInGame = false;
                    ClientStateManager.Instance.FileSaveManager.SaveFile(cache, "cache.ubv");
                }
                else if (cache.WasInGame)
                {
                    data.LoadingData.IsTryingToRejoinGame = cache.WasInGame;
                    // for now, auto rejoin
                    RejoinGame();
                }
            }
        }

        public void RejoinGame()
        {
            if (data.LoadingData.IsTryingToRejoinGame)
            {
                ClientStateManager.Instance.PushState(m_clientGameSearch);
            }
        }

        protected override void StateLoad()
        { }

        protected override void StatePause()
        { }

        protected override void StateResume()
        { }

        protected override void StateUnload()
        { }
    }
}
