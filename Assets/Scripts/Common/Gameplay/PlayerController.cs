using UnityEngine;
using System.Collections;

namespace ubv.common.gameplay
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerSuit m_currentSuit;

        [SerializeField] private PlayerStats m_defaultStats;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public PlayerStats GetStats()
        {
            return m_currentSuit ? m_currentSuit.Stats : m_defaultStats;
        }
    }
}