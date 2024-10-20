using ImageHub.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageHub.Services.Processors
{
    public class CenterCropProcessor : IImageProcessor
    {
        public Image Process(Image originalImage, RequestImageDescriptor requestImageDescriptor)
        {
            int srcWidth = originalImage.Width;
            int srcHeight = originalImage.Height;
            int destWidth = requestImageDescriptor.Width;
            int destHeight = requestImageDescriptor.Height;

            double ratio = 1.0;
            if ((srcWidth * destHeight) < (srcHeight * destWidth))
            {
                ratio = (double)srcWidth / (double)destWidth;
            }
            else
            {
                ratio = (double)srcHeight / (double)destHeight;
            }

            originalImage.Mutate(operation =>
            {
                operation
                    .Crop(new Rectangle(
                        (int)((srcWidth - (destWidth * ratio)) / 2),
                        (int)((srcHeight - (destHeight * ratio)) / 2),
                        (int)(destWidth * ratio),
                        (int)(destHeight * ratio))
                    )
                    .Resize(destWidth, destHeight);
            });

            return originalImage;
        }
    }
}
