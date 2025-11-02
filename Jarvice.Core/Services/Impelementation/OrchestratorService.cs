using Jarvice.Core.Models;
using Jarvice.Core.Services.Interface;
using Jarvice.Plugins.Abstraction.Interface;
using static Jarvice.Plugins.Abstraction.Interface.IJarvisPlugin;

namespace Jarvice.Core.Services.Impelementation
{
    public class OrchestratorService : IOrchestrator
    {
        #region Fields

        private readonly INluEngine _nlueEngine;
        private readonly IEnumerable<IJarvisPlugin> _plugins;

        #endregion

        #region Ctor

        public OrchestratorService(INluEngine nlueEngine, IEnumerable<IJarvisPlugin> plugins)
        {
            _nlueEngine = nlueEngine;
            _plugins = plugins;
        }

        #endregion

        public async Task<BotMessage> HandleAsync(UserMessage input, CancellationToken cancellationToken = default)
        {
            
            var intent = _nlueEngine.Predict(input.Text);
            var plugin = _plugins.FirstOrDefault(p => p.Intents.Contains(intent.Intent));
            if(plugin is null)
            {
                return new BotMessage($"متوجه نشدم چی می‌خوای. (intent={intent.Intent})", intent.Intent);
            }

            var result = await plugin.ExecuteAsync(new PluginContext(intent.Intent, intent.Slots, null,null),cancellationToken);
            return new BotMessage(result.Message, intent.Intent);
        }
    }
}