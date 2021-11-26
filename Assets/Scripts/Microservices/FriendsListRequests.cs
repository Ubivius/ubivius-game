using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Events;
using static ubv.microservices.RelationInfo;

namespace ubv.microservices
{
    public class RelationInfo
    {
        public enum RelationshipType
        {
            None = 0,
            Friend,
            Blocked,
            PendingIncoming,
            PendingOutgoing
        }

        public readonly string RelationID;
        public readonly string FriendUserID;
        public readonly string FriendUsername;
        public readonly StatusType FriendStatus;
        public readonly RelationshipType RelationType;
        public readonly string ConversationID;
        public readonly string CreatedOn;
        public readonly string UpdatedOn;

        public RelationInfo(string relationID, string friendID, string friendUsername, StatusType friendStatus, RelationshipType relationship, string conversationID, string createdOn, string updatedOn)
        {
            RelationID = relationID;
            FriendUserID = friendID;
            FriendUsername = friendUsername;
            FriendStatus = friendStatus;
            RelationType = relationship;
            ConversationID = conversationID;
            CreatedOn = createdOn;
            UpdatedOn = updatedOn;
        }
    }
    
    [Serializable]
    public struct JSONFriendInfo
    {
        public string user_id;
        public string relationship_type;
    }

    [Serializable]
    public struct JSONDetailedFriendInfo
    {
        public string id;
        public string username;
        public string status;
        public string relationship_type;
    }

    [Serializable]
    public struct JSONRelationInfo
    {
        public string id;
        public JSONDetailedFriendInfo user;
        public string conversation_id;
        public string created_on;
        public string updated_on;
    }

    [Serializable]
    public struct JSONPutRelationInfo
    {
        public string id;
        public JSONFriendInfo user_1;
        public JSONFriendInfo user_2;
        public string conversation_id;
        public string created_on;
        public string updated_on;
    }

    [Serializable]
    public struct JSONPostRelation
    {
        public JSONFriendInfo user_1;
        public JSONFriendInfo user_2;
    }

    public delegate void OnGetFriendsRequest(RelationInfo[] friendsInfo);
    abstract public class GetFriendRequest : GetMicroserviceRequest
    {
        public readonly string UserID;
        public readonly OnGetFriendsRequest Callback;

        public GetFriendRequest(string user, OnGetFriendsRequest callback)
        {
            UserID = user;
            Callback = callback;
        }
    }

    public class GetRelationsFromUserRequest : GetFriendRequest
    {
        public GetRelationsFromUserRequest(string user, OnGetFriendsRequest callback) : base(user, callback)
        { }

        public override string URL()
        {
            return "friends/" + UserID;
        }
    }

    public class GetInvitesForUserRequest : GetFriendRequest
    {
        public GetInvitesForUserRequest(string user, OnGetFriendsRequest callback) : base(user, callback)
        { }

        public override string URL()
        {
            return "invites/" + UserID;
        }
    }

    public class PostInviteRequest : PostMicroserviceRequest
    {
        private readonly string m_user_1;
        private readonly string m_user_2;

        public readonly UnityAction Callback;

        public PostInviteRequest(string user_1, string user_2, UnityAction callback = default)
        {
            Callback = callback;
            m_user_1 = user_1;
            m_user_2 = user_2;
        }

        public override string JSONString()
        {
            return JsonUtility.ToJson(new JSONPostRelation
            {
                user_1 = new JSONFriendInfo
                {
                    user_id = m_user_1,
                    relationship_type = "PendingOutgoing"
                },
                user_2 = new JSONFriendInfo
                {
                    user_id = m_user_2,
                    relationship_type = "PendingIncoming"
                }
            }).ToString();
        }

        public override string URL()
        {
            return "relationships";
        }
    }

    public class ResponseToInviteRequest : PutMicroserviceRequest
    {
        private readonly string m_user_id;
        private readonly RelationInfo m_relation;
        private readonly RelationshipType m_response;

        public readonly UnityAction Callback;

        public ResponseToInviteRequest(string userID, RelationInfo relation, RelationshipType response, UnityAction callback = default)
        {
            Callback = callback;
            m_user_id = userID;
            m_relation = relation;
            m_response = response;
        }

        public override string JSONString()
        {
            RelationshipType m_friendRelationType = m_relation.RelationType;
            if (m_response == RelationshipType.Friend)
            {
                m_friendRelationType = RelationshipType.Friend;
            }

            return JsonUtility.ToJson(new JSONPutRelationInfo
            {
                id = m_relation.RelationID,
                user_1 = new JSONFriendInfo
                {
                    user_id = m_user_id,
                    relationship_type = m_response.ToString()
                },
                user_2 = new JSONFriendInfo
                {
                    user_id = m_relation.FriendUserID,
                    relationship_type = m_friendRelationType.ToString()
                },
                conversation_id = m_relation.ConversationID,
                created_on = m_relation.CreatedOn,
                updated_on = m_relation.UpdatedOn
            }).ToString();
        }

        public override string URL()
        {
            return "relationships";
        }
    }

    public class DeleteRelationRequest : DeleteMicroserviceRequest
    {
        private readonly string m_relationID;

        public readonly UnityAction Callback;

        public DeleteRelationRequest(string relationID, UnityAction callback = default)
        {
            m_relationID = relationID;
            Callback = callback;
        }

        public override string URL()
        {
            return "relationships/" + m_relationID;
        }
    }
}
