using UnityEngine;
using System.Collections;
using ubv.client.logic;
using System;
using UnityEngine.EventSystems;

namespace ubv.client
{
    public class ClientEndGameState : ClientSyncState
    {
        [SerializeField] private string m_clientGameMenu;
        [SerializeField] private EventSystem m_eventSystem;
        
        public override void OnStart()
        { }

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
