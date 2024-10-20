namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Models;

using System;
using System.Collections.Generic;

internal record GetTelemetryRequest(
    Guid? ClientId,
    Guid ConnectorId,
    DateTime Start,
    DateTime End,
    int PageSize,
    string ContinuationToken,
    List<string> DtdIds = null,
    List<Guid> TrendIds = null,
    int LastRowNumber = 0);
