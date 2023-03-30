using Data_Access_Layer.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;

namespace car_db;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().Enrich.FromLogContext()
           .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, shared: true)
           .WriteTo.Console()
           .CreateBootstrapLogger();
        try
        {
            //TODO: Remove this after ensuring all DateTimes and DateTimeOffsets are moved to UTC
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            IHost host = Host.CreateDefaultBuilder(args)
            .UseSerilog((context, config) =>
            {
                config.ReadFrom.Configuration(context.Configuration);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseIISIntegration();
                webBuilder.UseStartup<Startup>();
            }).Build();
            Log.Information($"Starting host: {nameof(Program)}.{nameof(Main)}");

            ///Отдельный проект для миграций
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var contextBase = services.GetRequiredService<ApplicationDbContext>();
                contextBase.Database.Migrate();
                var context = services.GetRequiredService<UserDbContext>();
                context.Database.Migrate();
            }


            await host.RunAsync();
        }
        catch (OperationCanceledException)
        {
            //This is normal. Ctrl+C pressed or host is quitting.
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, $"Fatal error: {nameof(Program)}.{nameof(Main)}");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
