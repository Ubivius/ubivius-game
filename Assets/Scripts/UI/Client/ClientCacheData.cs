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
    }
}
