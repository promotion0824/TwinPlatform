using System;

namespace DirectoryCore.Services
{
    public interface IImagePathHelper
    {
        string GetCustomerLogoPath(Guid customerId);
    }

    public class ImagePathHelper : IImagePathHelper
    {
        public string GetCustomerLogoPath(Guid customerId)
        {
            return $"{customerId}/logo";
        }
    }
}
