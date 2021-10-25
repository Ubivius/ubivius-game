using UnityEngine;
using UnityEditor;

namespace ubv.microservices
{
    public class UserInfo
    {
        public readonly string ID;
        public readonly string UserName;
        public readonly string Email;
        public readonly string DateOfBirth;

        public UserInfo(string id, string userName, string email, string dateOfBirth)
        {
            ID = id;
            UserName = userName;
            Email = email;
            DateOfBirth = dateOfBirth;
        }
    }

    public delegate void OnGetInfo(UserInfo info);

    public class GetUserInfoRequest : GetMicroserviceRequest
    {
        public readonly string ID;
        public readonly OnGetInfo Callback;

        public GetUserInfoRequest(string ID, OnGetInfo callback)
        {
            this.ID = ID;
            Callback = callback;
        }

        public override string URL()
        {
            return "users/" + ID;
        }
    }
}
