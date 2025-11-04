using Jarvis.Plugins.Reminder.Services.Interface;
using Microsoft.Extensions.Hosting;

namespace Jarvis.Infrastruture.Scheduler
{
    public sealed class ReminderScheduler : BackgroundService
    {
        #region Fields

        private readonly IReminderStore _reminderStore;

        #endregion

        #region Ctor

        public ReminderScheduler(IReminderStore reminderStore)
        {
            _reminderStore = reminderStore;
        }

        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.Now;
                var due = await _reminderStore.GetDueAsync(now, stoppingToken);

                foreach (var r in due)
                {
                    if(r.DueAt.AddSeconds(5) >= DateTime.Now || r.DueAt <= DateTime.Now)
                    {
                        Console.WriteLine($"⏰ الان وقتشه: {(r.Message ?? "یادآور")}  ({r.DueAt:yyyy-MM-dd HH:mm:ss})");
                        await _reminderStore.MarkFiredAsync(r.Id, stoppingToken);
                    }
                }

                var nextTick = now.AddSeconds(1);
                var delay = nextTick - DateTimeOffset.Now;
                if (delay < TimeSpan.Zero) delay = TimeSpan.FromMilliseconds(100);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}