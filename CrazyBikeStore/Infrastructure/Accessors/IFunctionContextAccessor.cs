using Microsoft.Azure.Functions.Worker;

namespace CrazyBikeStore.Infrastructure.Accessors
{
    public interface IFunctionContextAccessor
    {
        FunctionContext FunctionContext { get; set; }
    }
}