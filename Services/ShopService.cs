using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LanguageLearningApp.Helpers;
using LanguageLearningApp.Models;
using Newtonsoft.Json;

namespace LanguageLearningApp.Services
{
    public class ShopService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseConfig _firebaseConfig;
        private readonly UserService _userService;

        public ShopService(FirebaseConfig firebaseConfig, UserService userService)
        {
            _httpClient = new HttpClient();
            _firebaseConfig = firebaseConfig;
            _userService = userService;
        }

        public async Task<List<ShopItemModel>> GetAllShopItemsAsync(string idToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/shop.json?auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var itemsDictionary = JsonConvert.DeserializeObject<Dictionary<string, ShopItemModel>>(content);

                    if (itemsDictionary == null)
                        return new List<ShopItemModel>();

                    var items = new List<ShopItemModel>();
                    foreach (var kvp in itemsDictionary)
                    {
                        var item = kvp.Value;
                        item.ItemId = kvp.Key;
                        items.Add(item);
                    }

                    return items;
                }

                return new List<ShopItemModel>();
            }
            catch
            {
                return new List<ShopItemModel>();
            }
        }

        public async Task<List<ShopItemModel>> GetItemsByTypeAsync(ItemType type, string idToken)
        {
            try
            {
                var allItems = await GetAllShopItemsAsync(idToken);
                return allItems.FindAll(i => i.Type == type);
            }
            catch
            {
                return new List<ShopItemModel>();
            }
        }

        public async Task<ShopItemModel> GetShopItemByIdAsync(string itemId, string idToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_firebaseConfig.DatabaseUrl}/shop/{itemId}.json?auth={idToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var item = JsonConvert.DeserializeObject<ShopItemModel>(content);

                    if (item != null)
                        item.ItemId = itemId;

                    return item;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> CreateShopItemAsync(ShopItemModel item, string idToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(item);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"{_firebaseConfig.DatabaseUrl}/shop.json?auth={idToken}",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

                    if (result.ContainsKey("name"))
                        return result["name"];
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateShopItemAsync(ShopItemModel item, string idToken)
        {
            try
            {
                var json = JsonConvert.SerializeObject(item);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync(
                    $"{_firebaseConfig.DatabaseUrl}/shop/{item.ItemId}.json?auth={idToken}",
                    content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteShopItemAsync(string itemId, string idToken)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(
                    $"{_firebaseConfig.DatabaseUrl}/shop/{itemId}.json?auth={idToken}");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PurchaseItemAsync(string userId, string itemId, string idToken)
        {
            try
            {
                // Get user
                var user = await _userService.GetUserByIdAsync(userId, idToken);
                if (user == null)
                    return false;

                // Get item
                var item = await GetShopItemByIdAsync(itemId, idToken);
                if (item == null)
                    return false;

                // Check if user has enough coins
                if (user.Coins < item.Price)
                    return false;

                // Check if item is limited and available
                if (item.IsLimited && item.AvailableQuantity <= 0)
                    return false;

                // Check if user already has this item
                if (user.PurchasedItemIds != null && user.PurchasedItemIds.Contains(itemId))
                    return false; // Already purchased

                // Purchase the item
                user.Coins -= item.Price;

                if (user.PurchasedItemIds == null)
                    user.PurchasedItemIds = new List<string>();

                user.PurchasedItemIds.Add(itemId);

                // Update user
                var userUpdateSuccess = await _userService.UpdateUserAsync(user, idToken);

                // If item is limited, decrease available quantity
                if (item.IsLimited)
                {
                    item.AvailableQuantity--;
                    await UpdateShopItemAsync(item, idToken);
                }

                return userUpdateSuccess;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ShopItemModel>> GetUserPurchasedItemsAsync(string userId, string idToken)
        {
            try
            {
                // Get user
                var user = await _userService.GetUserByIdAsync(userId, idToken);
                if (user == null || user.PurchasedItemIds == null || user.PurchasedItemIds.Count == 0)
                    return new List<ShopItemModel>();

                // Get all items
                var allItems = await GetAllShopItemsAsync(idToken);

                // Filter items that the user has purchased
                var userItems = new List<ShopItemModel>();
                foreach (var item in allItems)
                {
                    if (user.PurchasedItemIds.Contains(item.ItemId))
                        userItems.Add(item);
                }

                return userItems;
            }
            catch
            {
                return new List<ShopItemModel>();
            }
        }
    }
}
