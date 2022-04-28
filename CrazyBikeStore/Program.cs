using AutoFixture;
using CrazyBikeStore.Infrastructure.Middleware;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CrazyBikeStore
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(worker =>
                {
                    // Register our custom middleware with the worker
                    //worker.UseMiddleware<HelloMiddleware>();
                    worker.UseNewtonsoftJson();
                })
                .ConfigureOpenApi()
                .ConfigureServices(services => services.AddSingleton<Fixture>())
                .Build();

            host.Run();
        }
    }
}