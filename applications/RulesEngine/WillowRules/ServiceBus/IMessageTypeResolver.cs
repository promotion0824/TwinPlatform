using System;

namespace Willow.ServiceBus;

public interface IMessageTypeResolver
{
	public Type GetMessageType(string name);
}
