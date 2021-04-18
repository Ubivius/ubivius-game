using UnityEngine;
using System.Collections;
using static ubv.microservices.CharacterDataService;
using System.Collections.Generic;

namespace ubv.client
{
    /// <summary>
    /// Contains all in-game data about the current game
    /// </summary>
    public class ClientGameInfo
    {
        public readonly Dictionary<int, CharacterData> PlayerCharacters;

        public ClientGameInfo(IEnumerable characters)
        {
            PlayerCharacters = new Dictionary<int, CharacterData>();
            foreach (CharacterData character in characters)
            {
                PlayerCharacters[character.PlayerID.GetHashCode()] = character;
            }
        }
    }
}
