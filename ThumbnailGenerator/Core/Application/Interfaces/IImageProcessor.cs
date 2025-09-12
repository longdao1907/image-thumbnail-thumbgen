namespace ThumbnailGenerator.Core.Application.Interfaces
{
    public interface IImageProcessor
    {
        Task CreateThumbnailAsync(Stream input, Stream output);
    }
}
