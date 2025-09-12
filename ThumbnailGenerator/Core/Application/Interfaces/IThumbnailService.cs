using ThumbnailGenerator.Core.Domain.Models;

namespace ThumbnailGenerator.Core.Application.Interfaces
{
    public interface IThumbnailService
    {
        Task ProcessImageAsync(StorageObjectData data, string accessToken);
    }
}
