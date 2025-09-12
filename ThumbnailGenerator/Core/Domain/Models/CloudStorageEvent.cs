using System.Text.Json.Serialization;

namespace ThumbnailGenerator.Core.Domain.Models
{
    /// <summary>
    /// Represents the 'data' payload of a CloudEvent for a Google Cloud Storage object finalization.
    /// See: https://cloud.google.com/storage/docs/json_api/v1/objects#resource
    /// </summary>
    /// 


    public class StorageObjectData
    {
        [JsonPropertyName("bucket")]
        public string Bucket { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = string.Empty;

        [JsonPropertyName("timeCreated")]
        public DateTime TimeCreated { get; set; }
    }
}
