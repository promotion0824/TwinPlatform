// -----------------------------------------------------------------------
// <copyright file="TimeSeriesDataWithIds.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.LiveData.Core.Domain;

using System;
using Newtonsoft.Json.Linq;

internal class TimeSeriesDataWithIds
{
    public Guid PointEntityId { get; set; }

    public DateTime Timestamp { get; set; }

    public JObject State { get; set; }

    public int OnCount { get; set; }

    public int OffCount { get; set; }

    public decimal? Average { get; set; }

    public decimal? Minimum { get; set; }

    public decimal? Maximum { get; set; }
}
