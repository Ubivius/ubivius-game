﻿using UnityEngine;
using System.Collections;

namespace ubv
{
    public abstract class Serializable
    {
        public class Cachable<T> where T : new()
        {
            public T Value
            {
                get
                {
                    return m_value;
                }
                private set
                {
                    Set(value);
                }
            }

            private Serializable m_owner;
            private T m_value;

            private Cachable() { }

            public Cachable(Serializable owner)
            {
                m_value = new T();
                m_owner = owner;
            }

            public static implicit operator T(Cachable<T> cachable)
            {
                return cachable.m_value;
            }

            public void Set(T value)
            {
                m_owner.Dirty();
                m_value = value;
            }
        }

        protected byte[] m_bytes;
        private bool m_dirty;

        private void Dirty()
        {
            m_dirty = true;
        }

        public Serializable()
        {
            m_bytes = null;
            m_dirty = true;
        }

        public byte[] ToBytes(bool force = false)
        {
            if (m_dirty || force)
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
            if (m_bytes == null)
                ToBytes(true);
            return m_bytes.Length;
        }

        protected abstract byte SerializationID();
        protected abstract byte[] InternalToBytes();
        
    }
}
