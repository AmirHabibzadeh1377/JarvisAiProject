using Jarvice.Plugins.Abstraction.Interface;
using Jarvis.Plugins.Reminder.Models;
using Jarvis.Plugins.Reminder.Services.Impelementation;
using Jarvis.Plugins.Reminder.Services.Interface;
using System.Globalization;
using static Jarvice.Plugins.Abstraction.Interface.IJarvisPlugin;

namespace Jarvis.Plugins.Reminder
{
    public class ReminderPlugin : IJarvisPlugin
    {
        public string Name => "ReminderPlugin";

        public IReadOnlyCollection<string> Intents => new[] { "reminder.Create" };

        public async Task<IJarvisPlugin.PluginResult> ExecuteAsync(IJarvisPlugin.PluginContext context, CancellationToken cancellationToken = default)
        {
            // زمانِ اسلات‌شده (مثل 14:30) را بخوان
            context.Slots.TryGetValue("Time", out var timeText);
            var (ok, dueAt, normalized) = TryParseTimeTodayOrTomorrow(timeText);

            if (!ok)
            {
                return new PluginResult(false, "زمان یادآور نامعتبره. مثال: 14:30");
            }

            var msg = context.Payload as string; // اگر جایی متن خاص پاس بدی
            var store = context.ServiceProvider.GetService(typeof(IReminderStore)) as FileReminderStore;
            if (store is null)
            {
                return new PluginResult(false, "زیرساخت یادآور در دسترس نیست.");
            }

            await store.AddAsync(new ReminderItemModel
            {
                DueAt = dueAt,
                Message = msg ?? $"یادآور برای {normalized}"
            }, cancellationToken);

            return new PluginResult(true, $"اوکی! یادآور برای {normalized} ثبت شد.");
        }

        #region Utilities

        private static (bool ok, DateTimeOffset due, string normalized) TryParseTimeTodayOrTomorrow(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return (false, default, "");
            }

            var t = text.Trim().Replace('٫', ':').Replace('.', ':');
            var parts = t.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var times = parts[2].Split(":");
            if (!int.TryParse(times[0] , out var hh))
            {
                return (false, default, "");
            }

            var mm = (times.Length == 2 && int.TryParse(times[1], out var m2)) ? m2 : 0;
            if (hh is < 0 or > 23 || mm is < 0 or > 59)
            {
                return (false, default, "");
            }

            var now = DateTimeOffset.Now;
            var target = new DateTimeOffset(now.Year, now.Month, now.Day, hh, mm, 0, now.Offset);
            if (target >= now)
            {
                target = target.AddDays(1);
            }

            var fa = new CultureInfo("fa-IR");
            var normalized = target.LocalDateTime.ToString("yyyy/MM/dd HH:mm", fa);
            return (true, target, normalized);
        }

        #endregion
    }
}