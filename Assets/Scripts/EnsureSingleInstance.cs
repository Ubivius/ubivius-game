using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.common
{
    public class EnsureSingleInstance : MonoBehaviour
    {
        private static EnsureSingleInstance m_inst;

        private void Awake()
        {
            if(m_inst == null)
            {
                m_inst = this;
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("Only one instance of " + gameObject.name + " may exist.");
#endif //DEBUG_LOG
                Destroy(this.gameObject);
            }
        }
    }
}