﻿using ThumbnailGenerator.Core.Application.Interfaces;

namespace ThumbnailGenerator.Core.Application.DTOs
{
    public class UpdateThumbnailImageDto
    {
        public Guid ImageId { get; set; }
        public string Status { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
