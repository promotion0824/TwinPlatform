using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Services.Apis;

namespace Willow.Tests.Infrastructure
{
    public static class ServerAssertionExtensions
    {
        public static T GetDbContext<T>(this ServerAssertion assertion) where T : DbContext
        {
            return assertion.MainServices.GetRequiredService<T>();
        }

        public static IImagePathHelper GetImagePathHelper(this ServerAssertion assertion)
        {
            return assertion.MainServices.GetRequiredService<IImagePathHelper>();
        }

    }
}
