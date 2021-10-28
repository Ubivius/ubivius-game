using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

namespace ubv.microservices
{
    public class SocialServicesController : MonoBehaviour
    {
        public UserInfo CurrentUser { get; private set; }

        [SerializeField]
        private AuthenticationService m_auth;
        [SerializeField]
        private UserService m_users;
        [SerializeField]
        private TextChatService m_textChat;
        [SerializeField]
        private FriendsListService m_friendsList;
        
        public UnityAction<string> OnAuthentication;
        // sender, Message
        public UnityAction<string, MessageInfo> OnNewMessageFrom;
        public UnityAction<string> OnNewInviteFrom;

        private Dictionary<string, UserInfo> m_cachedUsers;
        private Dictionary<string, string> m_userNameIDs;
        private Dictionary<string, string> m_cachedUserConversations;

        private void Awake()
        {
            m_cachedUsers = new Dictionary<string, UserInfo>();
            m_userNameIDs = new Dictionary<string, string>();
            m_cachedUserConversations = new Dictionary<string, string>();
            CurrentUser = null;
        }

        public void Authenticate(string user, string password)
        {
            m_auth.Request(new PostAuthenticationRequest(user, password, (string userID) =>
            {
                m_users.Request(new GetUserInfoRequest(userID, (UserInfo info) =>
                {
                    CurrentUser = info;
                    m_friendsList.DefaultUser = CurrentUser.ID;
                    m_textChat.IsFetcherActive = true;
                    m_friendsList.IsFetcherActive = true;
                    OnAuthentication.Invoke(userID);
                    m_textChat.OnNewMessageFrom += (string id, MessageInfo msg) => {
                        
                        OnNewMessageFrom(id, msg);
                    };
                    m_friendsList.OnNewFriendInvite += OnNewInviteFrom;

                    FetchAllPrivateConversations();
                }));
            }));
        }

        private void FetchAllPrivateConversations()
        {
            m_friendsList.GetAllFriendsIDs(CurrentUser.ID, (HashSet<string> friends) => {

                foreach (string id in friends)
                {
                    m_friendsList.GetConversationIDWith(CurrentUser.ID, id, (string conversation) => {
                        m_textChat.AddConversationToCache(conversation);
                    });
                }
            });
        }

        public void GetFriendIDFromName(string friendName, UnityAction<string> OnGetFriendID, UnityAction OnFailure = default)
        {
            if (m_userNameIDs.ContainsKey(friendName))
            {
                OnGetFriendID(m_userNameIDs[friendName]);
                return;
            }

            foreach(string id in m_cachedUsers.Keys)
            {
                if (!m_userNameIDs.ContainsKey(m_cachedUsers[id].UserName))
                {
                    m_userNameIDs.Add(m_cachedUsers[id].UserName, id);
                }

                if (m_cachedUsers[id].UserName.Equals(friendName))
                {
                    OnGetFriendID(id);
                    return;
                }
            }

            int allFriends = 0;
            m_friendsList.GetAllFriendsIDs(CurrentUser.ID, (HashSet<string> friendsIDs) => {
                
                foreach (string id in friendsIDs)
                {
                    GetUserInfo(id, (UserInfo info) =>
                    {
                        if (!m_cachedUsers.ContainsKey(id))
                        {
                            m_cachedUsers.Add(id, info);
                        }

                        if (!m_userNameIDs.ContainsKey(m_cachedUsers[id].UserName))
                        {
                            m_userNameIDs.Add(m_cachedUsers[id].UserName, id);
                        }

                        if (m_cachedUsers[id].UserName.Equals(friendName))
                        {
                            OnGetFriendID(id);
                            return;
                        }

                        if(++allFriends >= friendsIDs.Count)
                        {
                            OnFailure?.Invoke();
                        }
                    });
                }
            });
        }

        public void SendMessageToCurrentGameChat(string message, UnityAction<bool, string> OnResult = default)
        {
            if (true) // if (on a pas encore de general chat) (va être avec dispatcher)
            {
                OnResult?.Invoke(false, "Cannot send to game chat - you are not in a game yet");
                return;
            }
            SendMessageToConversation("placeholder-general-chat", message, OnResult);
        }

        public void SendMessageToConversation(string conversationID, string message, UnityAction<bool, string> OnResult = default)
        {
            m_textChat.SendMessageToConversation(CurrentUser.ID, conversationID, message, OnResult);
        }

        public void SendMessageToUser(string otherUserID, string message, UnityAction<bool, string> OnResult = default)
        {
            if (otherUserID.Equals(CurrentUser.ID))
            {
                OnResult?.Invoke(false, "Cannot send a message to yourself");
                return;
            }

            if (m_cachedUserConversations.ContainsKey(otherUserID))
            {
                m_textChat.SendMessageToConversation(CurrentUser.ID, m_cachedUserConversations[otherUserID], message, OnResult);
            }
            else
            {
                m_friendsList.GetConversationIDWith(CurrentUser.ID, otherUserID, (string conversationID) => {
                    m_cachedUserConversations.Add(otherUserID, conversationID);
                    m_textChat.SendMessageToConversation(CurrentUser.ID, m_cachedUserConversations[otherUserID], message, OnResult);
                });
            }
        }
        
        public void GetUserInfo(string userID, OnGetInfo callback)
        {
            if (m_cachedUsers.ContainsKey(userID))
            {
                callback(m_cachedUsers[userID]);
            }
            else
            {
                m_users.Request(new GetUserInfoRequest(userID, (UserInfo info) => {
                    m_cachedUsers.Add(userID, info);
                    callback(info);
                }));
            }
        }
    }
}
