using System.Linq;
using CrazyBikeStore.Infrastructure.Accessors;
using CrazyBikeStore.Infrastructure.Extensions;
using CrazyBikeStore.Infrastructure.Middleware;
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
                    //worker.UseMiddleware<HelloMiddleware>();
                    worker.UseMiddleware<FunctionContextAccessorMiddleware>();
                    worker.UseMiddleware<LoggingMiddleware>();
                    
                    worker.UseMiddleware<AuthenticationMiddleware>();
                    worker.UseMiddleware<AuthorizationMiddleware>();
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
                    // The accessor itself should be registered as a singleton, but the context
                    // within the accessor will be scoped to the Function invocation
                    services.AddSingleton<IFunctionContextAccessor, FunctionContextAccessor>();
                    
                    services.AddAuthorization(options =>
                    {
                        const string scopeClaimType = "http://schemas.microsoft.com/identity/claims/scope";
                        
                        options.AddPolicy("BikeReader", policy =>
                            policy.RequireAssertion(authContext =>
                            
                                authContext.User.HasClaim(c => c.Type == scopeClaimType) &&
                                authContext.User.Claims.Single(c => c.Type == scopeClaimType).Value.Contains("Bikes.Read")
                            ));
                        
                        options.AddPolicy("BikeWriter", policy =>
                            policy.RequireAssertion(authContext =>
                            
                                authContext.User.HasClaim(c => c.Type == scopeClaimType) &&
                                authContext.User.Claims.Single(c => c.Type == scopeClaimType).Value.Contains("Bikes.Writer")
                            ));
                    });
                    
                    services.AddBikeFaker(context.Configuration);
                })
                .Build();

            host.Run();
        }
    }
}