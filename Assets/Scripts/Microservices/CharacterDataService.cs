using UnityEngine;
using System.Collections;
using ubv.http.client;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;

namespace ubv.microservices
{
    public class CharacterDataService : Microservice<GetCharacterRequest, 
        PostMicroserviceRequest, PutMicroserviceRequest, DeleteMicroserviceRequest>
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
            if (originalRequest is GetCharactersFromUserRequest)
            {
                string JSONFixed = JsonHelper.FixJsonArrayFromServer(JSON);
                JSONCharacterData[] jsonDataArray = JsonHelper.FromJson<JSONCharacterData>(JSONFixed);

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
