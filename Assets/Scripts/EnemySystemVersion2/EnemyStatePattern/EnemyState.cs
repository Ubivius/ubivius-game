using System.Collections;
using UnityEngine;

namespace Assets.EnemyAISystem.EnemyStatePattern
{
    public class EnemyState : MonoBehaviour
    {

        // Use this for initialization
        public void Start()
        {
            
        }

        // Update is called once per frame
        public EnemyState Update()
        {
            return new EnemyState();
        }
    }
}