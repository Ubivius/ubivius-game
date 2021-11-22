using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace ubv.microservices
{
    public class ConversationInfo
    {
        public readonly string[] UserIDs;
        public readonly string GameID;

        public ConversationInfo(string[] userIDs, string gameID)
        {
            UserIDs = userIDs;
            GameID = gameID;
        }
    }

    public class MessageInfo
    {
        public readonly string MessageID;
        public readonly string UserID;
        public readonly string ConversationID;
        public readonly string Text;
        public readonly System.DateTime CreatedOn;

        public MessageInfo(string id, string userID, string conversationID, string text, System.DateTime createdOn)
        {
            MessageID = id;
            UserID = userID;
            ConversationID = conversationID;
            Text = text;
            CreatedOn = createdOn;
        }
    }

    [System.Serializable]
    public struct JSONMessage
    {
        public string id;
        public string user_id;
        public string conversation_id;
        public string text;
        public string created_on;
    }


    [System.Serializable]
    public struct JSONConversation
    {
        public string[] user_id;
        public string game_id;
    }

    [System.Serializable]
    public struct JSONConversationUpdate
    {
        public string id;
        public string[] user_id;
        public string game_id;
    }

    [System.Serializable]
    public struct JSONConversationInfo
    {
        public string id;
        public string[] user_id;
        public string game_id;
        public string created_on;
        public string updated_on;
    }

    public delegate void OnGetConversationRequest(ConversationInfo info);
    public delegate void OnGetMessagesRequest(MessageInfo[] infos);

    public abstract class GetTextChatRequest : GetMicroserviceRequest { }

    public class GetMessagesRequest : GetTextChatRequest
    {
        private readonly string m_conversationID;
        public readonly OnGetMessagesRequest Callback;

        public GetMessagesRequest(string conversationID, OnGetMessagesRequest callback)
        {
            m_conversationID = conversationID;
            Callback = callback;
        }

        public override string URL()
        {
            return "messages/conversation/" + m_conversationID;
        }
    }

    public class GetConversationInfoRequest : GetTextChatRequest
    {
        private readonly string m_conversationID;
        public readonly OnGetConversationRequest Callback;

        public GetConversationInfoRequest(string conversationID, OnGetConversationRequest callback)
        {
            m_conversationID = conversationID;
            Callback = callback;
        }

        public override string URL()
        {
            return "conversations/" + m_conversationID;
        }
    }

    public abstract class PostTextChatRequest : PostMicroserviceRequest
    {
        public PostTextChatRequest(UnityAction<string> failCallback) : base(failCallback)
        {
        }
    }

    public class PostMessageRequest : PostTextChatRequest
    {
        public readonly UnityAction Callback;

        private readonly string m_userID;
        private readonly string m_conversationID;
        private readonly string m_text;
        
        public PostMessageRequest(string userID, string conversationID, string text, UnityAction successCallback, UnityAction<string> failCallback) : base(failCallback)
        {
            m_userID = userID;
            m_conversationID = conversationID;
            m_text = text;
            Callback = successCallback;
        }

        public override string JSONString()
        {
            return JsonUtility.ToJson(new JSONMessage
            {
                user_id = m_userID,
                conversation_id = m_conversationID,
                text = m_text
            }).ToString();
        }

        public override string URL()
        {
            return "messages";
        }
    }

    public class PostConversationRequest : PostTextChatRequest
    {
        public readonly UnityAction<string> Callback;
        private readonly string[] m_users;
        private readonly string m_gameID;

        public PostConversationRequest(string gameID, string[] users, UnityAction<string> successCallback, UnityAction<string> failCallback) : base(failCallback)
        {
            m_users = users;
            m_gameID = gameID;
            Callback = successCallback;
        }

        public override string JSONString()
        {
            return JsonUtility.ToJson(new JSONConversation
            {
                user_id = m_users,
                game_id = m_gameID
            }).ToString();
        }

        public override string URL()
        {
            return "conversations";
        }
    }

    public class PutTextChatRequest : PutMicroserviceRequest
    {
        public readonly UnityAction Callback;

        private readonly string m_conversationID;
        private readonly string[] m_users;
        private readonly string m_gameID;

        public PutTextChatRequest(string conversationID, string gameID, string[] users, UnityAction successCallback, UnityAction<string> failCallback) : base(failCallback)
        {
            m_conversationID = conversationID;
            m_users = users;
            m_gameID = gameID;
            Callback = successCallback;
        }

        public override string JSONString()
        {
            return JsonUtility.ToJson(new JSONConversationUpdate
            {
                id = m_conversationID,
                user_id = m_users,
                game_id = m_gameID
            }).ToString();
        }

        public override string URL()
        {
            return "conversations";
        }
    }
}
