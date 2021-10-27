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
                    m_textChat.OnNewMessageFrom += OnNewMessageFrom;
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

        public void GetFriendIDFromName(string friendName, UnityAction<string> OnGetFriendID)
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

            m_friendsList.GetAllFriendsIDs(CurrentUser.ID, (HashSet<string> friendsIDs) => {
                foreach (string id in friendsIDs)
                {
                    GetUserInfo(id, (UserInfo info) =>
                    {
                        if (info.UserName.Equals(friendName))
                        {
                            OnGetFriendID(id);
                        }
                    });
                }
            });
        }

        public void SendMessageTo(string otherUserID, string message)
        {
            if (m_cachedUserConversations.ContainsKey(otherUserID))
            {
                m_textChat.SendMessageToConversation(CurrentUser.ID, m_cachedUserConversations[otherUserID], message);
            }
            else
            {
                m_friendsList.GetConversationIDWith(CurrentUser.ID, otherUserID, (string conversationID) => {
                    m_cachedUserConversations.Add(otherUserID, conversationID);
                    m_textChat.SendMessageToConversation(CurrentUser.ID, m_cachedUserConversations[otherUserID], message);
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
