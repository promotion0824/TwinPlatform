namespace Willow.MessageDispatch
{
    public interface IMessageDispatchHandler
    {
        void OnMessageDispatch(object sender, MessageDispatchEventArgs e);
    }
}
