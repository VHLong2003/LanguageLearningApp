using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LanguageLearningApp.Models
{
    public class UsersModel
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; } // "User" or "Admin"

        [JsonProperty("points")]
        public int Points { get; set; }

        [JsonProperty("coins")]
        public int Coins { get; set; }

        [JsonProperty("friendIds")]
        public List<string> FriendIds { get; set; } = new List<string>();

        [JsonProperty("badgeIds")]
        public List<string> BadgeIds { get; set; } = new List<string>();

        [JsonProperty("purchasedItemIds")]
        public List<string> PurchasedItemIds { get; set; } = new List<string>();

        [JsonProperty("dateJoined")]
        public DateTime DateJoined { get; set; }

        [JsonProperty("lastActive")]
        public DateTime LastActive { get; set; }

        [JsonProperty("currentStreak")]
        public int CurrentStreak { get; set; }
    }
}
