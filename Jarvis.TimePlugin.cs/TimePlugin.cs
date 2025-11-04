using Jarvice.Plugins.Abstraction.Interface;
using System.Globalization;
using static Jarvice.Plugins.Abstraction.Interface.IJarvisPlugin;

namespace Jarvis.Plugins.Time
{
    public class TimePlugin : IJarvisPlugin
    {
        public string Name => "TimePlugin";

        public IReadOnlyCollection<string> Intents => new[] { "time.now" };

        public Task<PluginResult> ExecuteAsync(PluginContext context, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.Now;
            var fa = new CultureInfo("fa-IR");

            // نمایش نمونه: دوشنبه 13 آبان 1404، 14:37
            var text = now.LocalDateTime.ToString("dddd dd MMMM yyyy، HH:mm", fa);
            return Task.FromResult(new PluginResult(true, $"الان: {text}"));
        }
    }
}