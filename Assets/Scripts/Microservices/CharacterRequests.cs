using UnityEngine;
using UnityEditor;

namespace ubv.microservices
{
    public delegate void OnGetCharacters(CharacterDataService.CharacterData[] characters);

    public abstract class GetCharacterRequest : GetMicroserviceRequest
    {
        public readonly OnGetCharacters Callback;
        
        public GetCharacterRequest(OnGetCharacters callback)
        {
            Callback = callback;
        }

        public override string URL()
        {
            return "characters";
        }
    }

    public class GetSingleCharacterRequest : GetCharacterRequest
    {
        private readonly string m_url;
        public GetSingleCharacterRequest(string CharacterID, OnGetCharacters callback) : base(callback)
        {
            this.m_url = CharacterID;
        }

        public override string URL()
        {
            return "characters/" + m_url;
        }
    }

    public class GetCharactersFromUserRequest : GetCharacterRequest
    {
        private readonly string m_url;

        public GetCharactersFromUserRequest(string PlayerID, OnGetCharacters callback) : base(callback)
        {
            this.m_url = PlayerID;
        }

        public override string URL()
        {
            return "characters/user/" + m_url;
        }
    }
}
