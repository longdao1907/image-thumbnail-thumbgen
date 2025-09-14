using CloudNative.CloudEvents;
using CloudNative.CloudEvents.NewtonsoftJson;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;
using ThumbnailGenerator.Core.Application.DTOs;
using ThumbnailGenerator.Core.Application.Interfaces;
using ThumbnailGenerator.Core.Application.Services;
using ThumbnailGenerator.Core.Domain.Models;

namespace ThumbnailGenerator.Controllers
{
    [ApiController]
    [Route("api/Event")]
    //[Authorize] // Protect all endpoints in this controller
    public class EventController : ControllerBase
    {
        private readonly IThumbnailService _thumbnailService;
        private readonly ILogger<EventController> _logger;


        public EventController(IThumbnailService thumbnailService, ILogger<EventController> logger)
        {
            _thumbnailService = thumbnailService;
            _logger = logger;
        }

        private static readonly CloudEventFormatter formatter = new JsonEventFormatter();

        [HttpPost("generate-thumbnail")]
        public async Task<IActionResult> Post(CloudEvent cloudEvent)
        {

            // Eventarc sends events with a specific type for GCS uploads
            if (cloudEvent.Type != "google.cloud.storage.object.v1.finalized")
            {
                _logger.LogWarning("Received event with unexpected type: {EventType}", cloudEvent.Type);
                return Ok(); // Acknowledge the event but do nothing
            }

            // The 'Data' property of the CloudEvent is the payload
            if (cloudEvent.Data is null)
            {
                _logger.LogError("Received CloudEvent with null data.");
                return BadRequest("CloudEvent data is required."); // Acknowledge the event but do nothing

            }

            // Deserialize the data into our strongly-typed model
            var storageObjectData = JsonSerializer.Deserialize<StorageObjectData>(cloudEvent.Data.ToString()!);
            if (storageObjectData is null)
            {
                _logger.LogError("Failed to deserialize StorageObjectData.");
                return BadRequest("Invalid StorageObjectData format."); // Acknowledge the event but do nothing

            }

            await _thumbnailService.ProcessImageAsync(storageObjectData);

            // Always return a 2xx status to Eventarc to prevent retries
            return Ok();
        }
    }
}
