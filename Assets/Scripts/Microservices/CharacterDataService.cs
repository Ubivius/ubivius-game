using UnityEngine;
using System.Collections;
using ubv.http.client;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using UnityEngine.Events;

namespace ubv.microservices
{
    public class CharacterDataService : Microservice<GetCharacterRequest, 
        PostCharacterRequest, PutMicroserviceRequest, DeleteCharacterRequest>
    {
        public class CharacterData
        {
            public string PlayerID { get; private set; }
            public string Name { get; private set; }
            public string ID { get; private set; }

            public CharacterData(string name, string id, string playerID)
            {
                Name = name;
                ID = id;
                PlayerID = playerID;
            }
        }
        [System.Serializable]
        public struct JSONCharacterData
        {
            public string id;
            public string user_id;
            public string name;
        }


        private Dictionary<string, CharacterData> m_cachedCharacters;
        private Dictionary<string, List<CharacterData>> m_cachedUserCharacters;
 
        private void Awake()
        {
            m_cachedCharacters = new Dictionary<string, CharacterData>();
            m_cachedUserCharacters = new Dictionary<string, List<CharacterData>>();
        }

        public void GetCharacter(string characterID, UnityAction<CharacterData> OnGetCharacter)
        {
            if (m_cachedCharacters.ContainsKey(characterID))
            {
                OnGetCharacter?.Invoke(m_cachedCharacters[characterID]);
                return;
            }

            this.Request(new GetSingleCharacterRequest(characterID, (CharacterData[] characters) =>
            {
                m_cachedCharacters.Add(characterID, characters[0]);
                OnGetCharacter?.Invoke(m_cachedCharacters[characterID]);
            }));
        }

        public void GetCharactersFromUser(string userID, UnityAction<List<CharacterData>> OnGetCharacters)
        {
            if (m_cachedUserCharacters.ContainsKey(userID))
            {
                OnGetCharacters?.Invoke(m_cachedUserCharacters[userID]);
                return;
            }

            this.Request(new GetCharactersFromUserRequest(userID, (CharacterData[] characters) =>
            {
                m_cachedUserCharacters.Add(userID, new List<CharacterData>(characters));
                OnGetCharacters?.Invoke(m_cachedUserCharacters[userID]);
            }));
        }

        protected override void OnGetResponse(string JSON, GetCharacterRequest originalRequest)
        {
            if (originalRequest is GetCharactersFromUserRequest)
            {
                string JSONFixed = JsonHelper.FixJsonArrayFromServer(JSON);
                JSONCharacterData[] jsonDataArray = JsonHelper.ArrayFromJsonString<JSONCharacterData>(JSONFixed);

                CharacterData[] characters = new CharacterData[jsonDataArray.Length];
                for (int i = 0; i < jsonDataArray.Length; i++)
                {
                    characters[i] = new CharacterData(jsonDataArray[i].name, jsonDataArray[i].id, jsonDataArray[i].user_id);
                }

                originalRequest.Callback.Invoke(characters);
            }
            else if (originalRequest is GetSingleCharacterRequest)
            {
                JSONCharacterData jsonData = JsonUtility.FromJson<JSONCharacterData>(JSON);
                CharacterData[] characters = new CharacterData[1];
                characters[0] = new CharacterData(jsonData.name, jsonData.id, jsonData.user_id);
                originalRequest.Callback.Invoke(characters);
            }
        }

        protected override void OnPostResponse(string JSON, PostCharacterRequest originalRequest)
        {
            originalRequest.Callback?.Invoke();
        }

        protected override void OnDeleteResponse(string JSON, DeleteCharacterRequest originalRequest)
        {
            originalRequest.Callback?.Invoke();
        }

        protected override void MockGet(GetCharacterRequest request)
        {
#if DEBUG_LOG
            Debug.Log("Mocking char-data.");
#endif // DEBUG_LOG
            CharacterData character = new CharacterData(m_mockData.CharacterName,
                m_mockData.CharacterID,
                m_mockData.UserID);
            CharacterData[] characters = new CharacterData[]
            {
                    character
            };
            request.Callback.Invoke(characters);
        }
    }
}
