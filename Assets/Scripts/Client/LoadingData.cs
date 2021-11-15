using UnityEngine;
using UnityEditor;
using ubv.common.world.cellType;
using System.Collections.Generic;
using ubv.common.data;
using static ubv.microservices.CharacterDataService;
using static ubv.microservices.DispatcherMicroservice;
using System;
using ubv.microservices;

namespace ubv.client.data
{
    public static class LoadingData
    {
        static public ServerInitMessage ServerInit = null;
        static public string GameID = string.Empty;
        static public string ActiveCharacterID = string.Empty;
    }
}
