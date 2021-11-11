using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using static ubv.microservices.CharacterDataService;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ubv.client.logic.ClientAchievementsState;
using UnityEngine.EventSystems;

namespace ubv.ui.client
{
    public class ClientAchievementsUI : TabBehaviour
    {
        [SerializeField] private ubv.client.logic.ClientAchievementsState m_initState;
        private EventSystem system;

        private void Awake()
        {
            system = EventSystem.current;
        }

        protected override void Update()
        {
            base.Update();
        }

        public void Back()
        {
            m_initState.Back();
        }

    }
}
