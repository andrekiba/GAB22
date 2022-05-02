using System;
using AutoFixture;
using Bogus;
using CrazyBikeStore.Infrastructure;
using CrazyBikeStore.Infrastructure.Middleware;
using CrazyBikeStore.Models;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace CrazyBikeStore
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(worker =>
                {
                    worker.UseNewtonsoftJson();
                    // Register our custom middleware with the worker
                    worker.UseMiddleware<LoggingMiddleware>();
                    //worker.UseMiddleware<HelloMiddleware>();
                })
                .ConfigureLogging((context, loggingBuilder) =>
                {
                    var logger = new LoggerConfiguration()
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
                        .MinimumLevel.Warning()
                        .CreateLogger();

                    loggingBuilder.AddSerilog(logger);
                })
                .ConfigureOpenApi()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<Fixture>();
                    services.AddBikeFaker(context.Configuration);
                })
                .Build();

            host.Run();
        }
    }
}