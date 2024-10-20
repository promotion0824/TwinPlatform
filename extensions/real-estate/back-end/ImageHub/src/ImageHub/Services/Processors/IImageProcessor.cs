using ImageHub.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageHub.Services.Processors
{
    public interface IImageProcessor
    {
        Image Process(Image originalImage, RequestImageDescriptor requestImageDescriptor);
    }
}
