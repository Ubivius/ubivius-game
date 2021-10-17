using UnityEngine;
using UnityEditor;
using ubv.common.world.cellType;
using System.Collections.Generic;

namespace ubv.client
{
    public static class LoadingData
    {
        //static public world.WorldRebuilder WorldRebuilder;
        static public CellInfo[,] WorldToLoad;
        static public List<int> PlayerIDs;
        static public ClientGameInfo GameInfo;
        static public string ActiveCharacterID;
    }
}