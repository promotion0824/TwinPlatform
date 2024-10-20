using Autodesk.Forge.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlatformPortalXL.Services.Forge
{
    public class TranslateToSvfOperation : AbstractOperation
    {
        private static readonly List<JobPayloadItem> Outputs = new List<JobPayloadItem>
        {
            new JobPayloadItem(
                JobPayloadItem.TypeEnum.Svf,
                new List<JobPayloadItem.ViewsEnum>{ JobPayloadItem.ViewsEnum._2d, JobPayloadItem.ViewsEnum._3d }
            )
        };

        public TranslateToSvfOperation(ForgeOperationContext context, ForgeOptions options)
            : base(context)
        {
        }

        protected new ForgeOperationContext Context => base.Context;
        private string WorkflowId => $"Workflow{Context.Id.GetHashCode()}";

        protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
        {
            var urn = Context.ForgeInfo.Urn;

            var input = new JobPayloadInput(urn);
            var output = new JobPayloadOutput(Outputs);
            var misc = new JobPayloadMisc(WorkflowId);
            var job = new JobPayload(input, output, misc);

            var result = await this.Context.ForgeApi.TranslateAsync(job, true);

            if (result.result != "success")
            {
                throw new InvalidOperationException($"Internal error: Forge translate api return error. {result.result}");
            }
        }

    }
}
