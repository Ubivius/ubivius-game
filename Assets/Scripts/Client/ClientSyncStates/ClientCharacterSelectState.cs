using UnityEngine;
using static ubv.microservices.CharacterDataService;
using ubv.microservices;
using ubv.ui.client;

namespace ubv.client.logic
{
    /// <summary>
    /// Represents the client state after he's logged in, and before he finds a lobby
    /// </summary>
    public class ClientCharacterSelectState : ClientSyncState
    {
        [SerializeField] private string m_clientGameSearch;
        [SerializeField] private AddCharacterUI m_addCharacterUI;
        private bool m_joiningGame;

        protected override void StateLoad()
        {
            m_joiningGame = false;
        }

        public override void StateUpdate()
        {
        }
        
        public void GoToJoinGame()
        {
            m_joiningGame = true;
            ClientStateManager.Instance.PushScene(m_clientGameSearch);
        }

        protected override void StateUnload()
        {
            m_joiningGame = false;
        }

        protected override void StatePause()
        {
        }

        protected override void StateResume()
        {
        }
    }   
}
