namespace Jarvice.Plugins.Abstraction.Interface
{
    public interface IJarvisPlugin
    {
        string Name { get; }
        IReadOnlyCollection<string> Intents { get; } // ["reminder.create","reminder.list"]
        Task<PluginResult> ExecuteAsync(PluginContext context, CancellationToken cancellationToken = default);

        #region Records
        public sealed record PluginContext(string Intent,IDictionary<string, string> Slots, object? Payload, IServiceProvider ServiceProvider);

        public sealed record PluginResult(bool Success , string Message , object? Data = null);

        #endregion
    }
}