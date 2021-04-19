using UnityEngine;
using System.Collections;


namespace ubv.utils
{
    public class Flag
    {
        private bool m_value;

        public Flag(bool defaultValue = false)
        {
            m_value = defaultValue;
        }

        public bool Read()
        {
            if (m_value)
            {
                m_value = false;
                return true;
            }
            return false;
        }

        public bool Peek()
        {
            return m_value;
        }

        public void Raise()
        {
            m_value = true;
        }
    }
}
