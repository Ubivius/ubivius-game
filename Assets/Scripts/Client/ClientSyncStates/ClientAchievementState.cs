using System.Collections;
using System.Collections.Generic;
using ubv.ui.client;
using UnityEngine;

namespace ubv.client.logic
{
    /// <summary>
    /// Represents the client state in the achievements menu
    /// </summary>
    public class ClientAchievementState : ClientSyncState
    {
        [SerializeField] ClientAchievementUI m_clientAchievementUI;

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
            AchievementService.Request(new ubv.microservices.GetAllAchievementsRequest(m_clientAchievementUI.CreateAchievement));
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
    }
}
