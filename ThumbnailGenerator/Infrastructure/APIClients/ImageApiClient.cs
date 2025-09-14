using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ThumbnailGenerator.Core.Application.DTOs;
using ThumbnailGenerator.Core.Application.Interfaces;

namespace ThumbnailGenerator.Infrastructure.APIClients
{
    public class ImageApiClient : IImageApiClient
    {
        private readonly HttpClient _httpClient;

        // This record is a private DTO for the PATCH request


        public ImageApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task UpdateThumbnailStatusAsync(UpdateThumbnailImageDto updateThumbnailImageDto)
        {

            var request = new HttpRequestMessage(HttpMethod.Put, "/api/Image/update-image");

            request.Content = new StringContent(JsonSerializer.Serialize(updateThumbnailImageDto), Encoding.UTF8, "application/json"); ;

            var response = await _httpClient.SendAsync(request);
        }
    }
}
