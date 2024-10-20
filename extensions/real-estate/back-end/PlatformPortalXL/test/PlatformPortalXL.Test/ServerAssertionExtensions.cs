using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlatformPortalXL.Services;

namespace Willow.Tests.Infrastructure
{
    public static class ServerAssertionExtensions
    {
        public static T GetDbContext<T>(this ServerAssertion assertion) where T : DbContext
        {
            return assertion.MainServices.GetRequiredService<T>();
        }

        public static IImageUrlHelper GetImageUrlHelper(this ServerAssertion arrangement)
        {
            return arrangement.MainServices.GetRequiredService<IImageUrlHelper>();
        }
    }
}
