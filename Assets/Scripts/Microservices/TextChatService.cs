using UnityEngine;
using UnityEditor;

namespace ubv.microservices
{
    public class TextChatService : Microservice<GetTextChatRequest, 
        PostTextChatRequest, PutMicroserviceRequest, DeleteMicroserviceRequest>
    {
        public struct JSONConversationInfo
        {
            public string[] user_id;
            public string game_id;
        }

        protected override void OnGetResponse(string JSON, GetTextChatRequest originalRequest)
        {
            if (originalRequest is GetMessagesRequest msgReq)
            {
                string JSONFixed = JsonHelper.FixJsonArrayFromServer(JSON);
                JSONMessage[] messages = JsonHelper.FromJson<JSONMessage>(JSONFixed);

                MessageInfo[] infos = new MessageInfo[messages.Length];
                for (int i = 0; i < messages.Length; i++)
                {
                    infos[i] = new MessageInfo(infos[i].UserID, infos[i].ConversationID, infos[i].Text);
                }

                msgReq.Callback.Invoke(infos);
            }
            else if (originalRequest is GetConversationInfoRequest convReq)
            {
                JSONConversationInfo conversationJSON = JsonUtility.FromJson<JSONConversationInfo>(JSON);
                
                convReq.Callback.Invoke(new ConversationInfo(conversationJSON.user_id, conversationJSON.game_id));
            }
        }

        protected override void OnPostResponse(string JSON, PostTextChatRequest originalRequest)
        {

        }
    }
}