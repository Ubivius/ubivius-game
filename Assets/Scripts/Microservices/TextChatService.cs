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
        private FriendsListService m_friends;

        private Dictionary<string, string> m_cachedConversations;

        // message id, user id, text
        public UnityAction<string, string, string> OnNewMessageFrom;
        private void Awake()
        {
            m_cachedConversations = new Dictionary<string, string>();
        }

        private void Update()
        {
            
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
            originalRequest.Callback.Invoke();
        }

        public void SendMessageTo(string currentUserID, string otherUserID, string text, UnityAction callback = default)
        {
            // get le conversation id associé à la relation entre current user pis other user
            // le cache (dans TextChat ?)
            if (m_cachedConversations.ContainsKey(otherUserID))
            {
                this.Request(new PostTextChatRequest(currentUserID, m_cachedConversations[otherUserID], text, callback));
            }
            else
            {
                m_friends.GetConversationWith(currentUserID, otherUserID, (string convID) => 
                {
                    if (convID != null)
                    {
                        m_cachedConversations.Add(otherUserID, convID);
                        this.Request(new PostTextChatRequest(currentUserID, m_cachedConversations[otherUserID], text, callback));
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