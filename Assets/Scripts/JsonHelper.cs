using UnityEngine;
using System.Collections;
using System;

namespace ubv
{
    public static class JsonHelper
    {
        public static T[] ArrayFromJsonString<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>
            {
                items = array
            };
            return JsonUtility.ToJson(wrapper);
        }

        public static string FixJsonArrayFromServer(string value)
        {
            return "{\"items\":" + value + "}";
        }
        
        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }
    }
}
