using System.Threading;
using System.Threading.Tasks;

namespace PlatformPortalXL.Services.Forge
{
    public abstract class AbstractOperation
    {
        public ForgeOperationContext Context { get; }

        protected AbstractOperation(ForgeOperationContext context)
        {
            Context = context;
        }

        protected abstract Task DoExecuteAsync(CancellationToken cancellationToken);

        public async Task ExecuteAsync()
        {
            await DoExecuteAsync(default);
        }
    }
}
