using UnityEngine;
using System.Collections;
using ubv.client.logic;
using System.Collections.Generic;
using ubv.microservices;
using UnityEngine.UI;
using ubv.client;
using TMPro;
using ubv.common.data;

namespace ubv.ui.client
{
    public class ClientEndGameUI : TabBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_characterName;
        [SerializeField]
        private ClientEndGameState m_EndGameState;

        [SerializeField] private TextMeshProUGUI m_finalScore;
        [SerializeField] private TextMeshProUGUI m_collectiveKills;
        [SerializeField] private TextMeshProUGUI m_numberOfDowns;
        [SerializeField] private TextMeshProUGUI m_numberOfHelps;
        [SerializeField] private TextMeshProUGUI m_victory;
        [SerializeField] private TextMeshProUGUI m_gameDuration;

        [SerializeField] private Button m_menuButton;

        private void Awake()
        {
            m_EndGameState.UpdateMenu += UpdateStats;
        }

        public void Menu()
        {
            m_EndGameState.GoToMenu();
        }

        public void UpdateStats(ServerPlayerGameStatsMessage stats, string characterName)
        {
            m_characterName.text = characterName;
            m_finalScore.text = stats.PlayerScore.Value.ToString();
            m_collectiveKills.text = stats.NumberOfKills.Value.ToString();
            m_numberOfDowns.text = stats.NumberOfDowns.Value.ToString();
            m_victory.text = stats.Win.Value ? "VICTOIRE" : "DEFAITE";
            var duration = System.TimeSpan.FromSeconds(stats.GameDuration.Value);
            m_gameDuration.text = string.Format("{0}:{1:00}", (int)duration.TotalMinutes, duration.Seconds);
        }
    }
}
