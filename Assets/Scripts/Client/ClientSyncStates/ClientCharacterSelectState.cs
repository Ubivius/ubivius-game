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

        protected override void Awake()
        {
            base.Awake();
            m_canBack = true;
        }

        protected override void StateLoad()
        {
        }

        public override void StateUpdate()
        {
        }

        public void GoToJoinGame()
        {
            ClientStateManager.Instance.PushScene(m_clientGameSearch);
        }

        protected override void StateUnload()
        {
        }

        protected override void StatePause()
        {
        }

        protected override void StateResume()
        {
        }

        public void GoBackToPreviousState()
        {
            ClientStateManager.Instance.PopState();
        }

        public void GoToLobby(string lobbyID)
        {
            data.LoadingData.GameID = lobbyID;
            GoToJoinGame();
        }
    }   
}
