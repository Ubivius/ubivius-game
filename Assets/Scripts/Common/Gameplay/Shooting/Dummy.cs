using System.Collections;
using UnityEngine;

namespace ubv.common.gameplay.shooting
{
    public class Dummy : MonoBehaviour
    {
        void Awake()
        {
            Hittable hit = GetComponent<Hittable>();
            hit.OnHit += Hit;
        }

        private void Hit()
        {
            Debug.Log("OUCH CRISS!");
        }
    }
}