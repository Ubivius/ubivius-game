using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.utils
{
    public class DestroyAfterTime : MonoBehaviour
    {
        [SerializeField] private float m_timeBeforeDeath;

        private void Awake()
        {
            StartCoroutine(DeathCoroutine());
        }

        private IEnumerator DeathCoroutine()
        {
            yield return new WaitForSeconds(m_timeBeforeDeath);
            Destroy(this.gameObject);
        }
    }
}