using UnityEngine;
using UnityEditor;

namespace ubv.microservices
{
    public class FriendsInfo
    {
        public readonly string UserID;
        // todo : add data
    }

    public delegate void OnGetFriendsRequest(FriendsInfo friendsInfo);
    abstract public class GetFriendRequest : GetMicroserviceRequest
    {
        public readonly string UserID;
        public readonly FriendsInfo Callback;

        public GetFriendRequest(string user, FriendsInfo callback)
        {
            UserID = user;
            Callback = callback;
        }
    }

    public class GetFriendsFromUserRequest : GetFriendRequest
    {
        public GetFriendsFromUserRequest(string user, FriendsInfo callback) : base(user, callback)
        { }

        public override string URL()
        {
            return "friends/" + UserID;
        }
    }

    public class GetInvitesForUserRequest : GetFriendRequest
    {
        public GetInvitesForUserRequest(string user, FriendsInfo callback) : base(user, callback)
        { }

        public override string URL()
        {
            return "invites/" + UserID;
        }
    }
}