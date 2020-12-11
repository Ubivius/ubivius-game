using System.Collections.Generic;

namespace ubv
{
    internal interface IConvertible
    {
        byte[] GetBytes();
        int GetByteCount(byte[] sourceBytes = null);
        void CreateFromBytes(byte[] sourceBytes);
    }

    public abstract class Serializable : IConvertible
    {
        /// <summary>
        /// Contains a value that can be converted to bytes and back
        /// The value is cached under the hood to avoir superfluous 
        /// computation.
        /// </summary>
        /// <typeparam name="T">The type of the value. Must be default-constructible.</typeparam>
        public abstract class Variable<T> : IConvertible where T : new() 
        {
            private bool m_dirty;
            protected T m_value;
            private byte[] m_cachedBytes;

            private Serializable m_owner;
            
            public T Value
            {
                get
                {
                    m_dirty = true;
                    return m_value;
                }
                set
                {
                    Set(value);
                }
            }

            public Variable(Serializable owner) : this(owner, new T()) { }

            public Variable(Serializable owner, T value)
            {
                m_owner = owner;
                m_owner.m_serializableMembers.Add(this);
                Set(value);
                m_cachedBytes = Bytes();
            }
            
            public void CreateFromBytes(byte[] bytes)
            {
                bool mustRebuild = m_cachedBytes.Length != bytes.Length;

                if (!mustRebuild)
                {
                    for (int i = 0; i < m_cachedBytes.Length; i++)
                    {
                        if (bytes[i] != m_cachedBytes[i])
                        {
                            mustRebuild = true;
                            break;
                        }
                    }
                }

                if (mustRebuild)
                {
                    m_cachedBytes = bytes;
                    Set(BuildFromBytes(bytes));
                }
            }
            
            public static implicit operator T(Variable<T> variable)
            {
                return variable.Value;
            }

            public virtual Variable<T> Set(T value)
            {
                m_owner.m_dirty = true;
                m_dirty = true;
                m_value = value;
                return this;
            }

            public int GetByteCount(byte[] sourceBytes = null)
            {
                return ByteCount(sourceBytes);
            }

            public byte[] GetBytes()
            {
                if (m_dirty)
                {
                    m_dirty = false;
                    m_cachedBytes = Bytes();
                }
                return m_cachedBytes;
            }

            protected abstract T BuildFromBytes(byte[] bytes);
            protected abstract int ByteCount(byte[] sourceBytes = null);
            protected abstract byte[] Bytes();
            
            // disable access to default and copy constructor
            private Variable() { }
            private Variable(Variable<T> other) { }
        }

        List<IConvertible> m_serializableMembers;

        private bool m_dirty;
        private byte[] m_bytes;
        
        public Serializable()
        {
            m_dirty = true;
            m_serializableMembers = new List<IConvertible>();
            m_bytes = null;
            InitSerializableMembers();
        }
        
        public int GetByteCount(byte[] sourceBytes = null)
        {
            if(m_dirty)
                GetBytes();
            return m_bytes.Length;
        }
        
        public byte[] GetBytes()
        {
            int totalByteCount = 0;
            foreach (IConvertible bc in m_serializableMembers)
            {
                totalByteCount += bc.GetByteCount();
            }

            m_bytes = new byte[totalByteCount + 1];
            
            int byteCount = 0;
            m_bytes[byteCount++] = SerializationID();
            
            foreach (IConvertible bc in m_serializableMembers)
            {
                byte[] memberBytes = bc.GetBytes();
                for (int i = 0; i < memberBytes.Length; i++)
                {
                    if(byteCount == m_bytes.Length)
                    {
                        int k = 0;
                    }
                    m_bytes[byteCount++] = memberBytes[i];
                }
            }

            m_dirty = false;
            
            return m_bytes;
        }
        
        public void CreateFromBytes(byte[] bytes)
        {
            int convertedBytes = 0;
            foreach(IConvertible ic in m_serializableMembers)
            {
                byte[] srcBytes = bytes.SubArray(convertedBytes, bytes.Length - convertedBytes);
                ic.CreateFromBytes(srcBytes.SubArray(0, ic.GetByteCount(srcBytes)));
                convertedBytes += ic.GetByteCount();
            }
            m_bytes = bytes;
        }

        protected abstract byte SerializationID();
        protected virtual void InitSerializableMembers() { }

        static public T FromBytes<T>(byte[] bytes) where T : Serializable, new()
        {
            T newObject = new T();

            // TODO find a way tonot waste memory by creating and destroying after
            // maybe a switch and a case "is" ?
            if(bytes[0] == newObject.SerializationID())
            {
                newObject.CreateFromBytes(bytes.SubArray(1, bytes.Length - 1));
            }
            else
            {
                newObject = null;
            }

            return newObject;
        } 
    }

