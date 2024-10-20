using System;

namespace Willow.MessageDispatch
{
    /// <summary>
    /// Notify a Message dispatch
    /// </summary>
    public interface IMessageDispatch
    {
        event EventHandler<MessageDispatchEventArgs> RaiseMessageDispatchEvent;
    }
}
