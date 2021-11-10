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
        public bool WasInGame;
        public DateTime LastUpdated;

        public static void SaveCache(bool wasInGame)
        {
            ClientCacheData cache = new ClientCacheData
            {
                WasInGame = wasInGame,
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
