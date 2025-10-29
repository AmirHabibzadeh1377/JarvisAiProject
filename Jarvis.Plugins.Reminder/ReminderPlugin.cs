using Jarvice.Plugins.Abstraction.Interface;
using static Jarvice.Plugins.Abstraction.Interface.IJarvisPlugin;

namespace Jarvis.Plugins.Reminder
{
    internal class ReminderPlugin : IJarvisPlugin
    {
        public string Name => "ReminderPlugin";

        public IReadOnlyCollection<string> Intents => new[] { "reminder.Create" };

        public Task<IJarvisPlugin.PluginResult> ExecuteAsync(IJarvisPlugin.PluginContext context, CancellationToken cancellationToken = default)
        {
            var time = context.Slots.TryGetValue("time", out var t) ? t : "قرباٌ";
            return Task.FromResult(new PluginResult(true, $"اوکی! یادآور برای {time} ثبت شد (فاز۱-ماک)."));
        }
    }
}