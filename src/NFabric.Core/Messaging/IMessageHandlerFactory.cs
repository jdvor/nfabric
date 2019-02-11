namespace NFabric.Core.Messaging
{
    public interface IMessageHandlerFactory
    {
        IMessageHandler GetHandler(MessageContext context);
    }
}
