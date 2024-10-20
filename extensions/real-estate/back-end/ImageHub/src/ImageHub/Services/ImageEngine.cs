using System;
using System.IO;
using System.Collections.Generic;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using ImageHub.Models;
using ImageHub.Services.Processors;

namespace ImageHub.Services
{
    public interface IImageEngine
    {
        Stream Process(Stream originalImageStream, RequestImageDescriptor requestImageDescriptor);
    }

    public class ImageEngine : IImageEngine
    {
        private readonly Dictionary<ScaleType, IImageProcessor> _processors = new Dictionary<ScaleType, IImageProcessor>
        {
            [ScaleType.Original] = new OriginalProcessor(),
            [ScaleType.CenterCrop] = new CenterCropProcessor()
        };

        public ImageEngine()
        {
        }

        public Stream Process(Stream originalImageStream, RequestImageDescriptor requestImageDescriptor)
        {
            if (!_processors.TryGetValue(requestImageDescriptor.ScaleType, out IImageProcessor processor))
            {
                throw new ArgumentException($"Cannot process image for scale type: {requestImageDescriptor.ScaleType}");
            }

            if(originalImageStream.CanSeek)
                originalImageStream.Seek(0, SeekOrigin.Begin);

            Image originalImage = null;

            try
            { 
                originalImage = Image.Load(originalImageStream);
            }
            catch(Exception ex)
            {
                throw new ArgumentException("Stream is not a valid image format", ex);
            }

            var processedImage = processor.Process(originalImage, requestImageDescriptor);

            return Save(processedImage, requestImageDescriptor);
        }

        private Stream Save(Image image, RequestImageDescriptor imageDescriptor)
        {
            var stream = new MemoryStream();

            try
            { 
                switch(imageDescriptor.FileExtension)
                {
                    case ImageFileExtension.Jpeg:
                        image.SaveAsJpeg(stream);
                        break;
                    case ImageFileExtension.Png:
                        image.SaveAsPng(stream);
                        break;
                    default:
                        throw new ArgumentException($"Cannot handle image file extension {imageDescriptor.FileExtension}");
                }
            }
            catch
            {
                stream.Dispose();
                throw;
            }

            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}
