using System.Threading.Tasks;

namespace NFabric.Core.Messaging
{
    public interface IPublisher
    {
        Task PublishAsync<T>(T message, PublishContext context = null)
            where T : class;
    }
}
