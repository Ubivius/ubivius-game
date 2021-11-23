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


        protected override void Start()
        {
            base.Start();
        }

        public void Back()
        {
            //Go to game main menu scene
        }

        public void CreateAchievement(ubv.microservices.AchievementService.Achievement[] achievements)
        {
            foreach (ubv.microservices.AchievementService.Achievement achievement in achievements)
            {
                GameObject achievementInstance = Instantiate(m_achievementPrefab, m_content.transform);
                achievementInstance.GetComponent<Achievement>().InitializeAchievement(achievement.Name, achievement.Description, "image");
                achievementInstance.GetComponent<Achievement>().Unlock();
            }
        }
    }
}
