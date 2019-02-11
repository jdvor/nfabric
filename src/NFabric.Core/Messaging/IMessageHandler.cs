namespace NFabric.Core.Messaging
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMessageHandler
    {
        Task<Ack> HandleAsync(
            object message,
            MessageContext context,
            CancellationToken cancellationToken = default(CancellationToken));

        Type MessageType { get; }
    }
}
