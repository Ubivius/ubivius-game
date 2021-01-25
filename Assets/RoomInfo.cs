using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv
{
    namespace common
    {
        public class RoomInfo : MonoBehaviour
        {

            [SerializeField] private int m_height;
            [SerializeField] private int m_width;

            Vector2Int GetDimension()
            {
                return new Vector2Int(m_width, m_height);
            }

        }
    }
    
}

