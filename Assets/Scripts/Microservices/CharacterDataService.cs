using UnityEngine;
using System.Collections;
using ubv.http;
using System.Net.Http;
using System.Net;

namespace ubv.microservices
{
    public class CharacterDataService : MonoBehaviour
    {
        [SerializeField] private bool m_mock;
        [SerializeField] private HTTPClient m_HTTPClient;
        [SerializeField] string m_authEndpoint;

        public delegate void OnGetCharacters(CharacterData[] characters);
        private OnGetCharacters m_onGetCharactersCallback;

        public class CharacterData
        {
            public string Name { get; private set; }
            public string ID { get; private set; }

            public CharacterData(string name, string id)
            {
                Name = name;
                ID = id;
            }
        }
        
        private struct JSONCharacterData
        {
            public string id;
            public string user_id;
            public string name;
        }

        private struct JSONCharacterRequest
        {
            string id;
        }

        public void GetCharacters(string playerID, OnGetCharacters onGetCharacters)
        {
            if (m_mock)
            {
#if DEBUG_LOG
                Debug.Log("Mocking char-data.");
#endif // DEBUG_LOG
                CharacterData character = new CharacterData("mock-murphy", "mock-murphy-id-1234");
                CharacterData[] characters = new CharacterData[]
                {
                    character
                };
                onGetCharacters(characters);
                return;
            }

            
            m_HTTPClient.SetEndpoint(m_authEndpoint);
            m_onGetCharactersCallback = onGetCharacters;
            m_HTTPClient.Get("characters/user" + playerID, OnCharacterDataResponse);
        }
        
        private void OnCharacterDataResponse(HttpResponseMessage message)
        {
            if (message.StatusCode == HttpStatusCode.OK)
            {
                // check https://stackoverflow.com/questions/36239705/serialize-and-deserialize-json-and-json-array-in-unity
                string JSON = message.Content.ReadAsStringAsync().Result;
                JSONCharacterData authResponse = JsonUtility.FromJson<JSONCharacterData>(JSON);
                
                CharacterData[] characters = new CharacterData[0];
                m_onGetCharactersCallback.Invoke(characters);
                m_onGetCharactersCallback = null;
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("Authentication login request was not successful");
#endif // DEBUG_LOG
            }
        }
    }
}
