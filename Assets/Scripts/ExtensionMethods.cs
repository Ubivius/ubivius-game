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

    }
}
