using Newtonsoft.Json;

namespace LanguageLearningApp.Models
{
    public enum BadgeTier
    {
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond
    }

    public class BadgeModel
    {
        [JsonProperty("badgeId")]
        public string BadgeId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("criteria")]
        public string Criteria { get; set; }

        [JsonProperty("requiredValue")]
        public int RequiredValue { get; set; }

        [JsonProperty("pointsReward")]
        public int PointsReward { get; set; }

        [JsonProperty("coinsReward")]
        public int CoinsReward { get; set; }

        [JsonProperty("tier")]
        public BadgeTier Tier { get; set; }

        [JsonProperty("isHidden")]
        public bool IsHidden { get; set; }
    }
}
