using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System;

namespace ubv.microservices
{
    public class UserInfo
    {

        public readonly int ID;
        public readonly string StringID;
        public readonly string UserName;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;
        public readonly string DateOfBirth;
        public StatusType Status;
        public bool Ready = false;

        public UserInfo(string id, string userName, string firstName, string lastName, string email, string dateOfBirth, StatusType status)
        {
            ID = id.GetHashCode();
            StringID = id;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            DateOfBirth = dateOfBirth;
            Status = status;
        }
    }

    public enum StatusType
    {
        Online = 0,
        Offline,
        InLobby,
        InGame
    }

    [Serializable]
    public struct JSONPutUserInfo
    {
        public string id;
        public string username;
        public string firstname;
        public string lastname;
        public string email;
        public string dateOfBirth;
        public string status;
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

    public class PutUserInfoRequest : PutMicroserviceRequest
    {
        private readonly UserInfo m_user;
        public readonly UnityAction Callback;

        public PutUserInfoRequest(UserInfo user, UnityAction callback = default)
        {
            Callback = callback;
            m_user = user;
        }

        public override string JSONString()
        {
            return JsonUtility.ToJson(new JSONPutUserInfo
            {
                id = m_user.StringID,
                username = m_user.UserName,
                firstname = m_user.FirstName,
                lastname = m_user.LastName,
                email = m_user.Email,
                dateOfBirth = m_user.DateOfBirth,
                status = m_user.Status.ToString(),
            }).ToString();
        }

        public override string URL()
        {
            return "users";
        }
    }
}
