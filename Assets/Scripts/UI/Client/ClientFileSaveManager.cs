using UnityEngine;
using System.Collections;
using ubv.common.serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ubv.client.io
{
    public class ClientFileSaveManager : MonoBehaviour
    {
        public void SaveFile(object obj, string filePath)
        {
            string dest = Application.persistentDataPath + "/" + filePath;
            FileStream file;

            if (File.Exists(dest))
            {
                file = File.OpenWrite(dest);
            }
            else
            {
                file = File.Create(dest);
            }

            // TODO: Use something else
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, obj);
            file.Close();
        }

        public T LoadFromFile<T>(string filePath)
        {
            FileStream file;
            string dest = Application.persistentDataPath + "/" + filePath;
            if (File.Exists(dest))
            {
                file = File.OpenRead(dest);
            }
            else
            {
                Debug.LogError("File not found");
                return default;
            }

            BinaryFormatter bf = new BinaryFormatter();
            file.Position = 0;
            T data = (T)bf.Deserialize(file);
            file.Close();

            return data;
        }
    }
}
