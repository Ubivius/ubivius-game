using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace ubv.microservices
{
    public class MicroserviceAutoFetcher : MonoBehaviour
    {
        [SerializeField]
        private float m_timerOffset = 0f;
        [SerializeField]
        private float m_conversationFetchInterval = 3.0f;
        private float m_fetchTimer;
        private bool m_readyToFetch;

        public UnityAction FetchLogic;

        private void Awake()
        {
            m_fetchTimer = m_timerOffset;
            m_readyToFetch = true;
        }
        
        // Update is called once per frame
        void Update()
        {
            if (m_readyToFetch)
                m_fetchTimer += Time.deltaTime;

            if (m_fetchTimer >= m_conversationFetchInterval)
            {
                m_readyToFetch = false;
                m_fetchTimer = 0;
                FetchLogic.Invoke();
            }
        }

        public void ReadyForNewFetch()
        {
            m_readyToFetch = true;
        }
    }
}