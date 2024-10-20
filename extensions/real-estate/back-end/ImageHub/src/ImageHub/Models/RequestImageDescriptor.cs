using System;

namespace ImageHub.Models
{
    public class RequestImageDescriptor
    {
        public Guid ImageId { get; set; }
        public ImageFileExtension FileExtension { get; set; }
        public ScaleType ScaleType { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public string GetNormalizedFileName()
        {
            string extension;
            switch (FileExtension)
            {
                case ImageFileExtension.Jpeg:
                    extension = "jpg";
                    break;
                case ImageFileExtension.Png:
                    extension = "png";
                    break;
                default:
                    throw new InvalidOperationException($"Unknown image file extension {FileExtension}");
            }

            if (ScaleType == ScaleType.Original)
            {
                return $"{ImageId:N}_{(int)ScaleType}.{extension}";
            }
            return $"{ImageId:N}_{(int)ScaleType}_w{Width}_h{Height}.{extension}";
        }
    }
}
