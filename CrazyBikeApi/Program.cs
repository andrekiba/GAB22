using CrazyBikeApi.Infrastructure.Middleware;
using Microsoft.Extensions.Hosting;

namespace CrazyBikeApi
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(workerApplication =>
                {
                    // Register our custom middleware with the worker
                    workerApplication.UseMiddleware<HelloMiddleware>();
                })
                .Build();

            host.Run();
        }
    }
}