using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LanguageLearningApp.Helpers
{
    public static class ApiHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        static ApiHelper()
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<T> GetAsync<T>(string url, string authToken = null)
        {
            SetAuthHeader(authToken);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content);
        }

        public static async Task<T> PostAsync<T>(string url, object data, string authToken = null)
        {
            SetAuthHeader(authToken);

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent);
        }

        public static async Task<T> PutAsync<T>(string url, object data, string authToken = null)
        {
            SetAuthHeader(authToken);

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent);
        }

        public static async Task DeleteAsync(string url, string authToken = null)
        {
            SetAuthHeader(authToken);

            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }

        private static void SetAuthHeader(string authToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                string.IsNullOrEmpty(authToken) ? null : new AuthenticationHeaderValue("Bearer", authToken);
        }
    }
}
