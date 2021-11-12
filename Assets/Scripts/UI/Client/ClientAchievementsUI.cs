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
    public class ClientAchievementsUI : TabBehaviour
    {
        [SerializeField] private GameObject m_achievementPrefab;
        [SerializeField] private GameObject m_content;

        private List<Achievement> m_achievements;


        private void Awake()
        {
            m_achievements = new List<Achievement>();
        }

        protected override void Start()
        {
            base.Start();

            GameObject test_achie = Instantiate(m_achievementPrefab, m_content.transform);
            test_achie.GetComponent<Achievement>().InitializeAchievement("Get Good", "HAHAHAHAHAHA HAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHA HAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHA HAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHAHA HAHAHAHAHAHAHAHAHAHAHAHA You're slowly making it there!", "01/23/2021", null);
            test_achie.GetComponent<Achievement>().Unlock();

            GameObject test_achie2 = Instantiate(m_achievementPrefab, m_content.transform);
            test_achie2.GetComponent<Achievement>().InitializeAchievement("You're so fking bad", "Read the title.", "02/23/1999", null);
            test_achie2.GetComponent<Achievement>().Unlock();

            GameObject test_achie3 = Instantiate(m_achievementPrefab, m_content.transform);
            test_achie3.GetComponent<Achievement>().InitializeAchievement("Hehe", "test", "01/23/2069", null);
        }

        protected override void Update()
        {
            base.Update();
        }

        public void Back()
        {
            //Go to game main menu scene
        }

    }
}
