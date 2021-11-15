using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubv.microservices
{
    public delegate void OnGetAchievement(AchievementService.Achievement[] achievements);

    abstract public class GetAchievementRequest : GetMicroserviceRequest
    {
        public readonly OnGetAchievement Callback;

        public GetAchievementRequest(OnGetAchievement callback)
        {
            Callback = callback;
        }

        public override string URL()
        {
            return "achievements";
        }
    }

    public class GetAllAchievementsRequest : GetAchievementRequest
    {
        public GetAllAchievementsRequest(OnGetAchievement callback) : base(callback)
        { }
    }

    public class GetSingleAchievementRequest : GetAchievementRequest
    {
        private readonly string m_url;
        public GetSingleAchievementRequest(string AchievementID, OnGetAchievement callback) : base(callback)
        {
            this.m_url = AchievementID;
        }

        public override string URL()
        {
            return "achievements/" + m_url;
        }
    }
}
