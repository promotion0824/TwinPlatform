using System;
using System.Threading.Tasks;
using Willow.Common;

namespace PlatformPortalXL.Test.MockServices
{
    public class MockMessageQueue : IMessageQueue
    {
        public Task Send(string message, DateTime? sendOn = null)
        {
            return Task.CompletedTask;
        }
    }
}
