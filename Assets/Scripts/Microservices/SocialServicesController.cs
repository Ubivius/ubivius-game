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

        private void Awake()
        {
            m_cachedUsers = new Dictionary<string, UserInfo>();
            CurrentUser = null;
            m_textChat.OnNewMessageFrom += OnNewMessageFrom;
            m_friendsList.OnNewFriendInvite += OnNewInviteFrom;
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
                }));
            }));
        }

        public void SendMessageTo(string otherUserID, string message)
        {
            m_textChat.SendMessageTo(CurrentUser.ID, otherUserID, message);
        }
        
        public void GetUserInfo(string userID, OnGetInfo callback)
        {
            if (m_cachedUsers.ContainsKey(userID))
            {
                callback(m_cachedUsers[userID]);
            }
            else
            {
                m_users.Request(new GetUserInfoRequest(userID, callback));
            }
        }
    }
}
