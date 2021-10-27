using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Events;

namespace ubv.microservices
{
    public class TextChatService : Microservice<GetTextChatRequest,
        PostTextChatRequest, PutMicroserviceRequest, DeleteMicroserviceRequest>
    {
        [SerializeField]
        private MicroserviceAutoFetcher m_messagesFetcher;
        
        // conversationID, messages
        private Dictionary<string, Dictionary<string, MessageInfo>> m_cachedConversations;

        private int m_activeRequestCount;

        public bool IsFetcherActive;
        // userID, msgInfo
        public UnityAction<string, MessageInfo> OnNewMessageFrom;

        private void Awake()
        {
            m_activeRequestCount = 0;
            IsFetcherActive = false;
            m_cachedConversations = new Dictionary<string, Dictionary<string, MessageInfo>>();
            m_messagesFetcher.FetchLogic += FetchNewMessages;
        }

        public void AddConversationToCache(string conversationID)
        {
            this.Request(new GetMessagesRequest(conversationID, (MessageInfo[] msgs) =>
            {
                RefreshConversation(conversationID, new List<MessageInfo>(msgs), false);
            }));
        }
        
        public void GetMessagesFromConversation(string conversationID, UnityAction<Dictionary<string, MessageInfo>> OnGetMessages)
        {
            this.Request(new GetMessagesRequest(conversationID, (MessageInfo[] msgs) =>
            {
                RefreshConversation(conversationID, new List<MessageInfo>(msgs), false);
                OnGetMessages(m_cachedConversations[conversationID]);
            }));
        }
        
        private void FetchNewMessages()
        {
            if (!IsFetcherActive || m_cachedConversations.Count == 0)
            {
                m_messagesFetcher.ReadyForNewFetch();
                return;
            }

            foreach (string conversationID in m_cachedConversations.Keys)
            {
                lock (m_requestLock)
                {
                    m_activeRequestCount++;
                }
                this.Request(new GetMessagesRequest(conversationID, (MessageInfo[] msgs) =>
                {
                    lock (m_requestLock)
                    {
                        m_activeRequestCount--;
                    }
                    if (m_activeRequestCount == 0)
                    {
                        m_messagesFetcher.ReadyForNewFetch();
                    }
                    RefreshConversation(conversationID, new List<MessageInfo>(msgs));
                }));
            }
        }

        /// <summary>
        /// Updates the conversations, caching it if not already locally cached, and
        /// callbacks OnNewMessages if enabled
        /// </summary>
        /// <param name="conversationID"></param>
        /// <param name="messages"></param>
        /// <param name="callUpdateCallback"></param>
        private void RefreshConversation(string conversationID, List<MessageInfo> messages, bool callUpdateCallback = true)
        {
            if (!m_cachedConversations.ContainsKey(conversationID))
            {
                m_cachedConversations.Add(conversationID, new Dictionary<string, MessageInfo>());
            }

            if (messages.Count == m_cachedConversations[conversationID].Count)
            {
                return;
            }

            foreach (MessageInfo msg in messages)
            {
                if (!m_cachedConversations[conversationID].ContainsKey(msg.MessageID))
                {
                    m_cachedConversations[conversationID].Add(msg.MessageID, msg);
                    if (callUpdateCallback)
                    {
                        OnNewMessageFrom.Invoke(msg.UserID, msg);
                    }
                }
            }
        }

        public struct JSONConversationInfo
        {
            public string[] user_id;
            public string game_id;
        }

        protected override void OnGetResponse(string JSON, GetTextChatRequest originalRequest)
        {
            if (originalRequest is GetMessagesRequest msgReq)
            {
                string JSONFixed = JsonHelper.FixJsonArrayFromServer(JSON);
                JSONMessage[] messages = JsonHelper.ArrayFromJsonString<JSONMessage>(JSONFixed);

                MessageInfo[] infos = new MessageInfo[messages.Length];
                for (int i = 0; i < messages.Length; i++)
                {
                    infos[i] = new MessageInfo(messages[i].id,
                        messages[i].user_id,
                        messages[i].conversation_id,
                        messages[i].text, 
                        System.DateTime.Parse(messages[i].created_on));
                }

                msgReq.Callback.Invoke(infos);
            }
            else if (originalRequest is GetConversationInfoRequest convReq)
            {
                JSONConversationInfo conversationJSON = JsonUtility.FromJson<JSONConversationInfo>(JSON);

                convReq.Callback.Invoke(new ConversationInfo(conversationJSON.user_id, conversationJSON.game_id));
            }
        }

        protected override void OnPostResponse(string JSON, PostTextChatRequest originalRequest)
        {
            originalRequest.Callback?.Invoke();
        }

        public void SendMessageToConversation(string currentUserID, string conversationID, string text, UnityAction callback = default)
        {
            this.Request(new PostTextChatRequest(currentUserID, conversationID, text, callback));
        }

#if UNITY_EDITOR
        [SerializeField]
        private AuthenticationService m_auth;
        [SerializeField]
        private UserService m_users;

        private void Start()
        {
            // TestWithMurphy();
        }

        private void TestWithMurphy()
        {
            string murphyID = "00bfd9f9-b93a-42dd-b27c-f8e03db4a946";
            string conversationID = "b07964e7-6525-469d-bb34-e0bead1b1836";
            m_auth.Request(new PostAuthenticationRequest("murphy", "password", (string id) =>
            {
                //this.Request(new PostTextChatRequest(murphyID, conversationID, "hello!", () => 
                //{
                    this.Request(new GetMessagesRequest(conversationID, (MessageInfo[] messages) => 
                    {
                        Debug.Log("Conversation messages: ");
                        foreach (MessageInfo msg in messages)
                        {
                            m_users.Request(new GetUserInfoRequest(msg.UserID, (UserInfo info) =>
                            {
                                Debug.Log(info.UserName + ": " + msg.Text);
                            }));
                        }
                    }));
                //}));
            }));

        }

#endif // UNITY_EDITOR
    }
}
