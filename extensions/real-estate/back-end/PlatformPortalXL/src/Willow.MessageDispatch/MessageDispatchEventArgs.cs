using System;

namespace Willow.MessageDispatch
{
    /// <summary>
    /// Custom EventArgs pattern to return Message data
    /// </summary>
    public class MessageDispatchEventArgs : EventArgs
    {
        public MessageDispatchEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
