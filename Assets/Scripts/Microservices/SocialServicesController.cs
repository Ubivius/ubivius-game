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
        // sender, receiver, Message
        public UnityAction<string, string, MessageInfo> OnNewMessage;
        public UnityAction<string> OnNewInviteFrom;

        private Dictionary<string, UserInfo> m_cachedUsers;
        private Dictionary<string, string> m_userNameIDs;
        private Dictionary<string, string> m_cachedUserConversations;
        private Dictionary<string, string> m_cachedConversationUsers;

        private void Awake()
        {
            m_cachedUsers = new Dictionary<string, UserInfo>();
            m_userNameIDs = new Dictionary<string, string>();
            m_cachedUserConversations = new Dictionary<string, string>();
            m_cachedConversationUsers = new Dictionary<string, string>();
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
                    m_textChat.OnNewMessageInConversation += (string conversationID, MessageInfo msg) => {
                        if (!msg.UserID.Equals(CurrentUser.ID))
                        {
                            OnNewMessage(msg.UserID, CurrentUser.ID, msg);
                        }
                        else
                        {
                            if (m_cachedConversationUsers.ContainsKey(conversationID))
                            {
                                OnNewMessage(CurrentUser.ID, m_cachedConversationUsers[conversationID], msg);
                            }
                            else
                            {
                                GetUserIDFromConversation(conversationID, (string friendID) => 
                                {
                                    m_cachedConversationUsers[conversationID] = friendID;
                                    OnNewMessage(CurrentUser.ID, m_cachedConversationUsers[conversationID], msg);
                                });
                            }
                        }
                    };
                    m_friendsList.OnNewFriendInvite += OnNewInviteFrom;

                    FetchAllPrivateConversations();
                }));
            }));
        }

        private void FetchAllPrivateConversations()
        {
            m_friendsList.GetAllFriends(CurrentUser.ID, (HashSet<RelationInfo> friends) => {

                foreach (RelationInfo info in friends)
                {
                    m_cachedConversationUsers[info.ConversationID] = info.FriendUserID;
                    m_cachedUserConversations[info.FriendUserID] = info.ConversationID;

                    m_friendsList.GetConversationIDWith(CurrentUser.ID, info.FriendUserID, (string conversation) => {
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
            
            GetConversationIDWith(otherUserID, (convID) =>
            {
                m_textChat.SendMessageToConversation(CurrentUser.ID, m_cachedUserConversations[otherUserID], message, OnResult);
            });
        }

        private void GetConversationIDWith(string otherUserID, UnityAction<string> OnGetConversation)
        {
            if (m_cachedUserConversations.ContainsKey(otherUserID))
            {
                m_cachedConversationUsers[m_cachedUserConversations[otherUserID]] = otherUserID;
                OnGetConversation(m_cachedUserConversations[otherUserID]);
                return;
            }

            m_friendsList.GetConversationIDWith(CurrentUser.ID, otherUserID, (string conversationID) => {
                m_cachedUserConversations.Add(otherUserID, conversationID);
                m_cachedConversationUsers.Add(conversationID, otherUserID);
                OnGetConversation(conversationID);
            });
        }

        private void GetUserIDFromConversation(string conversationID, UnityAction<string> OnGetUserID)
        {
            if (m_cachedConversationUsers.ContainsKey(conversationID))
            {
                OnGetUserID(m_cachedConversationUsers[conversationID]);
                return;
            }
            
            m_friendsList.GetAllFriends(CurrentUser.ID, (HashSet<RelationInfo> friends) => {

                foreach (RelationInfo info in friends)
                {
                    m_cachedConversationUsers[info.ConversationID] = info.FriendUserID;
                    m_cachedUserConversations[info.FriendUserID] = info.ConversationID;
                    if(info.ConversationID == conversationID)
                    {
                        OnGetUserID(info.FriendUserID);
                        return;
                    }
                }
            });
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
