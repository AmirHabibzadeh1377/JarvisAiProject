namespace Jarvis.Plugins.Reminder.Models
{
    public sealed class ReminderItemModel
    {
        public string Id { get; init; } = Guid.NewGuid().ToString("N");
        public string Message { get; init; } = string.Empty;
        public DateTimeOffset DueAt { get; init; }
        public bool Fired { get; set; }
    }
}