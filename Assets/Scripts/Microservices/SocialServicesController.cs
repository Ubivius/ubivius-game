using UnityEngine;
using System.Collections;
using UnityEngine.Events;

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

        public UserService UserService {
            get
            {
                return m_users;
            }
        }

        public UnityAction<string> OnAuthentication;
        public UnityAction<string, MessageInfo> OnNewMessageFrom;

        private void Awake()
        {
            CurrentUser = null;
            m_textChat.OnNewMessageFrom += OnNewMessageFrom;
        }

        public void Authenticate(string user, string password)
        {
            m_auth.Request(new PostAuthenticationRequest(user, password, (string userID) =>
            {
                m_users.Request(new GetUserInfoRequest(userID, (UserInfo info) =>
                {
                    CurrentUser = info;
                    OnAuthentication.Invoke(userID);
                }));
            }));
        }

        public void SendMessageTo(string otherUserID, string message)
        {
            m_textChat.SendMessageTo(CurrentUser.ID, otherUserID, message);
        }
        
    }
}
