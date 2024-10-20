using Microsoft.Extensions.DependencyInjection;
using MobileXL.Services;

namespace Willow.Tests.Infrastructure
{
    public static class ServerAssertionExtensions
    {
        public static IImageUrlHelper GetImageUrlHelper(this ServerAssertion arrangement)
        {
            return arrangement.MainServices.GetRequiredService<IImageUrlHelper>();
        }
    }
}
