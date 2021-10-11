using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ubv.microservices
{
    public class FriendsListService : Microservice<GetFriendRequest, 
        PostMicroserviceRequest, PutMicroserviceRequest, DeleteMicroserviceRequest>
    {
        private struct JSONFriendInfo
        {
            public string user_id;
            public string relationship_type;
        }

        protected override void OnGetResponse(string JSON, GetFriendRequest originalRequest)
        {
            //string JSONFixed = JsonHelper.FixJsonArrayFromServer(JSON);
            JSONFriendInfo[] authResponse = JsonHelper.FromJson<JSONFriendInfo>(JSON);

            RelationInfo[] friends = new RelationInfo[authResponse.Length];
            for (int i = 0; i < authResponse.Length; i++)
            {
                RelationInfo.RelationshipType relationshipType;
                switch (authResponse[i].relationship_type)
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
                friends[i] = new RelationInfo(authResponse[i].user_id, relationshipType);
            }

            originalRequest.Callback.Invoke(friends);
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
        private AuthenticationService m_users;
        public void Start()
        {
            m_users.Request(new PostAuthenticationRequest("murphy", "password", (string id) => {

            Debug.Log("Login in with murphy :" + id);
                // friends list test
                this.Request(new GetRelationsFromUserRequest(id, (RelationInfo[] infos) =>
                {
                    Debug.Log(id + "'s friends:");
                    LogFriends(infos);
                }
                ));
            }));
        }

        public void LogFriends(RelationInfo[] infos)
        {
            List<string> friends = GetAllConfirmedFriends(infos);
            foreach(string friendID in friends)
            {
                Debug.Log("" + friendID);
            }
        }
    }
}