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
            ClientStateManager.Instance.PushScene(m_clientCharacterSelect);
        }

        private void Start()
        {
            data.ClientCacheData cache = data.ClientCacheData.LoadCache();
            if (cache != null)
            {
                if ((DateTime.UtcNow - cache.LastUpdated).TotalSeconds > 1200)
                {
                    data.ClientCacheData.SaveCache(false);
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
                ClientStateManager.Instance.PushScene(m_clientGameSearch);
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
