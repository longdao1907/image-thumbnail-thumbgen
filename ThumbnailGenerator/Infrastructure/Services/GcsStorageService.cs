using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using ThumbnailGenerator.Core.Application.Interfaces;

namespace ThumbnailGenerator.Infrastructure.Services
{
    public class GcsStorageService: IStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public GcsStorageService(IConfiguration configuration)
        {
            _storageClient = StorageClient.Create();
            _bucketName = configuration["Gcp:BucketName"] ?? throw new ArgumentNullException("Gcp:SourceBucketName");
        }

        public async Task DownloadFileAsync(string objectName, Stream destination)
        {
            try
            {
                await _storageClient.DownloadObjectAsync(_bucketName, objectName, destination);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }

        public async Task<string> UploadFileAsync(string objectName, Stream source, string contentType)
        {
            var uploadedObject = await _storageClient.UploadObjectAsync(
                _bucketName,
                objectName,
                contentType,
                source
            );
            return uploadedObject.MediaLink;
        }
    }
    
}

