using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.common.world
{
    public class WorldGeneraterCall : MonoBehaviour
    {
        [SerializeField] private WorldGenerator m_generator;
        // Start is called before the first frame update
        void OnEnable()
        {
            //m_generator.GenerateWorld();     // full map
            m_generator.GenerateWithOneRoom(); // one room
        }
    }
}