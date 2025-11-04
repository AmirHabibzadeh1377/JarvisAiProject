
using Jarvice.Core.Bus.Implementations;
using Jarvice.Core.Bus.Interface;
using Jarvice.Core.Services.Impelementation;
using Jarvice.Core.Services.Interface;
using Jarvice.Plugins.Abstraction.Interface;
using Jarvis.Infrastruture.Scheduler;
using Jarvis.Plugins.Reminder;
using Jarvis.Plugins.Reminder.Services.Impelementation;
using Jarvis.Plugins.Reminder.Services.Interface;
using Jarvis.Plugins.Time;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Reflection;

namespace Jarvis.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ——— Swagger ———
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            // ——— CORS (برای تست‌ها) ———
            builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
                p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

            // ——— UTF-8 و فرهنگ فارسی ———
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var cultures = new[] { new CultureInfo("fa-IR"), new CultureInfo("en-US") };
                options.DefaultRequestCulture = new RequestCulture("fa-IR");
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
            });

            // ——— DI: Jarvis Core + Plugins ———
            builder.Services.AddSingleton<IMessageBus, InmemoryMessageBus>();
            builder.Services.AddSingleton<INluEngine, RuleBasedNlu>();
            builder.Services.AddSingleton<IOrchestrator, OrchestratorService>();

            // لود داینامیک پلاگین‌ها از پوشه Plugins کنار خروجی وب
            var pluginDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
            Directory.CreateDirectory(pluginDir);
            var pluginDlls = Directory.GetFiles(pluginDir, "*.dll");
            var pluginTypes = pluginDlls
                .SelectMany(d => Assembly.LoadFrom(d).GetTypes())
                .Where(t => typeof(IJarvisPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (var t in pluginTypes)
            {
                builder.Services.AddSingleton(typeof(IJarvisPlugin), t);
            }

            // اگر پلاگینی نبود، Reminder داخلی را رجیستر کن
            if (!pluginTypes.Any())
            {
                builder.Services.AddSingleton<IJarvisPlugin, ReminderPlugin>();
                builder.Services.AddSingleton<IJarvisPlugin, TimePlugin>();
            }
            builder.Services.AddSingleton<IReminderStore>(sp =>
            {
                var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
                Directory.CreateDirectory(dataDir);
                var path = Path.Combine(dataDir, "reminders.json");
                return new FileReminderStore(path);
            });
            builder.Services.AddHostedService<ReminderScheduler>(); // Scheduler فعال
            builder.Services.AddSingleton<IOrchestrator>(sp =>
            {
                var nlu = sp.GetRequiredService<INluEngine>();
                var plugins = sp.GetServices<IJarvisPlugin>();
                return new OrchestratorService(nlu, plugins,sp); // سازندهٔ جدید
            });

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
            app.Use(async (ctx, next) =>
            {
                // تضمین UTF-8 برای پاسخ‌ها
                ctx.Response.Headers.ContentType = "application/json; charset=utf-8";
                await next();
            });
            app.UseCors();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            // سلامت
            app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTimeOffset.Now }));

            // پرسش و پاسخ
            app.MapPost("/v1/ask", async Task<Results<Ok<AskResponse>, BadRequest<string>>> (AskRequest req, IOrchestrator orch) =>
            {
                if (string.IsNullOrWhiteSpace(req.Text))
                    return TypedResults.BadRequest("متن خالیه.");

                var reply = await orch.HandleAsync(new(req.Text, DateTimeOffset.Now));
                return TypedResults.Ok(new AskResponse(reply.Text, reply.Intent));
            })
            .WithName("Ask")
            .WithOpenApi(op =>
            {
                op.Summary = "ارسال پرسش به دستیار";
                op.Description = "یک متن ورودی بده؛ پاسخ دستیار را دریافت کن.";
                return op;
            });

            // مشاهدهٔ پلاگین‌های فعال
            app.MapGet("/v1/plugins", (IEnumerable<IJarvisPlugin> plugins) =>
                plugins.Select(p => new PluginInfo(p.Name, p.Intents)).ToList())
            .WithName("ListPlugins")
            .WithOpenApi(op =>
            {
                op.Summary = "فهرست پلاگین‌ها و intentها";
                return op;
            });


            app.Run();

        }
        public record AskRequest(string Text);
        public record AskResponse(string Text, string? Intent);
        public record PluginInfo(string Name, IReadOnlyCollection<string> Intents);
    }
}
