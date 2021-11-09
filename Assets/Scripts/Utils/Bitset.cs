using UnityEngine;
using System.Collections;
using ubv.common;
using ubv.common.serialization;

namespace ubv.utils
{
    public class Bitset : Serializable
    {
        private common.serialization.types.Int64 m_statesBitSet;

        public Bitset() : base()
        {
            m_statesBitSet = new common.serialization.types.Int64(0);
            InitSerializableMembers(m_statesBitSet);
        }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.BITSET;
        }
        
        public bool IsDifferent(Bitset bitset)
        {
            return m_statesBitSet.Value == bitset.m_statesBitSet.Value;
        }

        public bool IsTrue(int bitIndex)
        {
            return ((1 << bitIndex) & m_statesBitSet.Value) == (1 << bitIndex);
        }

        public void Set(int bitIndex, bool value)
        {
            if (value)
            {
                m_statesBitSet.Value |= ((uint)1 << bitIndex);
            }
            else
            {
                m_statesBitSet.Value &= ~((uint)1 << bitIndex);
            }
        }
    }
}
