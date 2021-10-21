using UnityEngine;
using System.Collections;
using ubv.client.logic;
using System;
using UnityEngine.EventSystems;

namespace ubv.client
{
    public class ClientMenuState : ClientSyncState
    {
        [SerializeField] private string m_clientGameSearch;
        [SerializeField] private string m_clientCharacterSelect;
        [SerializeField] private BaseInputModule m_inputModule;
        
        public override void OnStart()
        {
            m_inputModule.ActivateModule();
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

        public void GoToPlay()
        {
            ClientStateManager.Instance.PushScene(m_clientCharacterSelect);
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
