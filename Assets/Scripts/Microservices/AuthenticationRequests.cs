using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace ubv.microservices
{
    public delegate void OnLogin(string playerIDString);

    public struct JSONAuthentificationCredentials
    {
        public string username;
        public string password;
    }

    public class PostAuthenticationRequest : PostMicroserviceRequest
    {
        public readonly string User;
        public readonly string Pass;
        public readonly OnLogin Success;

        public PostAuthenticationRequest(string User, string Pass, OnLogin callback, UnityAction<string> fail) : base(fail)
        {
            this.User = User;
            this.Pass = Pass;
            Success = callback;
        }

        public override string JSONString()
        {
            return JsonUtility.ToJson(new JSONAuthentificationCredentials
            {
                username = User,
                password = Pass,
            }).ToString();
        }

        public override string URL()
        {
            return "signin";
        }
    }
}
