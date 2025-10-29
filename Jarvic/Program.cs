using Jarvice.Core;
using Jarvice.Plugins.Abstraction;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Jarvice.Core.Bus.Interface;
using Jarvice.Core.Bus.Implementations;
using Jarvice.Core.Services.Interface;
using Jarvice.Core.Services.Impelementation;
using Jarvice.Plugins.Abstraction.Interface;

await Host.CreateDefaultBuilder(args).ConfigureServices((ctx, services) =>
{
    services.AddSingleton<IMessageBus,InmemoryMessageBus>();
    services.AddSingleton<INluEngine, RuleBasedNlu>();
    services.AddSingleton<IOrchestrator, OrchestratorService>();

    var pluginDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
    Directory.CreateDirectory(pluginDir);
    var pluginDlls = Directory.GetFiles(pluginDir, "*.dll");
    var pluginTypes = pluginDlls.SelectMany(p => Assembly.LoadFrom(p).GetTypes())
    .Where(p => typeof(IJarvisPlugin).IsAssignableFrom(p)

    && !p.IsInterface && !p.IsAbstract);
});