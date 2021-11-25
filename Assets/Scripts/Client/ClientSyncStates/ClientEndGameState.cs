using UnityEngine;
using System.Collections;
using ubv.client.logic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using ubv.common.data;

namespace ubv.client
{
    public class ClientEndGameState : ClientSyncState
    {
        [SerializeField] private string m_clientGameMenu;
        [SerializeField] private EventSystem m_eventSystem;

        public UnityAction<ServerPlayerGameStatsMessage, string> UpdateMenu;

        public override void OnStart()
        {
            data.ClientCacheData.SaveCache(string.Empty);
            data.LoadingData.GameChatID = string.Empty;
            data.LoadingData.GameID = string.Empty;
            CharacterService.GetCharacter(data.LoadingData.ActiveCharacterID, (character) => {
                UpdateMenu?.Invoke(data.LoadingData.GameStats, character.Name);
            });
            m_server.Disconnect();
        }

        public void GoToMenu()
        {
            ClientStateManager.Instance.BackToScene(m_clientGameMenu);
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
