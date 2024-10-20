namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Willow.LiveData.Core.Domain;

    [ExcludeFromCodeCoverage]
    internal class PagedTelemetry
    {
        public IReadOnlyCollection<Telemetry> Telemetry { get; set; }

        public string ContinuationToken { get; set; }

        public int TotalRowsCount { get; set; }
    }
}
