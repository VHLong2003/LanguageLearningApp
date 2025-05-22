using Newtonsoft.Json;

namespace LanguageLearningApp.Models
{
    public enum ItemType
    {
        Avatar,
        Theme,
        PowerUp,
        Customization,
        Decoration,
        Special
    }

    public class ShopItemModel
    {
        [JsonProperty("itemId")]
        public string ItemId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }

        [JsonProperty("type")]
        public ItemType Type { get; set; }

        [JsonProperty("isLimited")]
        public bool IsLimited { get; set; }

        [JsonProperty("availableQuantity")]
        public int AvailableQuantity { get; set; }

        [JsonProperty("requiredLevel")]
        public int RequiredLevel { get; set; }

        [JsonProperty("effectDescription")]
        public string EffectDescription { get; set; }

        [JsonProperty("durationDays")]
        public int DurationDays { get; set; } // 0 means permanent
    }
}
