using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ubv.common.gameplay
{
    [System.Serializable]
    public class PlayerItemInfo
    {
        public string Name;
        public string Desc;
        public Image UIIcon;
    }

    public abstract class PlayerItem : MonoBehaviour
    {
        [SerializeField] private PlayerItemInfo m_info;
        [SerializeField] private float m_activationCooldown;

        public bool ReadyToActivate { get; private set; }

        private float m_activationTimer;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(m_activationTimer < m_activationCooldown)
            {
                m_activationTimer += Time.deltaTime;
            }
            else
            {
                ReadyToActivate = true;
            }
        }

        public void Activate()
        {
            if (ReadyToActivate)
            {
                m_activationTimer = 0;
                ItemActivation();
            }
        }

        protected abstract void ItemActivation();
    }
}
