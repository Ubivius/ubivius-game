using System;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.common.serialization
{
    public abstract class IConvertible
    {
        public byte[] GetBytes()
        {
            byte[] bytes = new byte[GetByteCount()];

            bytes[0] = (byte)SerializationID();
            
            byte[] sourceBytes = GetSourceBytes();

            for(int i = 1; i < bytes.Length; i++)
            {
                bytes[i] = sourceBytes[i - 1];
            }

            //Debug.Log("Created bytes : " + System.BitConverter.ToString(bytes)  + " in "+ this.SerializationID() + ", " + this.GetType().ToString());
            return bytes;
        }

        public int GetByteCount()
        {
            return GetSourceByteCount() + 1;
        }

        public bool CreateFromBytes(ArraySegment<byte> bytes)
        {
            if (bytes.At(0) == (byte)SerializationID())
            {
                if (CreateFromSourceBytes(bytes.ArraySegmentFrom(1)))
                {
                    return true;
                }
            }
            return false;
        }
        
        static public T CreateFromBytes<T>(ArraySegment<byte> bytes) where T : IConvertible, new()
        {
            T obj = new T();
            if (obj.CreateFromBytes(bytes))
            {
                return obj;
            }
            return null;
            
        }

        static public byte[] GetBytesFrom<T>(T convertible) where T : IConvertible 
        {
            return convertible.GetBytes();
        }
        
        protected abstract byte[] GetSourceBytes();
        protected abstract int GetSourceByteCount();
        protected abstract bool CreateFromSourceBytes(ArraySegment<byte> sourceBytes);
        protected abstract ID.BYTE_TYPE SerializationID();
    }

    /// <summary>
    /// Class containing a collection of serializable variables.
    /// To use, inherit Serializable and make the members
    /// you want to convert into bytes into one of the 
    /// serializable types defined in the namespace 
    /// SerializableTypes and add your serialized members
    /// with AddSerializedMember()
    /// </summary>
    public abstract class Serializable : IConvertible
    {
        private List<IConvertible> m_serializableMembers;
        
        protected void InitSerializableMembers(params IConvertible[] convertibles)
        {
            m_serializableMembers = new List<IConvertible>();
            for(int i = 0; i < convertibles.Length; i++)
            {
                m_serializableMembers.Add(convertibles[i]);
            }
        }

        protected override int GetSourceByteCount()
        {
            int total = 0;
            for(int i = 0; i < m_serializableMembers.Count; i++)
            {
                total += m_serializableMembers[i].GetByteCount();
            }
            return total;
        }

        protected override byte[] GetSourceBytes()
        {
            byte[] bytes = new byte[GetSourceByteCount()];

            int byteCount = 0;
            foreach (IConvertible convertible in m_serializableMembers)
            {
                byte[] memberBytes = convertible.GetBytes();
                for (int i = 0; i < memberBytes.Length; i++)
                {
                    bytes[byteCount++] = memberBytes[i];
                }
            }
            return bytes;
        }

        protected override bool CreateFromSourceBytes(ArraySegment<byte> bytes)
        {
            int index = 0;
            for(int i = 0; i < m_serializableMembers.Count; i++)
            {
                if (!m_serializableMembers[i].CreateFromBytes(bytes.ArraySegmentFrom(index)))
                {
                    return false;
                }
                index += m_serializableMembers[i].GetByteCount();
            }
            return true;
        }

        protected abstract override ID.BYTE_TYPE SerializationID();
    }

    namespace types
    {
        public abstract class Serialized<T> : IConvertible
        {
            protected T m_value;

            public Serialized()
            {
                m_value = default;
            }

            public Serialized(T value)
            {
                m_value = value;
            }

            public T Value
            {
                get => m_value;
                set => m_value = value;
            }

            protected abstract override byte[] GetSourceBytes();
            protected abstract override int GetSourceByteCount();
            protected abstract override bool CreateFromSourceBytes(ArraySegment<byte> sourceBytes);

            protected abstract override ID.BYTE_TYPE SerializationID();
        }

        public class Byte : Serialized<byte>
        {
            public Byte(byte value) : base(value) { }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> sourceBytes)
            {
                m_value = sourceBytes.At(0);
                return true;
            }

            protected override int GetSourceByteCount()
            {
                return 1;
            }

            protected override byte[] GetSourceBytes()
            {
                return new byte[] { m_value };
            }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.BYTE;
            }
        }

        public class ByteArray : Serialized<byte[]>
        {
            public ByteArray(byte[] value) : base(value)  { }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> sourceBytes)
            {
                int length = System.BitConverter.ToInt32(sourceBytes.Array, sourceBytes.Offset);
                m_value = new byte[length];
                if (length <= sourceBytes.Count - sizeof(int))
                {
                    for (int i = 0; i < length; i++)
                    {
                        m_value[i] = sourceBytes.At(i + sizeof(int));
                    }
                    return true;
                }
                return false;
            }

            protected override int GetSourceByteCount()
            {
                return m_value.Length + sizeof(int);
            }

            protected override byte[] GetSourceBytes()
            {
                byte[] bytes = new byte[GetSourceByteCount()];

                byte[] countBytes = System.BitConverter.GetBytes(m_value.Length);
                for(int i = 0; i < sizeof(int); i++)
                {
                    bytes[i] = countBytes[i];
                }

                for(int i = sizeof(int); i < bytes.Length; i++)
                {
                    bytes[i] = m_value[i - sizeof(int)];
                }

                return bytes;
            }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.BYTEARRAY;
            }
        }

        public class Int32 : Serialized<int>
        {
            public Int32() : base() { }

            public Int32(int value) : base(value) { }
            public Int32() : base() { }
            
            protected override byte[] GetSourceBytes()
            {
                return System.BitConverter.GetBytes(m_value);
            }

            protected override int GetSourceByteCount()
            {
                return sizeof(int);
            }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> bytes)
            {
                m_value =  System.BitConverter.ToInt32(bytes.Array, bytes.Offset);
                return true;
            }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.INT32;
            }
        }

        public class Float : Serialized<float>
        {
            public Float(float value) : base(value)
            {
            }

            protected override byte[] GetSourceBytes()
            {
                return System.BitConverter.GetBytes(m_value);
            }

            protected override int GetSourceByteCount()
            {
                return sizeof(float);
            }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> bytes)
            {
                m_value = System.BitConverter.ToSingle(bytes.Array, bytes.Offset);
                return true;
            }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return serialization.ID.BYTE_TYPE.FLOAT;
            }
        }

        public class Uint32 : Serialized<uint>
        {
            public Uint32(uint value) : base(value)  { }

            protected override byte[] GetSourceBytes()
            {
                return System.BitConverter.GetBytes(m_value);
            }

            protected override int GetSourceByteCount()
            {
                return sizeof(int);
            }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> bytes)
            {
                m_value = System.BitConverter.ToUInt32(bytes.Array, bytes.Offset);
                return true;
            }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.UINT32;
            }
        }

        public class Bool : Serialized<bool>
        {
            public Bool(bool value) : base(value)
            {
            }

            protected override byte[] GetSourceBytes()
            {
                return System.BitConverter.GetBytes(m_value);
            }

            protected override int GetSourceByteCount()
            {
                return sizeof(bool);
            }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> bytes)
            {
                m_value = System.BitConverter.ToBoolean(bytes.Array, bytes.Offset);
                return true;
            }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.BOOL;
            }
        }

        public class Vector2 : Serialized<UnityEngine.Vector2>
        {
            public Vector2(UnityEngine.Vector2 value) : base(value)
            {
            }

            protected override byte[] GetSourceBytes()
            {
                byte[] bytes = new byte[sizeof(float) * 2];
                for (int i = 0; i < sizeof(float); i++)
                {
                    bytes[i] = System.BitConverter.GetBytes(m_value.x)[i];
                    bytes[i + sizeof(float)] = System.BitConverter.GetBytes(m_value.y)[i];
                }
                return bytes;
            }

            protected override int GetSourceByteCount()
            {
                return sizeof(float) * 2;
            }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> bytes)
            {
                m_value = new UnityEngine.Vector2(
                    System.BitConverter.ToSingle(bytes.Array, bytes.Offset), 
                    System.BitConverter.ToSingle(bytes.Array, bytes.Offset + sizeof(float)));
                return true;
            }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.VECTOR2;
            }
        }

        public class Vector2Int : Serialized<UnityEngine.Vector2Int>
        {
            public Vector2Int(UnityEngine.Vector2Int value) : base(value)
            {
            }

            protected override byte[] GetSourceBytes()
            {
                byte[] bytes = new byte[sizeof(int) * 2];
                for (int i = 0; i < sizeof(int); i++)
                {
                    bytes[i] = System.BitConverter.GetBytes(m_value.x)[i];
                    bytes[i + sizeof(int)] = System.BitConverter.GetBytes(m_value.y)[i];
                }
                return bytes;
            }

            protected override int GetSourceByteCount()
            {
                return sizeof(int) * 2;
            }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> bytes)
            {
                m_value = new UnityEngine.Vector2Int(
                    System.BitConverter.ToInt32(bytes.Array, bytes.Offset),
                    System.BitConverter.ToInt32(bytes.Array, bytes.Offset + sizeof(int)));
                return true;
            }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.VECTOR2INT;
            }
        }

        public class Quaternion : Serialized<UnityEngine.Quaternion>
        {
            public Quaternion(UnityEngine.Quaternion value) : base(value)
            {
            }

            protected override byte[] GetSourceBytes()
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

            protected override int GetSourceByteCount()
            {
                return sizeof(float) * 4;
            }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> bytes)
            {
                m_value = new UnityEngine.Quaternion(
                    System.BitConverter.ToSingle(bytes.Array, bytes.Offset),
                    System.BitConverter.ToSingle(bytes.Array, bytes.Offset + sizeof(float)),
                    System.BitConverter.ToSingle(bytes.Array, bytes.Offset + 2 * sizeof(float)),
                    System.BitConverter.ToSingle(bytes.Array, bytes.Offset + 3 * sizeof(float)));
                return true;
            }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.QUATERNION;
            }
        }

        public class String : Serialized<string>
        {
            public String() : base() { }

            public String(string value) : base(value)
            {
            }

            protected override byte[] GetSourceBytes()
            {
                byte[] strBytes = System.Text.Encoding.Unicode.GetBytes(m_value);
                byte[] countBytes = System.BitConverter.GetBytes(strBytes.Length);
                byte[] bytes = new byte[strBytes.Length + countBytes.Length];

                int i = 0;
                for(; i < countBytes.Length; i++)
                {
                    bytes[i] = countBytes[i];
                }

                for (; i < strBytes.Length + countBytes.Length; i++)
                {
                    bytes[i] = strBytes[i - countBytes.Length];
                }

                return bytes;
            }

            protected override int GetSourceByteCount()
            {
                return GetSourceBytes().Length;
            }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> bytes)
            {
                int byteCount = System.BitConverter.ToInt32(bytes.Array, bytes.Offset);
                m_value = System.Text.Encoding.Unicode.GetString(bytes.Array, bytes.Offset + sizeof(int), byteCount);
                return true;
            }

            protected override ID.BYTE_TYPE SerializationID()
            {
                return ID.BYTE_TYPE.STRING;
            }
        }

        /// <summary>
        /// The following container classes are abstract.
        /// To use a serialized container class :
        /// - Create a child class with the type of data to contain
        /// - Implement SerializationID with that data type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        abstract public class List<T> : Serialized<System.Collections.Generic.List<T>> where T : IConvertible, new()
        {
            public T this[int key]
            {
                get => m_value[key];
                set => m_value[key] = value;
            }

            public List()
            {
                m_value = new System.Collections.Generic.List<T>();
            }
            
            public List(System.Collections.Generic.List<T> list) : base(list) { }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> bytes)
            {
                int itemCount = System.BitConverter.ToInt32(bytes.Array, bytes.Offset);

                if (m_value == null)
                {
                    m_value = new System.Collections.Generic.List<T>(itemCount);
                }
                m_value.Clear();

                int header = sizeof(int); // item count
                int subIndex = header;
                for (int i = 0; i < itemCount; i++)
                {
                    T obj = new T();
                    if(!obj.CreateFromBytes(bytes.ArraySegmentFrom(subIndex)))
                    {
                        m_value.Clear();
                        return false;
                    }
                    subIndex += obj.GetByteCount();
                    m_value.Add(obj);
                }

                return true;
            }

            protected override int GetSourceByteCount()
            {
                int total = sizeof(int); // item count
                for(int i = 0; i < m_value.Count; i++)
                {
                    total += m_value[i].GetByteCount();
                }

                return total;
            }

            protected override byte[] GetSourceBytes()
            {
                byte[] bytes = new byte[GetSourceByteCount()];

                byte[] itemCountBytes = System.BitConverter.GetBytes(m_value.Count);
                int byteCount = 0;
                for (int i = 0; i < itemCountBytes.Length; i++)
                {
                    bytes[byteCount++] = itemCountBytes[i];
                }

                for(int i = 0; i < m_value.Count; i++)
                {
                    byte[] itemBytes = m_value[i].GetBytes();
                    for (int b = 0; b < itemBytes.Length; b++)
                    {
                        bytes[byteCount++] = itemBytes[b];
                    }
                }
                return bytes;
            }

            protected abstract override ID.BYTE_TYPE SerializationID();
        }
        
        abstract public class HashMap<T> : Serialized<Dictionary<int, T>> where T : IConvertible, new()
        {
            public HashMap(Dictionary<int, T> dict) : base(dict) { }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> bytes)
            {
                int itemCount = System.BitConverter.ToInt32(bytes.Array, bytes.Offset);

                Dictionary<int, T> dict = new Dictionary<int, T>();

                int header = sizeof(int); // item count
                int index = header;
                for (int i = 0; i < itemCount; i++)
                {
                    int key = System.BitConverter.ToInt32(bytes.Array, index + bytes.Offset);
                    index += sizeof(int);

                    T obj = new T();
                    if(!obj.CreateFromBytes(bytes.ArraySegmentFrom(index)))
                    {
                        m_value.Clear();
                        return false;
                    }

                    index += obj.GetByteCount();

                    dict[key] = obj;
                }
                m_value = dict;
                return true;
            }

            protected override int GetSourceByteCount()
            {
                int total = sizeof(int); // item count;
                
                foreach(int key in m_value.Keys)
                {
                    total += sizeof(int); // key is int
                    total += m_value[key].GetByteCount();
                }

                return total;
            }

            protected override byte[] GetSourceBytes()
            {
                byte[] bytes = new byte[GetByteCount()];

                byte[] itemCountBytes = System.BitConverter.GetBytes(m_value.Count);
                for (int i = 0; i < itemCountBytes.Length; i++)
                {
                    bytes[i] = itemCountBytes[i];
                }

                int header = itemCountBytes.Length;
                int index = 0;
                foreach (int key in m_value.Keys)
                {
                    T obj = m_value[key];
                    byte[] keyBytes = System.BitConverter.GetBytes(key);
                    byte[] objBytes = obj.GetBytes();

                    for (int b = 0; b < keyBytes.Length; b++)
                    {
                        bytes[header + (index * (objBytes.Length + keyBytes.Length)) + b] = keyBytes[b];
                    }

                    for (int b = 0; b < objBytes.Length; b++)
                    {
                        bytes[header + keyBytes.Length + (index * (objBytes.Length + keyBytes.Length)) + b] = objBytes[b];
                    }
                    ++index;
                }
                return bytes;
            }

            protected abstract override ID.BYTE_TYPE SerializationID();
        }

        abstract public class Array<T> : Serialized<T[]> where T : IConvertible, new()
        {
            public T this[int key]
            {
                get => m_value[key];
                set => m_value[key] = value;
            }

            public Array(int size)
            {
                m_value = new T[size];
            }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> bytes)
            {
                int itemCount = System.BitConverter.ToInt32(bytes.Array, bytes.Offset);

                m_value = new T[itemCount];

                int header = sizeof(int); // item count
                int subIndex = header;
                for (int i = 0; i < itemCount; i++)
                {
                    T obj = new T();
                    if (!obj.CreateFromBytes(bytes.ArraySegmentFrom(subIndex)))
                    {
                        m_value = new T[0];
                        return false;
                    }
                    subIndex += obj.GetByteCount();
                    m_value[i] = obj;
                }

                return true;
            }

            protected override int GetSourceByteCount()
            {
                int total = sizeof(int); // item count
                for (int i = 0; i < m_value.Length; i++)
                {
                    total += m_value[i].GetByteCount();
                }

                return total;
            }

            protected override byte[] GetSourceBytes()
            {
                byte[] bytes = new byte[GetSourceByteCount()];

                int byteCount = 0;

                byte[] itemCountBytes = System.BitConverter.GetBytes(m_value.Length);
                for (int i = 0; i < itemCountBytes.Length; i++)
                {
                    bytes[byteCount++] = itemCountBytes[i];
                }

                for (int i = 0; i < m_value.Length; i++)
                {
                    byte[] itemBytes = m_value[i].GetBytes();
                    for (int b = 0; b < itemBytes.Length; b++)
                    {
                        bytes[byteCount++] = itemBytes[b];
                    }
                }
                return bytes;
            }

            protected abstract override ID.BYTE_TYPE SerializationID();
        }

        abstract public class Array2D<T> : Serialized<T[,]> where T : IConvertible, new()
        {
            public T this[int x, int y]
            {
                get => m_value[x, y];
                set => m_value[x, y] = value;
            }

            public Array2D(int width, int length) : base()
            {
                m_value = new T[width, length];
            }

            public Array2D(T[,] array) : base(array) { }

            protected override bool CreateFromSourceBytes(ArraySegment<byte> bytes)
            {
                int width = System.BitConverter.ToInt32(bytes.Array, bytes.Offset);
                int length = System.BitConverter.ToInt32(bytes.Array, bytes.Offset + sizeof(int));

                m_value = new T[width, length];

                int header = sizeof(int) * 2; // item count
                int index = header;
                int x = 0;
                int y = 0;
                for (int i = 0; i < width * length; i++)
                {
                    x = i % length;
                    y = i / length;
                    T obj = new T();
                    if (!obj.CreateFromBytes(bytes.ArraySegmentFrom(index)))
                    {
                        m_value = new T[0, 0];
                        return false;
                    }
                    index += obj.GetByteCount();
                    m_value[y, x] = obj; // we swap due to how objects are stored in the serialized array
                }
                
                return true;
            }

            protected override int GetSourceByteCount()
            {
                int total = sizeof(int) * 2; // item count
                int itemCount = m_value.GetLength(0) * m_value.GetLength(1);
                for (int i = 0; i < itemCount; i++)
                {
                    total += m_value[i  % m_value.GetLength(0), i / m_value.GetLength(0)].GetByteCount();
                }

                return total;
            }

            protected override byte[] GetSourceBytes()
            {
                byte[] bytes = new byte[GetSourceByteCount()];

                int width = m_value.GetLength(0);
                int length = m_value.GetLength(1);
                
                byte[] widthBytes = System.BitConverter.GetBytes(width);
                for (int i = 0; i < widthBytes.Length; i++)
                {
                    bytes[i] = widthBytes[i];
                }

                byte[] lengthBytes = System.BitConverter.GetBytes(length);
                for (int i = 0; i < widthBytes.Length; i++)
                {
                    bytes[i + widthBytes.Length] = lengthBytes[i];
                }

                int header = sizeof(int) * 2; // item count
                int index = header;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < length; y++)
                    {
                        T obj = m_value[x, y];
                        byte[] objBytes = obj.GetBytes();
                        for (int b = 0; b < objBytes.Length; b++)
                        {
                            bytes[index + b] = objBytes[b];
                        }
                        index += objBytes.Length;
                    }
                }
                return bytes;
            }

            protected abstract override ID.BYTE_TYPE SerializationID();
        }
    }
}
