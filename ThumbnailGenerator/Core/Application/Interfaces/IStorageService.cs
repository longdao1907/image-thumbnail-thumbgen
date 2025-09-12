namespace ThumbnailGenerator.Core.Application.Interfaces
{
    public interface IStorageService
    {
        Task DownloadFileAsync (string objectName, Stream destination);
        Task<string> UploadFileAsync(string objectName, Stream source, string contentType);
    }
}

