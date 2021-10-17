using UnityEngine;
using System.Collections;
using ubv.client.logic;

namespace ubv.client
{
    public class ClientMenuState : ClientSyncState
    {
        [SerializeField] private string m_gameSearchScene;

        public void GoToPlay()
        {
            ClientStateManager.Instance.PushState(m_gameSearchScene);
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
