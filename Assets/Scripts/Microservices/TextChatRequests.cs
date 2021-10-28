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

    public class PostTextChatRequest : PostMicroserviceRequest
    {
        private readonly string m_userID;
        private readonly string m_conversationID;
        private readonly string m_text;

        public readonly UnityAction<bool, string> Callback;

        public PostTextChatRequest(string userID, string conversationID, string text, UnityAction<bool, string> callback)
        {
            m_userID = userID;
            m_conversationID = conversationID;
            m_text = text;
            Callback = callback;
        }

        public override string JSONString()
        {
            return JsonUtility.ToJson(new JSONMessage {
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
}
