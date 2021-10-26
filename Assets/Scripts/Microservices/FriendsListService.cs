using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

namespace ubv.microservices
{
    public class FriendsListService : Microservice<GetFriendRequest, 
        PostInviteRequest, PutMicroserviceRequest, DeleteMicroserviceRequest>
    {
        [SerializeField]
        private MicroserviceAutoFetcher m_invitesFetcher;

        [HideInInspector]
        public string DefaultUser;

        private HashSet<string> m_currentPendingIncomingInvites;
        public UnityAction<string> OnNewFriendInvite;

        public bool IsFetcherActive;

        private void Awake()
        {
            m_currentPendingIncomingInvites = new HashSet<string>();
            m_invitesFetcher.FetchLogic += Fetch;
            IsFetcherActive = false;
        }

        private void Fetch()
        {
            if (!IsFetcherActive)
            {
                m_invitesFetcher.ReadyForNewFetch();
                return;
            }

             this.Request(new GetInvitesForUserRequest(DefaultUser, OnGetNewInvites));
        }

        public void SendInviteTo(string userID)
        {
            this.Request(new PostInviteRequest(DefaultUser, userID));
        }

        public void GetAllFriendsIDs(string userID, UnityAction<HashSet<string>> OnGetFriendIDs)
        {
            this.Request(new GetRelationsFromUserRequest(userID, (RelationInfo[] relations) => 
            {
                OnGetFriendIDs(GetAllConfirmedFriends(relations));
            }));
        }

        private void OnGetNewInvites(RelationInfo[] infos)
        {
            HashSet<string> invites = GetAllInvites(infos);
            foreach(string friend in invites)
            {
                if (!m_currentPendingIncomingInvites.Contains(friend))
                {
                    OnNewFriendInvite(friend);
                    m_currentPendingIncomingInvites.Add(friend);
                }
            }
            m_invitesFetcher.ReadyForNewFetch();
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
                RelationInfo.RelationshipType relationshipType;
                switch (relationsResponse[i].user_2.relationship_type)
                {
                    case "Friend":
                        relationshipType = RelationInfo.RelationshipType.Friend;
                        break;
                    case "Blocked":
                        relationshipType = RelationInfo.RelationshipType.Blocked;
                        break;
                    case "PendingIncoming":
                        relationshipType = RelationInfo.RelationshipType.PendingIncoming;
                        break;
                    case "PendingOutgoing":
                        relationshipType = RelationInfo.RelationshipType.PendingOutgoing;
                        break;
                    default:
                        relationshipType = RelationInfo.RelationshipType.None;
                        break;
                }
                relations[i] = new RelationInfo(relationsResponse[i].id, relationsResponse[i].user_2.user_id, relationshipType, relationsResponse[i].conversation_id);
            }

            originalRequest.Callback.Invoke(relations);
        }

        public void GetConversationWith(string currentUserID, string otherUserID, UnityAction<string> OnGetConversation)
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

        private HashSet<string> GetAllConfirmedFriends(RelationInfo[] relations)
        {
            HashSet<string> friends = new HashSet<string>();
            foreach(RelationInfo relation in relations)
            {
                if (relation.RelationType == RelationInfo.RelationshipType.Friend)
                {
                    friends.Add(relation.FriendUserID);
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
                    m_users.Request(new GetUserInfoRequest(id, (UserInfo userInfo) =>
                    {
                        Debug.Log("ID:" + userInfo.ID);
                        Debug.Log(userInfo.UserName + "'s friends :");
                    }));
                    
                    foreach (RelationInfo info in infos)
                    {
                        m_users.Request(new GetUserInfoRequest(info.FriendUserID, (UserInfo userInfo) => 
                        {
                            Debug.Log("Conversation :" + info.ConversationID);
                            Debug.Log(" with user " + userInfo.UserName);
                        }));
                    }
                }
                ));
            }));
        }
#endif // UNITY_EDITOR
    }
}
