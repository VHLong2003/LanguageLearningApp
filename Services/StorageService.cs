using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LanguageLearningApp.Helpers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace LanguageLearningApp.Services
{
    public class StorageService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseConfig _firebaseConfig;

        public StorageService(FirebaseConfig firebaseConfig)
        {
            _httpClient = new HttpClient();
            _firebaseConfig = firebaseConfig;
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string folder, string idToken)
        {
            try
            {
                // Firebase Storage upload endpoint
                // Note: This is a simplified example - actual Firebase Storage requires specific authentication and endpoints
                // You would typically need to generate a signed URL or use Firebase Admin SDK

                // For this example, we'll use the Storage REST API
                var storageUrl = $"https://firebasestorage.googleapis.com/v0/b/{_firebaseConfig.StorageBucket}/o/{folder}%2F{fileName}";

                // Read the image stream into a byte array
                var memoryStream = new MemoryStream();
                await imageStream.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                // Prepare the multipart form data
                var content = new ByteArrayContent(bytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Upload the image
                var response = await _httpClient.PostAsync(storageUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Get the download URL
                    var responseJson = await response.Content.ReadAsStringAsync();

                    // In real Firebase Storage, you would parse the response to get the download URL
                    // Here's a simplified example:
                    return $"https://firebasestorage.googleapis.com/v0/b/{_firebaseConfig.StorageBucket}/o/{folder}%2F{fileName}?alt=media";
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> UploadImageFromFileAsync(string filePath, string folder, string idToken)
        {
            try
            {
                // Open the file as a stream
                using var fileStream = File.OpenRead(filePath);

                // Get file name from path
                var fileName = Path.GetFileName(filePath);

                // Upload using the existing method
                return await UploadImageAsync(fileStream, fileName, folder, idToken);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> UploadImageFromPickerAsync(FileResult fileResult, string folder, string idToken)
        {
            try
            {
                if (fileResult == null)
                    return null;

                // Open the picked file
                using var stream = await fileResult.OpenReadAsync();

                // Generate unique filename
                var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(fileResult.FileName)}";

                // Upload
                return await UploadImageAsync(stream, fileName, folder, idToken);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl, string idToken)
        {
            try
            {
                // Extract object path from URL
                // Example URL: https://firebasestorage.googleapis.com/v0/b/bucket/o/folder%2Fimage.jpg?alt=media
                var uri = new Uri(imageUrl);
                var pathSegments = uri.AbsolutePath.Split('/');
                var objectPath = Uri.UnescapeDataString(pathSegments[pathSegments.Length - 1]);

                var deleteUrl = $"https://firebasestorage.googleapis.com/v0/b/{_firebaseConfig.StorageBucket}/o/{objectPath}";

                var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
                request.Headers.Add("Authorization", $"Bearer {idToken}");

                var response = await _httpClient.SendAsync(request);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ImageSource> GetImageSourceAsync(string imageUrl)
        {
            try
            {
                // If the URL is null or empty, return null
                if (string.IsNullOrEmpty(imageUrl))
                    return null;

                // Check if it's a remote URL or a local resource
                if (imageUrl.StartsWith("http"))
                {
                    // It's a remote URL
                    return ImageSource.FromUri(new Uri(imageUrl));
                }
                else
                {
                    // It's a local resource
                    return ImageSource.FromFile(imageUrl);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
