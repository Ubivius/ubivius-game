using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.common.gameplay
{
    public class PlayerSuit : MonoBehaviour
    {
        [SerializeField] protected PlayerItem m_mainItem;
        [SerializeField] protected PlayerItem m_sideItem;

        public PlayerStats Stats;

        private void Awake()
        {
            Stats.Init();
        }

        // Start is called before the first frame update
        void Start()
        {
            m_mainItem = GameObject.Instantiate(m_mainItem, transform).GetComponent<PlayerItem>();
            m_sideItem = GameObject.Instantiate(m_sideItem, transform).GetComponent<PlayerItem>();
        }
        
        public void ActivateMainItem()
        {
            m_mainItem.Activate();
        }

        public void ActivateSideItem()
        {
            m_sideItem.Activate();
        }
    }
}
