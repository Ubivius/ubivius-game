using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Events;

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
        public readonly RelationshipType RelationType;
        public readonly string ConversationID;

        public RelationInfo(string relationID, string friendID, RelationshipType relationship, string conversationID)
        {
            RelationID = relationID;
            FriendUserID = friendID; ;
            RelationType = relationship;
            ConversationID = conversationID;
        }
    }
    
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
}
