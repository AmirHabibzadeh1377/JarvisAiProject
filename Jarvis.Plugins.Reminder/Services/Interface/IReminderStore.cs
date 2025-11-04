using Jarvis.Plugins.Reminder.Models;

namespace Jarvis.Plugins.Reminder.Services.Interface
{
    public interface IReminderStore
    {
        Task AddAsync(ReminderItemModel item, CancellationToken ct = default);
        Task<IReadOnlyList<ReminderItemModel>> GetDueAsync(DateTimeOffset now, CancellationToken ct = default);
        Task MarkFiredAsync(string id, CancellationToken ct = default);
    }
}