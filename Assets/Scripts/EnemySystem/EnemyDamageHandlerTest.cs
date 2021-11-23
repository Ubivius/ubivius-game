using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ubv.common.gameplay.shooting;
using UnityEngine;

namespace Assets.Scripts.EnemySystem
{
    class EnemyDamageHandlerTest: MonoBehaviour
    {
        private Hittable m_hittable;
        private void Start()
        {
            m_hittable = GetComponent<Hittable>();
        }

        private void Update()
        {
            m_hittable.OnHit?.Invoke();
        }
    }
}
