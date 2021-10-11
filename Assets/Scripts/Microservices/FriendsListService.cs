using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ubv.microservices
{
    public class FriendsListService : Microservice<GetFriendRequest, 
        PostMicroserviceRequest, PutMicroserviceRequest, DeleteMicroserviceRequest>
    {
        [Serializable]
        public struct JSONFriendInfo
        {
            public string user_id;
            public string relationship_type;
        }

        [Serializable]
        public struct JSONRelationInfo
        {
            public string id;
            public JSONFriendInfo user_1;
            public JSONFriendInfo user_2;
            public string conversation_id;
        }

        protected override void OnGetResponse(string JSON, GetFriendRequest originalRequest)
        {
            string JSONFixed = JsonHelper.FixJsonArrayFromServer(JSON);
            JSONRelationInfo[] relationsResponse = null;
            try
            {
                relationsResponse = JsonHelper.FromJson<JSONRelationInfo>(JSONFixed);
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

        public List<string> GetAllConfirmedFriends(RelationInfo[] relations)
        {
            List<string> friends = new List<string>();
            foreach(RelationInfo relation in relations)
            {
                if (relation.RelationType == RelationInfo.RelationshipType.Friend)
                {
                    friends.Add(relation.FriendUserID);
                }
            }
            return friends;
        }

        public List<string> GetAllInvites(RelationInfo[] relations)
        {
            List<string> invites = new List<string>();
            foreach (RelationInfo relation in relations)
            {
                if (relation.RelationType == RelationInfo.RelationshipType.PendingIncoming)
                {
                    invites.Add(relation.FriendUserID);
                }
            }
            return invites;
        }

        [SerializeField]
        private AuthenticationService m_auth;
        [SerializeField]
        private UserService m_users;

        public void Start()
        {
            m_auth.Request(new PostAuthenticationRequest("murphy", "password", (string id) => {
                
                // friends list test
                this.Request(new GetRelationsFromUserRequest(id, (RelationInfo[] infos) =>
                {
                    m_users.Request(new GetUserInfoRequest(id, (UserInfo userInfo) =>
                    {
                        Debug.Log(userInfo.UserName + "'s friends :");
                    }));

                    List<string> list = GetFriendsFrom(infos);
                    foreach (string friendID in list)
                    {
                        m_users.Request(new GetUserInfoRequest(friendID, (UserInfo userInfo) => 
                        {
                            Debug.Log(userInfo.UserName);
                        }));
                    }
                }
                ));
            }));
        }

        public List<string> GetFriendsFrom(RelationInfo[] infos)
        {
            List<string> friends = GetAllConfirmedFriends(infos);
            return friends;
        }
    }
}