using Microsoft.Extensions.Hosting;

namespace Jarvic.Helper
{
    public static class HostExtensions
    {
        public static async Task RunConsoleApp(this IHostBuilder builder, Func<IHost, CancellationToken, Task> run)
        {
            using var host = builder.Build();
            var ct = new CancellationTokenSource().Token;
            await run(host, ct);
        }
    }
}