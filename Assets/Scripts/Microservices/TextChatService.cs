﻿using UnityEngine;
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

        [SerializeField]
        private FriendsListService m_friends;

        private Dictionary<string, string> m_cachedConversationIDs;
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
            m_cachedConversationIDs = new Dictionary<string, string>();
            m_messagesFetcher.FetchLogic += FetchNewMessages;
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
                    RefreshConversation(conversationID, new List<MessageInfo>(msgs));
                }));
            }
        }

        private void RefreshConversation(string conversationID, List<MessageInfo> messages)
        {
            lock (m_requestLock)
            {
                m_activeRequestCount--;
                if(m_activeRequestCount == 0)
                {
                    m_messagesFetcher.ReadyForNewFetch();
                }
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
                        messages[i].text);
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

        public void SendMessageTo(string currentUserID, string otherUserID, string text, UnityAction callback = default)
        {
            // get le conversation id associé à la relation entre current user pis other user
            // le cache (dans TextChat ?)
            if (m_cachedConversationIDs.ContainsKey(otherUserID))
            {
                this.Request(new PostTextChatRequest(currentUserID, m_cachedConversationIDs[otherUserID], text, callback));
            }
            else
            {
                m_friends.GetConversationWith(currentUserID, otherUserID, (string convID) => 
                {
                    if (convID != null)
                    {
                        m_cachedConversationIDs.Add(otherUserID, convID);
                        this.Request(new PostTextChatRequest(currentUserID, m_cachedConversationIDs[otherUserID], text, callback));
                    }
                });
            }
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
