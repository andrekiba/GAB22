using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace CrazyBikeApi.Infrastructure.Middleware
{
    public class HelloMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            // This is added pre-function execution, function will have access to this information
            // in the context.Items dictionary
            context.Items.Add("hello", "Hello, from middleware");
            
            await next(context);

            // This happens after function execution. We can inspect the context after the function
            // was invoked
            if (context.Items.TryGetValue("hello", out var value) && value is string message)
            {
                ILogger logger = context.GetLogger<HelloMiddleware>();

                logger.LogInformation("From function: {message}", message);
            }
        }
    }
}