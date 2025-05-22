using System;
using Newtonsoft.Json;

namespace LanguageLearningApp.Models
{
    public class LeaderboardModel
    {
        [JsonProperty("entryId")]
        public string EntryId { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("totalPoints")]
        public int TotalPoints { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("previousRank")]
        public int PreviousRank { get; set; }

        [JsonProperty("weeklyPoints")]
        public int WeeklyPoints { get; set; }

        [JsonProperty("monthlyPoints")]
        public int MonthlyPoints { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTime LastUpdated { get; set; }

        [JsonProperty("streak")]
        public int Streak { get; set; }
    }
}
