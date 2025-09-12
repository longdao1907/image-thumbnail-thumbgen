using Microsoft.Extensions.Logging;
using ThumbnailGenerator.Core.Application.Interfaces;
using ThumbnailGenerator.Core.Application.DTOs;
using ThumbnailGenerator.Core.Domain.Models;

namespace ThumbnailGenerator.Core.Application.Services
{
    public class ThumbnailService : IThumbnailService
    {
        private readonly IStorageService _storageService;
        private readonly IImageProcessor _imageProcessor;
        private readonly IImageApiClient _imageApiClient;
        private readonly ILogger<ThumbnailService> _logger;

        public ThumbnailService(
            IStorageService storageService,
            IImageProcessor imageProcessor,
            IImageApiClient imageApiClient, 
            ILogger<ThumbnailService> logger)
        {
            _storageService = storageService;
            _imageProcessor = imageProcessor;
            _imageApiClient = imageApiClient;
            _logger = logger;
        }

        public async Task ProcessImageAsync(StorageObjectData data, string accessToken)
        {
            // **Architectural Note:** This assumes the GCS object name is structured like:
            // {foldername}/{imageId}_{originalFileName}
            // We parse the imageId (a Guid) from the second part of the path.
            var pathSegments = data.Name.Split('/');
            if (pathSegments.Length < 2 || !Guid.TryParse(pathSegments[1].Split('_')[0], out var imageId))
            {
                _logger.LogError("Could not parse imageId from GCS object name: {ObjectName}", data.Name);
                return;
            }

            _logger.LogInformation("Processing imageId: {ImageId}", imageId);

            var originalImageStream = new MemoryStream();
            var thumbnailStream = new MemoryStream();

            try
            {
                // 1. Download the original image
                _logger.LogInformation("Downloading {ObjectName} from {Bucket}", data.Name, data.Bucket);
                await _storageService.DownloadFileAsync(data.Name, originalImageStream);
                originalImageStream.Position = 0; // Reset stream for reading

                // 2. Process the image to create a thumbnail
                _logger.LogInformation("Creating thumbnail for {ImageId}", imageId);
                await _imageProcessor.CreateThumbnailAsync(originalImageStream, thumbnailStream);
                thumbnailStream.Position = 0; // Reset stream for reading

                // 3. Upload the new thumbnail
                var thumbnailObjectName = $"thumbnail-image/{imageId}_{pathSegments[1].Split('_')[1]}_thumb.png";

                _logger.LogInformation("Uploading thumbnail to {ThumbnailName}", thumbnailObjectName);
                var thumbnailUrl = await _storageService.UploadFileAsync(thumbnailObjectName, thumbnailStream, "image/png");

                // 4. Update the status in ImageAPI
                //_logger.LogInformation("Updating status to Completed for {ImageId} with URL: {Url}", imageId, thumbnailUrl);

                UpdateThumbnailImageDto uploadThumbnailImageDto = new UpdateThumbnailImageDto
                {
                    ImageId = imageId,
                    ThumbnailUrl = string.Empty,
                    Status = ThumbnailUpdateStatus.Completed.ToString()
                };
                await _imageApiClient.UpdateThumbnailStatusAsync(uploadThumbnailImageDto, accessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing thumbnail for {ImageId}", imageId);
                // On failure, update the status in ImageAPI
                UpdateThumbnailImageDto uploadThumbnailImageDto = new UpdateThumbnailImageDto
                {
                    ImageId = imageId,
                    Status = ThumbnailUpdateStatus.Completed.ToString(),
                    ThumbnailUrl = null
                };
                await _imageApiClient.UpdateThumbnailStatusAsync(uploadThumbnailImageDto, accessToken);
            }
        }
    }
}

