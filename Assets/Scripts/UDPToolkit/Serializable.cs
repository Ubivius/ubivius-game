using System.Collections.Generic;

namespace ubv
{
    namespace udp
    {
        internal interface IConvertible
        {
            byte[] GetBytes();
            int GetByteCount();
            void CreateFromBytes(byte[] sourceBytes);
        }

        /// <summary>
        /// Class containing a collection of serializable variables.
        /// To use, inherit Serializable and make the members
        /// you want to convert into bytes into one of the 
        /// serializable types defined in the namespace 
        /// SerializableTypes and implement the approriate functions
        /// (SerializationID and InitSerializedMembers)
        /// </summary>
        public abstract class Serializable : IConvertible
        {
            /// <summary>
            /// Contains a value that can be converted to bytes and back
            /// The value is cached under the hood to avoir superfluous 
            /// computation.
            /// </summary>
            /// <typeparam name="T">The type of the value. Must be default-constructible.</typeparam>
            public abstract class Variable<T> : IConvertible
            {
                protected T m_value;
                protected byte[] m_cachedBytes;

                private Serializable m_owner;

                public T Value
                {
                    get
                    {
                        return m_value;
                    }
                    set
                    {
                        Set(value);
                    }
                }

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
                    m_value = value;
                    return this;
                }

                public int GetByteCount()
                {
                    return ByteCount();
                }

                public byte[] GetBytes()
                {
                    m_cachedBytes = Bytes();
                    return m_cachedBytes;
                }

                protected abstract T BuildFromBytes(byte[] bytes);
                protected abstract int ByteCount();
                protected abstract byte[] Bytes();

                // disable access to default and copy constructor
                private Variable() { }
                private Variable(Variable<T> other) { }
            }

            private List<IConvertible> m_serializableMembers;

            private bool m_dirty;
            private byte[] m_bytes;

            public Serializable()
            {
                m_dirty = true;
                m_serializableMembers = new List<IConvertible>();
                m_bytes = null;
                InitSerializableMembers();
            }

            public int GetByteCount()
            {
                m_serializableMembers.Add(member);
            }

            public int GetByteCount()
            {
                if (m_dirty)
                    GetBytes();
                return m_bytes.Length;
            }

            public byte[] GetBytes()
            {
                if (m_dirty)
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
                            m_bytes[byteCount++] = memberBytes[i];
                        }
                    }

                    //m_dirty = false;
                }

                return m_bytes;
            }

            public void CreateFromBytes(byte[] bytes)
            {
                int convertedBytes = 0;
                foreach (IConvertible ic in m_serializableMembers)
                {
                    byte[] srcBytes = bytes.SubArray(convertedBytes, bytes.Length - convertedBytes);
                    ic.CreateFromBytes(srcBytes);
                    convertedBytes += ic.GetByteCount();
                }
                m_bytes = bytes;
            }

            protected abstract byte SerializationID();
            protected abstract void InitSerializableMembers();

            static public T FromBytes<T>(byte[] bytes) where T : Serializable, new()
            {
                T newObject = new T();

                // TODO find a way tonot waste memory by creating and destroying after
                // maybe a switch and a case "is" ?
                if (bytes[0] == newObject.SerializationID())
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
                public Int32(Serializable owner, int value) : base(owner, value) { }

                protected override byte[] Bytes()
                {
                    return System.BitConverter.GetBytes(m_value);
                }

                protected override int ByteCount()
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
                public Uint32(Serializable owner, uint value) : base(owner, value) { }

                protected override byte[] Bytes()
                {
                    return System.BitConverter.GetBytes(m_value);
                }

                protected override int ByteCount()
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
                public Bool(Serializable owner, bool value) : base(owner, value) { }

                protected override byte[] Bytes()
                {
                    return System.BitConverter.GetBytes(m_value);
                }

                protected override int ByteCount()
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
                public Vector2(Serializable owner, UnityEngine.Vector2 value) : base(owner, value) { }

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

                protected override int ByteCount()
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
                private int m_bytesPerElement;

                public override Serializable.Variable<System.Collections.Generic.List<T>> Set(System.Collections.Generic.List<T> value)
                {
                    m_bytesPerElement = new T().GetByteCount();
                    base.Set(value);
                    return this;
                }

                public List(Serializable owner, System.Collections.Generic.List<T> value) : base(owner, value)
                {
                    m_bytesPerElement = new T().GetByteCount();
                }
                
                protected override System.Collections.Generic.List<T> BuildFromBytes(byte[] bytes)
                {
                    int frameCount = System.BitConverter.ToInt32(bytes, 0);

                    System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();
                    
                    for (int i = 0; i < frameCount; i++)
                    {
                        T obj = Serializable.FromBytes<T>(bytes.SubArray(sizeof(int) + (i * m_bytesPerElement), m_bytesPerElement));
                        list.Add(obj);
                    }
                    return list;
                }

                protected override int ByteCount()
                {
                    return sizeof(int) + (m_value.Count * m_bytesPerElement);
                }

                protected override byte[] Bytes()
                {
                    byte[] bytes = new byte[ByteCount()];

                    byte[] frameCountBytes = System.BitConverter.GetBytes(m_value.Count);

                    for (int i = 0; i < sizeof(int); i++)
                    {
                        bytes[i] = frameCountBytes[i];
                    }

                    for (int i = 0; i < m_value.Count; i++)
                    {
                        T obj = m_value[i];
                        byte[] objBytes = obj.GetBytes();
                        for (int b = 0; b < objBytes.Length; b++)
                        {
                            bytes[sizeof(int) + (i * objBytes.Length) + b] = objBytes[b];
                        }
                    }
                    return bytes;
                }
            }

            public class String : Serializable.Variable<string>
            {
                public String(Serializable owner, string value) : base(owner, value) { }

                protected override byte[] Bytes()
                {
                    return System.Text.Encoding.Unicode.GetBytes(m_value);
                }

                protected override int ByteCount()
                {
                    return Bytes().Length;
                }

                protected override string BuildFromBytes(byte[] bytes)
                {
                    return System.Text.Encoding.Unicode.GetString(bytes);
                }
            }

            public class Quaternion : Serializable.Variable<UnityEngine.Quaternion>
            {
                public Quaternion(Serializable owner, UnityEngine.Quaternion value) : base(owner, value) { }

                protected override byte[] Bytes()
                {
                    byte[] bytes = new byte[sizeof(float) * 4];
                    for (int i = 0; i < sizeof(float); i++)
                    {
                        bytes[i] = System.BitConverter.GetBytes(m_value.x)[i];
                        bytes[i + sizeof(float)] = System.BitConverter.GetBytes(m_value.y)[i];
                        bytes[i + sizeof(float) * 2] = System.BitConverter.GetBytes(m_value.z)[i];
                        bytes[i + sizeof(float) * 3] = System.BitConverter.GetBytes(m_value.w)[i];
                    }
                    return bytes;
                }

                protected override int ByteCount()
                {
                    return sizeof(float) * 4;
                }

                protected override UnityEngine.Quaternion BuildFromBytes(byte[] bytes)
                {
                    return new UnityEngine.Quaternion(System.BitConverter.ToSingle(bytes, 0),
                        System.BitConverter.ToSingle(bytes, 4),
                        System.BitConverter.ToSingle(bytes, 8),
                        System.BitConverter.ToSingle(bytes, 12));
                }
            }
        }

    }
}
