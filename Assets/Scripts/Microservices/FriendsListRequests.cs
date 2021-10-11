using UnityEngine;
using UnityEditor;

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
}