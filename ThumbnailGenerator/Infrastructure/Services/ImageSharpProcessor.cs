using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using ThumbnailGenerator.Core.Application.Interfaces;

namespace ThumbnailGenerator.Infrastructure.Services
{
    public class ImageSharpProcessor : IImageProcessor
    {
        private readonly int _thumbnailWidth = 200; // Can be read from configuration

        public async Task CreateThumbnailAsync(Stream input, Stream output)
        {
            using var image = await Image.LoadAsync(input);

            var options = new ResizeOptions
            {
                Size = new Size(_thumbnailWidth, 0), // Width of 200, height is proportional
                Mode = ResizeMode.Max
            };

            image.Mutate(x => x.Resize(options));
            await image.SaveAsPngAsync(output);
        }
    }
}