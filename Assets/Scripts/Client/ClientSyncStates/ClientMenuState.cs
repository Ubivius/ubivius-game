using UnityEngine;
using System.Collections;
using ubv.client.logic;

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
                if (cache.IsInGame.Value)
                {
                    LoadingData.ServerInit = cache.CachedInitMessage;
                }
            }
        }

        public void RejoinGame()
        {
            if (LoadingData.ServerInit != null)
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
