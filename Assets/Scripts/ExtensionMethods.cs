using System;
namespace ubv
{
    public static class ExtensionMethods
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T[] ArrayFrom<T>(this T[] data, int index)
        {
            T[] result = new T[data.Length - index];
            Array.Copy(data, index, result, 0, result.Length);
            return result;
        }
        
        public static ArraySegment<T> ArraySegment<T>(this T[] data)
        {
            return new ArraySegment<T>(data, 0, data.Length);
        }

        public static ArraySegment<T> ArraySegmentFrom<T>(this ArraySegment<T> segment, int start)
        {
            return new ArraySegment<T>(segment.Array, segment.Offset + start, segment.Count - start);
        }

        public static T At<T>(this ArraySegment<T> data, int index)
        {
            return data.Array[index + data.Offset];
        }
    }
}
