using UnityEngine;
using System.Collections;

namespace ubv
{
    public abstract class Serializable
    {
        protected byte[] m_bytes;
        private bool m_dirty;

        public void Dirty()
        {
            m_dirty = true;
        }

        public Serializable()
        {
            m_bytes = null;
            m_dirty = true;
            ToBytes();
        }

        public byte[] ToBytes()
        {
            if (m_dirty)
            {
                byte[] bytes = InternalToBytes();
                if (m_bytes == null)
                    m_bytes = new byte[1 + bytes.Length];

                m_bytes[0] = SerializationID();
                for (int i = 0; i < bytes.Length; i++)
                {
                    m_bytes[i + 1] = bytes[i];
                }
                m_dirty = false;
            }

            return m_bytes;
        }

        public int ByteCount()
        {
            return m_bytes.Length;
        }

        protected abstract byte SerializationID();
        protected abstract byte[] InternalToBytes();
        
    }
}
