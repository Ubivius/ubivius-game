using UnityEngine;
using System.Collections;
using ubv.common.serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ubv.client.io
{
    public class ClientFileSaveManager : MonoBehaviour
    {
        public void SaveFile(Serializable obj, string filePath)
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

            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, obj.GetBytes());
            file.Close();
        }

        public T LoadFromFile<T>(string filePath) where T : Serializable, new()
        {
            FileStream file;

            if (File.Exists(filePath))
            {
                file = File.OpenRead(filePath);
            }
            else
            {
                Debug.LogError("File not found");
                return null;
            }

            BinaryFormatter bf = new BinaryFormatter();
            byte[] data = (byte[])bf.Deserialize(file);
            file.Close();

            return IConvertible.CreateFromBytes<T>(data.ArraySegment());
        }
    }
}
