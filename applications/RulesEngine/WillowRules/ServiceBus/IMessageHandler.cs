using System;
using System.Threading;
using System.Threading.Tasks;

namespace Willow.ServiceBus;

/// <summary>
/// An object that handles messages of a given type from service bus
/// </summary>
public interface IMessageHandler
{
	/// <summary>
	/// Can this message handler handle the named message type
	/// </summary>
	bool CanHandle(string messageType);

	/// <summary>
	/// Handle the message of type Type, true is successful
	/// </summary>
	Task<bool> Handle(BinaryData message, CancellationToken cancellationToken);
}
