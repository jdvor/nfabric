namespace NFabric.Core.Messaging
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class MessageHandlerBase<T> : IMessageHandler
        where T : class
    {
        public Type MessageType { get; } = typeof(T);

        public async Task<Ack> HandleAsync(
            object message,
            MessageContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var msg = message as T;
            if (msg == null)
            {
                throw new ArgumentException(
                    $"Incoming message {message.GetType().Name} could not be handled " +
                    $"as it is not or derived from {MessageType.Name}.", nameof(message));
            }

            return await HandleAsync(msg, context, cancellationToken).ConfigureAwait(false);
        }

        protected abstract Task<Ack> HandleAsync(T message, MessageContext context, CancellationToken cancellationToken);
    }
}
