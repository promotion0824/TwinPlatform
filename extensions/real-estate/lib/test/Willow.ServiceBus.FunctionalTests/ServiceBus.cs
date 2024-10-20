using System.Threading.Tasks;

using Xunit;

using Willow.Common;
using Willow.ServiceBus;

namespace Willow.ServiceBus.FunctionalTests
{
    public class ServiceBusTests
    {
        [Fact(Skip = "This test requires a real connection string")]
        public async Task ServiceBus_Send_now()
        {
            IMessageQueue q = new ServiceBus("???", "test");

            await q.Send("This is a test message");
        }
    }
}