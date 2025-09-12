using ThumbnailGenerator.Core.Application.DTOs;

namespace ThumbnailGenerator.Core.Application.Interfaces
{
    public enum ThumbnailUpdateStatus { Completed, Failed }
    public interface IImageApiClient
    {
        Task UpdateThumbnailStatusAsync(UpdateThumbnailImageDto updateThumbnailImageDto, String accessToken);

    }
}
