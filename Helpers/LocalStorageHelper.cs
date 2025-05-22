using System.Threading.Tasks;

namespace LanguageLearningApp.Helpers
{
    public static class LocalStorageHelper
    {
        // For non-sensitive data
        public static void SetItem(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            Preferences.Set(key, value);
        }

        public static string GetItem(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            return Preferences.Get(key, null);
        }

        public static void RemoveItem(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            Preferences.Remove(key);
        }

        public static void Clear()
        {
            Preferences.Clear();
        }

        // For sensitive data like auth tokens
        public static async Task SetSecureItem(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            await SecureStorage.SetAsync(key, value);
        }

        public static async Task<string> GetSecureItem(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            return await SecureStorage.GetAsync(key);
        }

        public static void RemoveSecureItem(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            SecureStorage.Remove(key);
        }

        public static void ClearSecure()
        {
            SecureStorage.RemoveAll();
        }
    }
}
