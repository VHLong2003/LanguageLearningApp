using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LanguageLearningApp.Helpers
{
    public static class ApiHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<string> GetAsync(string url)
        {
            return await _httpClient.GetStringAsync(url);
        }

        public static async Task<string> PostAsync(string url, string jsonContent)
        {
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> PutAsync(string url, string jsonContent)
        {
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, content);
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> DeleteAsync(string url)
        {
            var response = await _httpClient.DeleteAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
