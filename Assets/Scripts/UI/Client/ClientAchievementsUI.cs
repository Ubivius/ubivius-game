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


        protected override void Start()
        {
            base.Start();

            // EXAMPLE OF HOW TO ADD AN ACHIEVEMENT ON THE SCENE
            // Instantiate the prefab
            GameObject achievement_axample = Instantiate(m_achievementPrefab, m_content.transform);
            // Add the info on the prefab
            achievement_axample.GetComponent<Achievement>().InitializeAchievement("Enfin libre!", "S'échapper du vaisseau pour la première fois.", "023/10/2021");
            // If the achievement is unlocked, call this method.
            achievement_axample.GetComponent<Achievement>().Unlock();
        }

        public void Back()
        {
            //Go to game main menu scene
        }

    }
}
