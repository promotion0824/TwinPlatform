using Azure;
using System;
using System.Collections.Generic;
using System.Threading;

#nullable disable

namespace Willow.Rules.Model;

public struct RawData
{
	public DateTime SourceTimestamp { get; set; }
	public DateTime EnqueuedTimestamp { get; set; }

	public string PointEntityId { get; set; }
	public double Value { get; set; }

	public string TextValue { get; set; }
	public string ExternalId { get; set; }
	public string ConnectorId { get; set; }
}

public class RawDataPageable : Azure.AsyncPageable<RawData>
{
	public RawDataPageable() : base() { }
	public RawDataPageable(CancellationToken token) : base(token) { }

	public override IAsyncEnumerable<Page<RawData>> AsPages(string continuationToken = null, int? pageSizeHint = null)
	{
		throw new NotImplementedException();
	}
}
