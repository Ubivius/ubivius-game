using UnityEngine;
using System.Collections;
using ubv.http.client;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;

namespace ubv.microservices
{
    public class CharacterDataService : Microservice<GetCharacterRequest, PostMicroserviceRequest>
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
        private struct JSONCharacterData
        {
            public string id;
            public string user_id;
            public string name;
        }
        
        protected override void OnGetResponse(string JSON, GetCharacterRequest originalRequest)
        {
            string JSONFixed = JsonHelper.FixJsonArrayFromServer(JSON);
            JSONCharacterData[] authResponse = JsonHelper.FromJson<JSONCharacterData>(JSONFixed);

            CharacterData[] characters = new CharacterData[authResponse.Length];
            for (int i = 0; i < authResponse.Length; i++)
            {
                characters[i] = new CharacterData(authResponse[i].name, authResponse[i].id, authResponse[i].user_id);
            }

            originalRequest.Callback.Invoke(characters);
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
