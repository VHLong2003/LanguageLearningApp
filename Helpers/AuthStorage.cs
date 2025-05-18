using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace LanguageLearningApp.Helpers
{
    public static class AuthStorage
    {
        public static void SaveLogin(string userId, string idToken, string role)
        {
            Preferences.Set("UserId", userId);
            Preferences.Set("IdToken", idToken);
            Preferences.Set("Role", role);
        }

        public static void ClearLogin()
        {
            Preferences.Remove("UserId");
            Preferences.Remove("IdToken");
            Preferences.Remove("Role");
        }

        public static string GetUserId() => Preferences.Get("UserId", "");
        public static string GetIdToken() => Preferences.Get("IdToken", "");
        public static string GetRole() => Preferences.Get("Role", "");
    }

}
