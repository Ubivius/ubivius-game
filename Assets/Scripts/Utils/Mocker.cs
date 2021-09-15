using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ubv.utils
{
    [CreateAssetMenu(fileName = "MockData", menuName = "ScriptableObjects/Utils/MockData", order = 1)]
    public class Mocker : ScriptableObject
    {
        public string UserID;
        public string UserName;
        public string CharacterID;
        public string CharacterName;
        
    }
}