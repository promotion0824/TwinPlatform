using Moq;
using Xunit;

using WorkflowCore.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using WorkflowCore.Extensions.ServiceCollectionExtensions;
using Willow.Scheduler;

namespace WorkflowCore.Test.UnitTests
{
    public class ServiceCollectionExtensionsTests
    {
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<ITicketTemplateService> _ticketTemplateService;
        private readonly Mock<ILogger<SchedulerService>> _logger;
        private readonly Mock<ISchedulerRepository> _repo;

        public ServiceCollectionExtensionsTests()
        {
            _configuration = new Mock<IConfiguration>();
            _ticketTemplateService = new  Mock<ITicketTemplateService>();
            _logger = new Mock<ILogger<SchedulerService>>();
            _repo = new Mock<ISchedulerRepository>();

            _configuration.Setup( c=> c["ScheduleTicketTemplateAdvance"]).Returns("7");

        }

        [Fact]
        [Trait("Category", "FrequencyUnit")]
        public void  ServiceCollectionExtensions_AddScheduler_success()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddScoped<ISchedulerRepository>( p=> _repo.Object);
            services.AddScoped<ILogger<SchedulerService>>( p=> _logger.Object);
            services.AddScoped<ITicketTemplateService>( p=> _ticketTemplateService.Object);

            services.AddScheduler(_configuration.Object, "bob", out int advance);

            var sp = services.BuildServiceProvider();

            Assert.NotNull(sp.GetRequiredService<ISchedulerService>());
            Assert.Equal(7, advance);
        }
    }
}
