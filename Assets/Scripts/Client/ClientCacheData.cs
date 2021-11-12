using UnityEngine;
using UnityEditor;
using ubv.common.serialization;
using ubv.common.data;
using static ubv.microservices.DispatcherMicroservice;
using System.Collections.Generic;
using System;

namespace ubv.client.data
{
    [System.Serializable]
    public class ClientCacheData
    {
        public string LastGameID;
        public DateTime LastUpdated;

        public static void SaveCache(string lastGameID)
        {
            ClientCacheData cache = new ClientCacheData
            {
                LastGameID = lastGameID,
                LastUpdated = DateTime.UtcNow
            };
            io.ClientFileSaveManager.SaveFile(cache, "cache.ubv");
        }

        public static ClientCacheData LoadCache()
        {
            return io.ClientFileSaveManager.LoadFromFile<ClientCacheData>("cache.ubv");
        }
    }
}
