using System;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Willow.Common
{
    public static class MessageQueueExtensions
    {
        public static Task Send<T>(this IMessageQueue messageQueue, T message, DateTime? sendOn = null)
        {
            return messageQueue.Send(JsonConvert.SerializeObject(message), sendOn);
        }
    }
}
