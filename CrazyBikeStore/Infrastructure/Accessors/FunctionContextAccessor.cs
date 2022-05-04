using System.Threading;
using Microsoft.Azure.Functions.Worker;

namespace CrazyBikeStore.Infrastructure.Accessors
{
    public class FunctionContextAccessor : IFunctionContextAccessor
    {
        static readonly AsyncLocal<FunctionContextRedirect> currentContext = new();

        public virtual FunctionContext FunctionContext
        {
            get => currentContext.Value?.HeldContext;
            set
            {
                var holder = currentContext.Value;
                if (holder != null)
                {
                    // Clear current context trapped in the AsyncLocals, as its done.
                    holder.HeldContext = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the context in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    currentContext.Value = new FunctionContextRedirect { HeldContext = value };
                }
            }
        }

        class FunctionContextRedirect
        {
            public FunctionContext HeldContext;
        }
    }
}