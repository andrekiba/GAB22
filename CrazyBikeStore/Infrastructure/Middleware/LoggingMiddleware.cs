using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace CrazyBikeStore.Infrastructure.Middleware
{
    public class LoggingMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var logger = context.GetLogger<LoggingMiddleware>();
            //do not log calls to swagger render functions
            if (context.FunctionDefinition.Name.StartsWith("Render"))
                await next(context);
            else
            {
                logger.LogInformation($"Start executing {context.FunctionDefinition.Name}");
                await next(context);
                logger.LogInformation($"End of {context.FunctionDefinition.Name}");
            }
        }
    }
}