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
using Jarvis.Plugins.Reminder;
using Microsoft.Extensions.Logging;
using Jarvic.Helper;
using System.Text;
using System.Runtime.InteropServices;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
{

    services.AddSingleton<IMessageBus, InmemoryMessageBus>();
    services.AddSingleton<INluEngine, RuleBasedNlu>();
    services.AddSingleton<IOrchestrator, OrchestratorService>();

    var pluginDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
    Directory.CreateDirectory(pluginDir);
    var pluginDlls = Directory.GetFiles(pluginDir, "*.dll");
    var pluginTypes = pluginDlls.SelectMany(p => Assembly.LoadFrom(p).GetTypes())
    .Where(p => typeof(IJarvisPlugin).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

    foreach (var pluginType in pluginTypes)
    {
        services.AddSingleton(typeof(IJarvisPlugin), pluginType);
    }

    if (!pluginTypes.Any())
    {
        services.AddSingleton<IJarvisPlugin, ReminderPlugin>();
    }
}).ConfigureLogging(l => l.ClearProviders().AddSimpleConsole())
.RunConsoleApp(async (host, ct) =>
{
    Console.OutputEncoding = Encoding.UTF8;
    Console.InputEncoding = Encoding.UTF8;
    var orchestrator = host.Services.GetRequiredService<IOrchestrator>();
    Console.WriteLine("Jarvis CLI آماده است. (exit برای خروج)");
    while (true)
    {
        Console.Write("> ");
        var line = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(line)) continue;
        if (line.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

        var reply = await orchestrator.HandleAsync(new(line, DateTimeOffset.Now), ct);
        Console.WriteLine(Encoding.UTF8.ToString(),$"Jarvis: {reply.Text}");
    }
});