namespace Jarvice.Core.Bus
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(T message , CancellationToken cancellationToken = default);
        void Subscribe<T>(Func<T, Task> handler);
    }
}