    namespace SerializableTypes
    {
        public class Int32 : Serializable.Variable<int>
        {
            public Int32(Serializable owner) : base(owner) { }

            protected override byte[] Bytes()
            {
                return System.BitConverter.GetBytes(m_value);
            }

            protected override int ByteCount(byte[] sourceBytes = null)
            {
                return sizeof(int);
            }

            protected override int BuildFromBytes(byte[] bytes)
            {
                return System.BitConverter.ToInt32(bytes, 0);
            }
        }
        
        public class Uint32 : Serializable.Variable<uint>
        {
            public Uint32(Serializable owner) : base(owner) { } 

            protected override byte[] Bytes()
            {
                return System.BitConverter.GetBytes(m_value);
            }

            protected override int ByteCount(byte[] sourceBytes = null)
            {
                return sizeof(uint);
            }

            protected override uint BuildFromBytes(byte[] bytes)
            {
                return System.BitConverter.ToUInt32(bytes, 0);
            }
        }

        public class Bool : Serializable.Variable<bool>
        {
            public Bool(Serializable owner) : base(owner) { }

            protected override byte[] Bytes()
            {
                return System.BitConverter.GetBytes(m_value);
            }

            protected override int ByteCount(byte[] sourceBytes = null)
            {
                return sizeof(bool);
            }

            protected override bool BuildFromBytes(byte[] bytes)
            {
                return System.BitConverter.ToBoolean(bytes, 0);
            }
        }
        
        public class Vector2 : Serializable.Variable<UnityEngine.Vector2>
        {
            public Vector2(Serializable owner) : base(owner) { }

            protected override byte[] Bytes()
            {
                byte[] bytes = new byte[sizeof(float) * 2];
                for (int i = 0; i < sizeof(float); i++)
                {
                    bytes[i] = System.BitConverter.GetBytes(m_value.x)[i];
                    bytes[i + sizeof(float)] = System.BitConverter.GetBytes(m_value.y)[i];
                }
                return bytes;
            }

            protected override int ByteCount(byte[] sourceBytes = null)
            {
                return sizeof(float) * 2;
            }

            protected override UnityEngine.Vector2 BuildFromBytes(byte[] bytes)
            {
                return new UnityEngine.Vector2(System.BitConverter.ToSingle(bytes, 0), System.BitConverter.ToSingle(bytes, 4));
            }
        }

        public class List<T> : Serializable.Variable<System.Collections.Generic.List<T>> where T : Serializable, new()
        {
            private int? m_frameCount = null;

            public override Serializable.Variable<System.Collections.Generic.List<T>> Set(System.Collections.Generic.List<T> value)
            {
                m_frameCount = value.Count;
                base.Set(value);
                return this;
            }

            public List(Serializable owner) : base(owner)
            {
                m_frameCount = null;
            }

            protected override System.Collections.Generic.List<T> BuildFromBytes(byte[] bytes)
            {
                m_frameCount = System.BitConverter.ToInt32(bytes, 0);

                System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();

                int byteCount = (bytes.Length - sizeof(int)) / m_frameCount.Value;
                for(int i = 0; i < m_frameCount; i++)
                {
                    T obj = Serializable.FromBytes<T>(bytes.SubArray(sizeof(int) + (i * byteCount), byteCount));
                    list.Add(obj);
                }
                return list;
            }

            protected override int ByteCount(byte[] sourceBytes = null)
            {
                if (sourceBytes != null)
                {
                    m_frameCount = System.BitConverter.ToInt32(sourceBytes.SubArray(0, sizeof(int)), 0);
                }

                // hack to check type byte count
                // TODO : enforce constant byte count 
                // OR separate byte count into two methods
                // (one to compute how many bytes are needed)
                // (one to return the actual byte count)
                int typeByteCount = 0;
                if(m_value.Count == 0)
                {
                    typeByteCount = new T().GetByteCount();
                }
                else
                {
                    typeByteCount = m_value[0].GetByteCount();
                }

                if (m_frameCount.Value > 0)
                {
                    return (m_frameCount.Value * typeByteCount) + sizeof(int);
                }
                return sizeof(int);
            }

            protected override byte[] Bytes()
            {
                byte[] bytes = new byte[ByteCount()];

                byte[] frameCountBytes = System.BitConverter.GetBytes(m_frameCount.Value);

                for (int i = 0; i < sizeof(int); i++)
                {
                    bytes[i] = frameCountBytes[i];
                }

                for (int i = 0; i < m_frameCount; i++)
                {
                    T obj = m_value[i];
                    byte[] objBytes = obj.GetBytes();
                    for(int b = 0; b < objBytes.Length; b++)
                    {
                        bytes[sizeof(int) + (i * objBytes.Length) + b] = objBytes[b];
                    }
                }
                return bytes;
            }
        }
    }
}
