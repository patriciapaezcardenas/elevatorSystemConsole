using ElevatorSystem.Domain;
using ElevatorSystem.Domain.Entities;
using ElevatorSystem.Interfaces;
using ElevatorSystem.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Runtime;

namespace ElevatorSystem
{
    /// <summary>
    /// Provides methods to configure and build the host application for the Elevator System.
    /// Responsible for setting up services, logging, and configuration.
    /// </summary>
    public static class AppBuilder
    {
        /// <summary>
        /// Creates and configures the application host.
        /// Registers services, sets up Serilog for logging, and loads configuration files.
        /// </summary>
        /// <returns>An <see cref="IHost"/> instance representing the configured application host.</returns>
        public static IHost Create()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("Starting Elevator System simulation");

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Register core services for the elevator system
                    services.AddSingleton<ElevatorControllerService>();

                    services.Configure<ElevatorSettings>(context.Configuration);

                    // Register IElevator implementations as transient (a new instance per use)
                    services.AddTransient<Func<int, Elevator>>(sp => (id) => 
                    { 
                        var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ElevatorSettings>>().Value;
                        return new Elevator(id, settings);
                    });

                    // Register IRequestService to generate random requests, with configuration-based interval
                    services.AddSingleton<IRequestService>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        var intervalMs = config.GetValue<int>("randomRequestService:intervalMilliseconds", 5000);
                        return new RandomRequestService(intervalMs);
                    });
                })
                .UseSerilog() // Use Serilog for structured logging
                .Build();

            return host;
        }

        /// <summary>
        /// Configures the application configuration builder.
        /// Loads base configuration and environment-specific settings from appsettings JSON files and environment variables.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to configure.</param>
        public static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }
    }
}