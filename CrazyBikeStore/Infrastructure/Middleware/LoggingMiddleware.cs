using System.Threading.Tasks;
using CrazyBikeStore.Infrastructure.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace CrazyBikeStore.Infrastructure.Middleware
{
    public class LoggingMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            //skip swagger functions
            if (context.FunctionDefinition.EntryPoint.Contains("Microsoft.Azure.Functions.Worker.Extensions.OpenApi"))
                await next(context);
            else
            {
                var logger = context.GetLogger<LoggingMiddleware>();
                logger.LogInformation($"Start executing {context.FunctionDefinition.Name}");
                await next(context);
                logger.LogInformation($"End of {context.FunctionDefinition.Name}");
            }
        }
    }
}