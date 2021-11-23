using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using static ubv.microservices.RelationInfo;

namespace ubv.microservices
{
    public class FriendsListService : Microservice<GetFriendRequest, 
        PostInviteRequest, ResponseToInviteRequest, DeleteRelationRequest>
    {
        [SerializeField]
        private MicroserviceAutoFetcher m_invitesFetcher;
        [SerializeField]
        private MicroserviceAutoFetcher m_friendsFetcher;

        [HideInInspector]
        public string DefaultUser;

        private Dictionary<string, RelationInfo> m_cachedInvites;
        private Dictionary<string, RelationInfo> m_cachedFriends;
        public UnityAction<RelationInfo> OnNewInvite;
        public UnityAction<RelationInfo> OnNewFriend;
        public UnityAction<RelationInfo> UpdateFriend;
        public UnityAction<string> OnDeleteInvite;

        public bool IsFetcherActive;

        private void Awake()
        {
            m_cachedInvites = new Dictionary<string, RelationInfo>();
            m_cachedFriends = new Dictionary<string, RelationInfo>();
            m_invitesFetcher.FetchLogic += FetchInvites;
            m_friendsFetcher.FetchLogic += FetchFriends;
            IsFetcherActive = false;
        }

        private void FetchInvites()
        {
            if (!IsFetcherActive)
            {
                m_invitesFetcher.ReadyForNewFetch();
                return;
            }
            this.Request(new GetInvitesForUserRequest(DefaultUser, OnGetInvites));
        }

        private void FetchFriends()
        {
            if (!IsFetcherActive)
            {
                m_friendsFetcher.ReadyForNewFetch();
                return;
            }
            this.Request(new GetRelationsFromUserRequest(DefaultUser, OnGetFriends));
        }

        public void GetAllFriendsIDs(string userID, UnityAction<HashSet<string>> OnGetFriendIDs)
        {
            this.Request(new GetRelationsFromUserRequest(userID, (RelationInfo[] relations) => 
            {
                OnGetFriendIDs(GetAllConfirmedFriendIDs(relations));
            }));
        }

        public void GetAllFriends(string userID, UnityAction<HashSet<RelationInfo>> OnGetFriends)
        {
            this.Request(new GetRelationsFromUserRequest(userID, (RelationInfo[] relations) =>
            {
                OnGetFriends(GetAllConfirmedFriends(relations));
            }));
        }

        private void OnGetInvites(RelationInfo[] infos)
        {
            foreach (RelationInfo invite in infos)
            {
                if (!m_cachedInvites.ContainsKey(invite.RelationID))
                {
                    OnNewInvite(invite);
                    m_cachedInvites.Add(invite.RelationID, invite);
                }
            }
            m_invitesFetcher.ReadyForNewFetch();
        }

        private void OnGetFriends(RelationInfo[] infos)
        {
            foreach (RelationInfo friend in infos)
            {
                if (!m_cachedFriends.ContainsKey(friend.RelationID))
                {
                    OnNewFriend(friend);
                    m_cachedFriends.Add(friend.RelationID, friend);
                }
                else
                {
                    if (m_cachedFriends[friend.RelationID].FriendStatus != friend.FriendStatus)
                    {
                        UpdateFriend(friend);
                        m_cachedFriends[friend.RelationID] = friend;
                    }
                }
            }
            m_friendsFetcher.ReadyForNewFetch();
        }

        protected override void OnPostResponse(string JSON, PostInviteRequest originalRequest)
        {
            originalRequest.Callback?.Invoke();
        }

        protected override void OnGetResponse(string JSON, GetFriendRequest originalRequest)
        {
            string JSONFixed = JsonHelper.FixJsonArrayFromServer(JSON);
            JSONRelationInfo[] relationsResponse = null;
            try
            {
                relationsResponse = JsonHelper.ArrayFromJsonString<JSONRelationInfo>(JSONFixed);
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
                return;
            }

            RelationInfo[] relations = new RelationInfo[relationsResponse.Length];
            for (int i = 0; i < relationsResponse.Length; i++)
            {
                JSONDetailedFriendInfo otherUser = relationsResponse[i].user; 

                RelationshipType relationshipType = (RelationshipType)Enum.Parse(typeof(RelationshipType), otherUser.relationship_type, true);
                StatusType statusType = (StatusType)Enum.Parse(typeof(StatusType), otherUser.status, true);

                relations[i] = new RelationInfo(
                    relationsResponse[i].id, 
                    otherUser.id, 
                    otherUser.username, 
                    statusType, 
                    relationshipType, 
                    relationsResponse[i].conversation_id, 
                    relationsResponse[i].created_on, 
                    relationsResponse[i].updated_on
                );
            }

            originalRequest.Callback.Invoke(relations);
        }

        public void GetConversationIDWith(string currentUserID, string otherUserID, UnityAction<string> OnGetConversation)
        {
            this.Request(new GetRelationsFromUserRequest(currentUserID, (RelationInfo[] infos) => {
                foreach(RelationInfo info in infos)
                {
                    if(info.FriendUserID == otherUserID)
                    {
                        OnGetConversation(info.ConversationID);
                        return;
                    }
                    OnGetConversation(null);
                }
            }));
        }

        private HashSet<string> GetAllConfirmedFriendIDs(RelationInfo[] relations)
        {
            HashSet<string> friends = new HashSet<string>();
            foreach(RelationInfo relation in relations)
            {
                if (relation.RelationType == RelationshipType.Friend)
                {
                    friends.Add(relation.FriendUserID);
                }
            }
            return friends;
        }

        private HashSet<RelationInfo> GetAllConfirmedFriends(RelationInfo[] relations)
        {
            HashSet<RelationInfo> friends = new HashSet<RelationInfo>();
            foreach (RelationInfo relation in relations)
            {
                if (relation.RelationType == RelationshipType.Friend)
                {
                    friends.Add(relation);
                }
            }
            return friends;
        }

        private HashSet<string> GetAllInvites(RelationInfo[] relations)
        {
            HashSet<string> invites = new HashSet<string>();
            foreach (RelationInfo relation in relations)
            {
                if (relation.RelationType == RelationInfo.RelationshipType.PendingIncoming)
                {
                    invites.Add(relation.FriendUserID);
                }
            }
            return invites;
        }

        protected override void OnPutResponse(string JSON, ResponseToInviteRequest originalRequest)
        {
            originalRequest.Callback?.Invoke();
        }

        protected override void OnDeleteResponse(string JSON, DeleteRelationRequest originalRequest)
        {
            originalRequest.Callback?.Invoke();
        }

#if UNITY_EDITOR
        [SerializeField]
        private AuthenticationService m_auth;
        [SerializeField]
        private UserService m_users;

        public void Start()
        {
            //TestWithMurphy();
        }

        public void TestWithMurphy()
        {
            m_auth.Request(new PostAuthenticationRequest("murphy", "password", (string id) => {
                
                // friends list test
                this.Request(new GetRelationsFromUserRequest(id, (RelationInfo[] infos) =>
                {
                    m_users.Request(new GetUserInfoByIDRequest(id, (UserInfo userInfo) =>
                    {
                        Debug.Log("ID:" + userInfo.StringID);
                        Debug.Log(userInfo.UserName + "'s friends :");
                    }));
                    
                    foreach (RelationInfo info in infos)
                    {
                        m_users.Request(new GetUserInfoByIDRequest(info.FriendUserID, (UserInfo userInfo) => 
                        {
                            Debug.Log("Conversation :" + info.ConversationID);
                            Debug.Log(" with user " + userInfo.UserName);
                        }));
                    }
                }
                ));
            }, 
            (string err) => {
                Debug.LogError(err);
            }
            ));
        }
#endif // UNITY_EDITOR
    }
}
