using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace ubv.microservices
{
    public class MicroservicesController : MonoBehaviour
    {
        public UserInfo CurrentUser { get; private set; }

        [SerializeField]
        private AuthenticationService m_auth;
        [SerializeField]
        private UserService m_users;
        [SerializeField]
        private TextChatService m_textChat;

        public UnityAction OnAuthentication;

        public void Authenticate(string user, string password)
        {
            m_auth.Request(new PostAuthenticationRequest(user, password, (string userID) =>
            {
                m_users.Request(new GetUserInfoRequest(userID, (UserInfo info) =>
                {
                    CurrentUser = info;
                    OnAuthentication.Invoke();
                }));
            }));
        }

        public void SendMessageTo(string otherUserID, string message)
        {
            m_textChat.SendMessageTo(CurrentUser.ID, otherUserID, message);
        }
        
    }
}
