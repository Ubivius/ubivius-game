using UnityEngine;
using System.Collections;
using ubv.http.client;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;

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
            public bool Alive { get; private set; }
            public int GamesPlayed { get; private set; }
            public int GamesWon { get; private set; }
            public int EnemiesKilled { get; private set; }

            public CharacterData(string name, string id, string playerID, bool alive, int gamesPlayed, int gamesWon, int enemiesKilled)
            {
                Name = name;
                ID = id;
                PlayerID = playerID;
                Alive = alive;
                GamesPlayed = gamesPlayed;
                GamesWon = gamesWon;
                EnemiesKilled = enemiesKilled;
            }
        }
        [System.Serializable]
        public struct JSONCharacterData
        {
            public string id;
            public string user_id;
            public string name;
            public bool alive;
            public int games_played;
            public int games_won;
            public int enemies_killed;
        }
        
        protected override void OnGetResponse(string JSON, GetCharacterRequest originalRequest)
        {
            if (originalRequest is GetCharactersFromUserRequest || originalRequest is GetCharactersAliveFromUserRequest)
            {
                string JSONFixed = JsonHelper.FixJsonArrayFromServer(JSON);
                JSONCharacterData[] jsonDataArray = JsonHelper.ArrayFromJsonString<JSONCharacterData>(JSONFixed);

                CharacterData[] characters = new CharacterData[jsonDataArray.Length];
                for (int i = 0; i < jsonDataArray.Length; i++)
                {
                    characters[i] = new CharacterData(
                        jsonDataArray[i].name, 
                        jsonDataArray[i].id, 
                        jsonDataArray[i].user_id, 
                        jsonDataArray[i].alive, 
                        jsonDataArray[i].games_played,
                        jsonDataArray[i].games_won,
                        jsonDataArray[i].enemies_killed);
                }

                originalRequest.Callback.Invoke(characters);
            }
            else if (originalRequest is GetSingleCharacterRequest)
            {
                JSONCharacterData jsonData = JsonUtility.FromJson<JSONCharacterData>(JSON);
                CharacterData[] characters = new CharacterData[1];
                characters[0] = new CharacterData(
                    jsonData.name, 
                    jsonData.id, 
                    jsonData.user_id,
                    jsonData.alive,
                    jsonData.games_played,
                    jsonData.games_won,
                    jsonData.enemies_killed);
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
            CharacterData character = new CharacterData(
                m_mockData.CharacterName,
                m_mockData.CharacterID,
                m_mockData.UserID,
                m_mockData.Alive,
                m_mockData.GamesPlayed,
                m_mockData.GamesWon,
                m_mockData.EnemiesKilled);
            CharacterData[] characters = new CharacterData[]
            {
                    character
            };
            request.Callback.Invoke(characters);
        }
    }
}
