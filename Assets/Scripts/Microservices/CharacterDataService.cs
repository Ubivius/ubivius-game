using UnityEngine;
using System.Collections;
using ubv.http;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;

namespace ubv.microservices
{
    public class CharacterDataService : MonoBehaviour
    {
        public delegate void OnGetCharacters(CharacterData[] characters);
        public delegate void OnGetSingle(CharacterData character);

        private class SingleRequest
        {
            public string CharacterID;
            public OnGetSingle Callback;
        }

        private class CharactersRequest
        {
            public string PlayerID;
            public OnGetCharacters Callback;
        }

        [SerializeField] private bool m_mock;
        [SerializeField] private HTTPClient m_HTTPClient;
        [SerializeField] string m_characterDataEndpoint;

        private bool m_readyForNextSingleRequest;
        private bool m_readyForNextCharactersRequest;
        private Queue<CharactersRequest> m_onGetCharactersRequests;
        private Queue<SingleRequest> m_onGetSingleRequests;

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

        private void Awake()
        {
            m_readyForNextCharactersRequest = true;
            m_readyForNextSingleRequest = true;
            m_onGetSingleRequests = new Queue<SingleRequest>();
            m_onGetCharactersRequests = new Queue<CharactersRequest>();
        }

        private void Update()
        {
            if (m_readyForNextSingleRequest)
            {
                if (m_onGetSingleRequests.Count > 0)
                {
                    SingleRequest request = m_onGetSingleRequests.Peek();
                    GetCharacter(request.CharacterID);
                }
            }

            if (m_readyForNextCharactersRequest)
            {
                if (m_onGetCharactersRequests.Count > 0)
                {
                    CharactersRequest request = m_onGetCharactersRequests.Peek();
                    GetCharacters(request.PlayerID);
                }
            }
        }

        public void GetCharacter(string characterID, OnGetSingle onGetCharacter)
        {
            if (m_mock)
            {
#if DEBUG_LOG
                Debug.Log("Mocking char-data.");
#endif // DEBUG_LOG
                CharacterData character = new CharacterData("mock-murphy", "mock-murphy-id-1234", "murphy-id-123");
                onGetCharacter(character);
                return;
            }

            m_onGetSingleRequests.Enqueue(new SingleRequest() { CharacterID = characterID, Callback = onGetCharacter });
            if (!m_readyForNextSingleRequest)
            {
                return;
            }
            
            m_readyForNextSingleRequest = false;
            GetCharacter(characterID);
        }

        private void GetCharacter(string characterID)
        {
            m_HTTPClient.SetEndpoint(m_characterDataEndpoint);
            m_HTTPClient.Get("characters/" + characterID, OnCharacterDataResponse);
        }

        private void GetCharacters(string userID)
        {
            m_HTTPClient.SetEndpoint(m_characterDataEndpoint);
            m_HTTPClient.Get("characters/user/" + userID, OnCharactersDataResponse);
        }

        public void GetCharacters(string playerID, OnGetCharacters onGetCharacters)
        {
            if (m_mock)
            {
#if DEBUG_LOG
                Debug.Log("Mocking char-data.");
#endif // DEBUG_LOG
                CharacterData character = new CharacterData("mock-murphy", "mock-murphy-id-1234", "murphy-id-123");
                CharacterData[] characters = new CharacterData[]
                {
                    character
                };
                onGetCharacters(characters);
                return;
            }
            
            m_onGetCharactersRequests.Enqueue(new CharactersRequest() { PlayerID = playerID, Callback = onGetCharacters });
            if (!m_readyForNextCharactersRequest)
            {
                return;
            }

            m_readyForNextCharactersRequest = false;
            GetCharacters(playerID);
        }
        
        private void OnCharactersDataResponse(HttpResponseMessage message)
        {
            if (message.StatusCode == HttpStatusCode.OK)
            {
                string JSON = JsonHelper.FixJsonArrayFromServer(message.Content.ReadAsStringAsync().Result);
                JSONCharacterData[] authResponse = JsonHelper.FromJson<JSONCharacterData>(JSON);

                CharacterData[] characters = new CharacterData[authResponse.Length];
                for (int i = 0; i < authResponse.Length; i++)
                {
                    characters[i] = new CharacterData(authResponse[i].name, authResponse[i].id, authResponse[i].user_id);
                }

                m_onGetCharactersRequests.Dequeue().Callback.Invoke(characters);
                m_readyForNextCharactersRequest = true;
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("Character data request was not successful");
#endif // DEBUG_LOG
            }
        }

        private void OnCharacterDataResponse(HttpResponseMessage message)
        {
            if (message.StatusCode == HttpStatusCode.OK)
            {
                string JSON = message.Content.ReadAsStringAsync().Result;
                JSONCharacterData authResponse = JsonUtility.FromJson<JSONCharacterData>(JSON);

                CharacterData character = new CharacterData(authResponse.name, authResponse.id, authResponse.user_id);

                m_onGetSingleRequests.Dequeue().Callback.Invoke(character);
                m_readyForNextSingleRequest = true;
            }
            else
            {
#if DEBUG_LOG
                Debug.Log("Character data request was not successful");
#endif // DEBUG_LOG
            }
        }
    }
}
