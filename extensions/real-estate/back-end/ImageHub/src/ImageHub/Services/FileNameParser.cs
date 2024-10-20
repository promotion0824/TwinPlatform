using System;
using System.IO;
using ImageHub.Models;

namespace ImageHub.Services
{
    public interface IFileNameParser
    {
        RequestImageDescriptor Parse(string fileName);
    }

    public class FileNameParser : IFileNameParser
    {
        public static RequestImageDescriptor ParseFileName(string fileName)
        {
            fileName = fileName.ToLowerInvariant();

            ImageFileExtension fileExtension;
            switch(Path.GetExtension(fileName))
            {
                case ".jpg":
                case ".jpeg":
                    fileExtension = ImageFileExtension.Jpeg;
                    break;
                case ".png":
                    fileExtension = ImageFileExtension.Png;
                    break;
                default:
                    throw new ArgumentException($"Cannot handle file extension {Path.GetExtension(fileName)}");
            }

            var parts = Path.GetFileNameWithoutExtension(fileName).Split('_');

            Guid imageId;
            if (!Guid.TryParse(parts[0], out imageId))
            {
                throw new ArgumentException($"Image id '{parts[0]}' is not a valid GUID.");
            }

            ScaleType scaleType;
            if (!Enum.TryParse<ScaleType>(parts[1], true, out scaleType))
            {
                throw new ArgumentException($"Failed to parse scale type '{parts[1]}'.");
            }
            if (!Enum.IsDefined(typeof(ScaleType), scaleType))
            {
                throw new ArgumentException($"Failed to parse scale type '{parts[1]}'.");
            }

            if (scaleType == ScaleType.Original)
            {
                return new RequestImageDescriptor
                {
                    ImageId = imageId,
                    FileExtension = fileExtension,
                    ScaleType = scaleType,
                    Width = 0,
                    Height = 0,
                };
            }

            int width;
            if (!parts[2].StartsWith("w") || !int.TryParse(parts[2].Substring(1), out width))
            {
                throw new ArgumentException($"Width should start with 'w' followed by an integer. Current input: '{parts[2]}'.");
            }

            int height;
            if (!parts[3].StartsWith("h") || !int.TryParse(parts[3].Substring(1), out height))
            {
                throw new ArgumentException($"Height should start with 'h' followed by an integer. Current input: '{parts[3]}'.");
            }

            return new RequestImageDescriptor
            {
                ImageId = imageId,
                FileExtension = fileExtension,
                ScaleType = scaleType,
                Width = width,
                Height = height
            };
        }

        public RequestImageDescriptor Parse(string fileName)
        {
            return ParseFileName(fileName);
        }
    }
}
