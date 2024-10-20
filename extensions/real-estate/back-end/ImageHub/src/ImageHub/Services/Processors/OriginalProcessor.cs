using ImageHub.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageHub.Services.Processors
{
    public class OriginalProcessor : IImageProcessor
    {
        public Image Process(Image originalImage, RequestImageDescriptor requestImageDescriptor)
        {
            return originalImage;
        }
    }
}
