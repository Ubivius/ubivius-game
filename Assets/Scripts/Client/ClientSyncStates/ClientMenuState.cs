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
        [SerializeField] private string m_clientMyCharatcers;
        [SerializeField] private string m_clientAchievements;
        [SerializeField] private EventSystem m_eventSystem;
        
        public override void OnStart()
        {
            data.ClientCacheData cache = data.ClientCacheData.LoadCache();
            if (cache != null)
            {
                if ((DateTime.UtcNow - cache.LastUpdated).TotalSeconds > 1200)
                {
                    data.ClientCacheData.SaveCache(string.Empty);
                }
                else if (cache.LastGameID != null && !cache.LastGameID.Equals(string.Empty))
                {
                    data.LoadingData.GameID = cache.LastGameID;
                    // for now, auto rejoin
                    RejoinGame();
                }
            }
        }

        public void RejoinGame()
        {
            ClientStateManager.Instance.PushScene(m_clientGameSearch);
        }

        public void GoToPlay()
        {
            ClientStateManager.Instance.PushScene(m_clientCharacterSelect);
        }

        public void GoToMyCharacters()
        {
            ClientStateManager.Instance.PushScene(m_clientMyCharatcers);
        }

        public void GoToMyAchievements()
        {
            ClientStateManager.Instance.PushScene(m_clientAchievements);
        }

        protected override void StateLoad()
        { }

        protected override void StatePause()
        { }

        protected override void StateResume()
        {
            m_eventSystem.UpdateModules();
        }

        protected override void StateUnload()
        { }
    }
}
