﻿using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace ubv.microservices
{
    public delegate void OnPostCharacter();
    public delegate void OnDeleteCharacter();

    public abstract class GetCharacterRequest : GetMicroserviceRequest
    {
        public readonly UnityAction<CharacterDataService.CharacterData[]> Callback;

        public GetCharacterRequest(UnityAction<CharacterDataService.CharacterData[]> callback)
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
        public GetSingleCharacterRequest(string CharacterID, UnityAction<CharacterDataService.CharacterData[]> callback) : base(callback)
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

        public GetCharactersFromUserRequest(string PlayerID, UnityAction<CharacterDataService.CharacterData[]> callback) : base(callback)
        {
            this.m_url = PlayerID;
        }

        public override string URL()
        {
            return "characters/user/" + m_url;
        }
    }

    public class GetCharactersAliveFromUserRequest : GetCharacterRequest
    {
        private readonly string m_url;

        public GetCharactersAliveFromUserRequest(string PlayerID, UnityAction<CharacterDataService.CharacterData[]> callback) : base(callback)
        {
            this.m_url = PlayerID;
        }

        public override string URL()
        {
            return "characters/alive/user/" + m_url;
        }
    }

    public class PostCharacterRequest : PostMicroserviceRequest
    {
        private readonly string m_userID;
        private readonly string m_characterName;

        public readonly OnPostCharacter Callback;

        public PostCharacterRequest(string userID, string characterName, OnPostCharacter callback)
        {
            m_userID = userID;
            m_characterName = characterName;
            Callback = callback;
        }

        public override string JSONString()
        {
            return JsonUtility.ToJson(new CharacterDataService.JSONCharacterData {
                user_id = m_userID,
                name = m_characterName
            }).ToString();
        }

        public override string URL()
        {
            return "characters";
        }
    }

    public class DeleteCharacterRequest : DeleteMicroserviceRequest
    {
        private readonly string m_characterID;

        public readonly OnDeleteCharacter Callback;

        public DeleteCharacterRequest(string characterID, OnDeleteCharacter callback)
        {
            m_characterID = characterID;
            Callback = callback;
        }

        public override string URL()
        {
            return "characters/" + m_characterID;
        }
    }
}
