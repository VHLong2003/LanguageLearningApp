using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace LanguageLearningApp.Helpers
{
    public static class FirebaseStorageHelper
    {
        public static async Task<string> UploadImageAsync(Stream imageStream, string fileName)
        {
            var storageUrl = $"https://firebasestorage.googleapis.com/v0/b/{FirebaseConfig.StorageBucket}/o/{fileName}?uploadType=media";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Content-Type", "image/jpeg");
            var content = new StreamContent(imageStream);
            var response = await client.PostAsync(storageUrl, content);
            if (!response.IsSuccessStatusCode) return null;
            var getUrl = $"https://firebasestorage.googleapis.com/v0/b/{FirebaseConfig.StorageBucket}/o/{fileName}?alt=media";
            return getUrl;
        }
    }
}
