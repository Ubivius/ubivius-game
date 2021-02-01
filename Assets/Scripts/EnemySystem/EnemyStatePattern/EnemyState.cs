using System.Collections;
using UnityEngine;

namespace ubv.server.logic.ai
{
    abstract public class EnemyState
    {
        // Use this for initialization
        public void Start()
        {

        }

        // Update is called once per frame
        public EnemyState Update()
        {
            return this;
        }
    }
}