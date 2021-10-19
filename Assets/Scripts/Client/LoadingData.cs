using UnityEngine;
using UnityEditor;
using ubv.common.world.cellType;
using System.Collections.Generic;
using ubv.common.data;
using static ubv.microservices.CharacterDataService;
using static ubv.microservices.DispatcherMicroservice;
using System;

namespace ubv.client.data
{
    public static class LoadingData
    {
        static public bool IsTryingToRejoinGame;
        static public ServerInitMessage ServerInit;
        static public ServerInfo? ServerInfo;
        static public string ActiveCharacterID;
    }
}