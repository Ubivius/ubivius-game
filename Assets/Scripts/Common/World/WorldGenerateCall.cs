using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.common.world
{
    public class WorldGenerateCall : MonoBehaviour
    {
        [SerializeField] private WorldGenerator m_generator;
        // Start is called before the first frame update
        void Start()
        {
            m_generator.GenerateWorld();
        }
    }
}
