using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ThumbnailGenerator.Core.Application.DTOs;
using ThumbnailGenerator.Core.Application.Interfaces;
using Google.Cloud.SecretManager.V1;

namespace ThumbnailGenerator.Infrastructure.APIClients
{
    public class ImageApiClient : IImageApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        // This record is a private DTO for the PATCH request


        public ImageApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;

            _httpClient = httpClient;
        }

        public async Task UpdateThumbnailStatusAsync(UpdateThumbnailImageDto updateThumbnailImageDto, string token)
        {
            var imageApiUrl = _configuration["ServiceUrls:ImageAPI"] + "/api/Image/update-image";
            var request = new HttpRequestMessage(HttpMethod.Put, imageApiUrl);
            request.Content = new StringContent(JsonSerializer.Serialize(updateThumbnailImageDto), Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.SendAsync(request);
        }

        public async Task<string> GenerateServiceToken()
        {

            // Ngược lại, gọi Auth API để lấy token mới
            var authApiUrl = _configuration["ServiceUrls:AuthAPI"] + "/api/Auth/service-token";
            string projectId = _configuration.GetSection("Gcp").GetValue<string>("ProjectID") ?? throw new ArgumentNullException("Gcp Project ID not configured.");
            string keyClientID = _configuration.GetSection("Gcp").GetValue<string>("Client-ID") ?? throw new ArgumentNullException("Gcp ClientID not configured.");
            string keyClientSecret = _configuration.GetSection("Gcp").GetValue<string>("Client-Secret") ?? throw new ArgumentNullException("Gcp ClientSecret not configured.");
            string jwtSecret = _configuration.GetSection("Gcp").GetValue<string>("JWTSecretKey") ?? throw new ArgumentNullException("Gcp JWTSecretKey not configured.");
            string secretVersion = _configuration.GetSection("Gcp").GetValue<string>("SecretVersion") ?? throw new ArgumentNullException("Gcp Secret Version not configured.");

            //Init Secret Manager Client
            SecretManagerServiceClient client = SecretManagerServiceClient.Create();

            //get the secret value for JWT creation
            SecretVersionName secretVersionName = new(projectId, keyClientID, secretVersion);
            AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);
            string clientId = result.Payload.Data.ToStringUtf8();

            secretVersionName = new(projectId, keyClientSecret, secretVersion);
            result = client.AccessSecretVersion(secretVersionName);
            string clientSecret = result.Payload.Data.ToStringUtf8();


            var requestBody = new { clientId, clientSecret };
            var jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(authApiUrl, content);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<ResponseDto>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return tokenResponse?.Result?.ToString() ?? string.Empty;

        }
    }
}
