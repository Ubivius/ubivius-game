using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.client.logic
{
    /// <summary>
    /// Represents the client state in the achievements menu
    /// </summary>
    public class ClientAchievementState : ClientSyncState
    {
        [SerializeField] ubv.microservices.AchievementService m_achievementService;
        [SerializeField] ubv.ui.client.ClientAchievementUI m_clientAchievementUI;

        protected override void Awake()
        {
            base.Awake();
            m_canBack = true;
        } 

        protected override void StateLoad()
        {
            GetAllAchievements();
        }

        public override void StateUpdate()
        {
        }

        public void GetAllAchievements()
        {
            m_achievementService.Request(new ubv.microservices.GetAllAchievementsRequest(m_clientAchievementUI.CreateAchievement));
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
    }
}
