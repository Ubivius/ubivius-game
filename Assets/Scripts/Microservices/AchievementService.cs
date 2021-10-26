using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubv.microservices
{
    public class AchievementService : Microservice<GetAchievementRequest,
        PostMicroserviceRequest, PutMicroserviceRequest, DeleteMicroserviceRequest>
    {
        public class Achievement
        {
            public string Id { get; private set; }
            public string Name { get; private set; }
            public string Condition { get; private set; }
            public string Description { get; private set; }
            public string SpriteID { get; private set; }

            public Achievement(string id, string name, string condition, string description, string sprite_id)
            {
                Id = id;
                Name = name;
                Condition = condition;
                Description = description;
                SpriteID = sprite_id;
            }
        }
        [System.Serializable]
        private struct JSONAchievement
        {
            public string id;
            public string name;
            public string condition;
            public string description;
            public string sprite_id;
        }

        protected override void OnGetResponse(string JSON, GetAchievementRequest originalRequest)
        {
            if (originalRequest is GetAchievementRequest)
            {
                string JSONFixed = JsonHelper.FixJsonArrayFromServer(JSON);
                JSONAchievement[] jsonDataArray = JsonHelper.ArrayFromJsonString<JSONAchievement>(JSONFixed);

                Achievement[] achievements = new Achievement[jsonDataArray.Length];
                for (int i = 0; i < jsonDataArray.Length; i++)
                {
                    achievements[i] = new Achievement(jsonDataArray[i].id, jsonDataArray[i].name, jsonDataArray[i].condition, jsonDataArray[i].description, jsonDataArray[i].sprite_id);
                }

                originalRequest.Callback.Invoke(achievements);
            }
            else if (originalRequest is GetSingleAchievementRequest)
            {
                JSONAchievement jsonData = JsonUtility.FromJson<JSONAchievement>(JSON);
                Achievement[] achievements = new Achievement[1];
                achievements[0] = new Achievement(jsonData.id, jsonData.name, jsonData.condition, jsonData.description, jsonData.sprite_id);
                originalRequest.Callback.Invoke(achievements);
            }
        }

#if UNITY_EDITOR
        [SerializeField]
        private UserService m_users;

        public void Start()
        {
            TestWithGodefroy();
        }

        public void TestWithGodefroy()
        {
            this.Request(new GetAllAchievementsRequest((Achievement[] achievements) =>
            {
                foreach(Achievement achievement in achievements)
                {
                    Debug.Log("Achievement ID: " + achievement.Id);
                    Debug.Log("  Name: " + achievement.Name);
                    Debug.Log("  Condition: " + achievement.Condition);
                    Debug.Log("  Description: " + achievement.Description);
                    Debug.Log("  SpriteID: " + achievement.SpriteID);
                }
            }));
        }
#endif // UNITY_EDITOR
    }
}
