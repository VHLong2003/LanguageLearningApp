using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using LanguageLearningApp.Helpers;

namespace LanguageLearningApp.Services
{
    public class StorageService
    {
        private readonly HttpClient _httpClient;
        private readonly FirebaseConfig _firebaseConfig;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB limit
        private readonly string[] AllowedFileTypes = { ".jpg", ".jpeg", ".png", ".gif" };

        public StorageService(FirebaseConfig firebaseConfig)
        {
            _httpClient = new HttpClient();
            _firebaseConfig = firebaseConfig;
            _httpClient.DefaultRequestHeaders.Authorization = null; // Avoid stale headers
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string folder, string idToken)
        {
            try
            {
                if (imageStream == null || string.IsNullOrEmpty(fileName))
                    throw new ArgumentException("Image stream or filename cannot be null or empty.");

                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                if (!AllowedFileTypes.Contains(extension))
                    throw new InvalidOperationException("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");

                if (imageStream.Length > MaxFileSize)
                    throw new InvalidOperationException("File size exceeds 5MB limit.");

                var objectPath = $"{folder}/{fileName}";
                var uploadUrl = $"https://firebasestorage.googleapis.com/v0/b/{_firebaseConfig.StorageBucket}/o?uploadType=media&name={Uri.EscapeDataString(objectPath)}";

                using var memoryStream = new MemoryStream();
                await imageStream.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                var content = new ByteArrayContent(bytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return $"https://firebasestorage.googleapis.com/v0/b/{_firebaseConfig.StorageBucket}/o/{Uri.EscapeDataString(objectPath)}?alt=media";
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Upload failed: Status {response.StatusCode}, Details: {errorContent}");
                throw new HttpRequestException($"Upload failed: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image: {ex.Message} - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<string> UploadImageFromFileAsync(string filePath, string folder, string idToken)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("File not found.", filePath);

                using var fileStream = File.OpenRead(filePath);
                var fileName = Path.GetFileName(filePath);
                return await UploadImageAsync(fileStream, fileName, folder, idToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading from file: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadImageFromPickerAsync(FileResult fileResult, string folder, string idToken)
        {
            try
            {
                if (fileResult == null)
                    throw new ArgumentNullException(nameof(fileResult), "No file selected.");

                var extension = Path.GetExtension(fileResult.FileName).ToLowerInvariant();
                if (!AllowedFileTypes.Contains(extension))
                    throw new InvalidOperationException("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");

                Console.WriteLine($"Selected file: {fileResult.FileName}, Size: {fileResult.OpenReadAsync().Result.Length} bytes");

                using var stream = await fileResult.OpenReadAsync();
                var fileName = $"{Guid.NewGuid():N}{extension}";
                return await UploadImageAsync(stream, fileName, folder, idToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading from picker: {ex.Message} - StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl, string idToken)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    throw new ArgumentNullException(nameof(imageUrl), "Image URL cannot be null or empty.");

                var uri = new Uri(imageUrl);
                var pathSegments = uri.AbsolutePath.Split('/');
                var objectPath = Uri.UnescapeDataString(pathSegments[pathSegments.Length - 1].Split('?')[0]);

                var deleteUrl = $"https://firebasestorage.googleapis.com/v0/b/{_firebaseConfig.StorageBucket}/o/{objectPath}";
                var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to delete image: {response.StatusCode} - {errorContent}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex.Message}");
                return false;
            }
        }

        public async Task<ImageSource> GetImageSourceAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return null;

                return imageUrl.StartsWith("http")
                    ? ImageSource.FromUri(new Uri(imageUrl))
                    : ImageSource.FromFile(imageUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting image source: {ex.Message}");
                return null;
            }
        }
    }
}