using Jarvice.Core.Models;

namespace Jarvice.Core.Services.Interface
{
    public interface IOrchestrator
    {
        Task<BotMessage> HandleAsync(UserMessage input , CancellationToken cancellationToken = default);
    }
}