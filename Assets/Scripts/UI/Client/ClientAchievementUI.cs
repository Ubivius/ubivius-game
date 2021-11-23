using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ubv.ui.client
{
    public class ClientAchievementUI : TabBehaviour
    {
        [SerializeField] private ubv.client.logic.ClientAchievementState m_achievementState;
        [SerializeField] private GameObject m_achievementPrefab;
        [SerializeField] private GameObject m_content;

        private List<microservices.AchievementService.Achievement> m_achievements;
        private bool m_achievementsLoaded;

        private void Awake()
        {
            m_achievementsLoaded = false;
            m_achievements = null;
        }

        protected override void Start()
        {
            base.Start();
        }

        public void Back()
        {
            m_achievementState.Back();
        }

        protected override void Update()
        {
            base.Update();
            if(!m_achievementsLoaded && m_achievements != null)
            {
                m_achievementsLoaded = true;
                foreach (ubv.microservices.AchievementService.Achievement achievement in m_achievements)
                {
                    GameObject achievementInstance = Instantiate(m_achievementPrefab, m_content.transform);
                    achievementInstance.GetComponent<Achievement>().InitializeAchievement(achievement.Name, achievement.Description, "image");
                    // if achievement is unlocked
                    achievementInstance.GetComponent<Achievement>().Unlock();
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_content.GetComponent<RectTransform>());
        }

        public void CreateAchievement(ubv.microservices.AchievementService.Achievement[] achievements)
        {
            m_achievements = new List<microservices.AchievementService.Achievement>(achievements);
        }
    }
}
