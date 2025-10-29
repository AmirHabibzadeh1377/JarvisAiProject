using Jarvice.Core.Bus.Interface;
using System.Collections.Concurrent;

namespace Jarvice.Core.Bus.Implementations
{
    public class InmemoryMessageBus : IMessageBus
    {
        #region Fields

        private readonly ConcurrentDictionary<Type, List<Func<object, Task>>> _handlers = new();

        #endregion

        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        {
            if (_handlers.TryGetValue(typeof(T), out var list))
            {
                foreach (var handler in list)
                {
                    await handler(message);
                }
            }
        }

        public void Subscribe<T>(Func<T, Task> handler)
        {
            var list = _handlers.GetOrAdd(typeof(T), _ => new List<Func<object, Task>>());
            list.Add(o => handler((T)o));
        }
    }
}