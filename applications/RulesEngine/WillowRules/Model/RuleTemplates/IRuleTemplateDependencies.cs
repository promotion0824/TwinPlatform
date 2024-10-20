using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Expressions;
using Willow.Rules.Services;
using WillowRules.Services;

namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// Abstracts away the implementation of accessing a list of TimedValues.
/// </summary>
public interface IRuleTemplateDependencies
{
	/// <summary>
	/// How many are in the buffer
	/// </summary>
	int Count {get;}

	/// <summary>
	/// Gets all time series lines for a given <see cref="RuleInstance"/>
	/// </summary>
	IEnumerable<KeyValuePair<string, TimeSeries>> GetAllTimeSeries(RuleInstance ruleInstance);

	/// <summary>
	/// Indicates whether a particular twin id has any data
	/// </summary>
	bool HasTimeSeriesData(string twinId);

	/// <summary>
	/// Tries to get time series data for a given twin Id. Returns false if nothing was found
	/// </summary>
	bool TryGetTimeSeriesByTwinId(string twinId, out TimeSeries? values);

	/// <summary>
	/// Get an existing time series buffer or add a new one if we have never seen this point before
	/// </summary>
	Task<TimeSeries?> GetOrAdd(string pointId, string connectorId, string externalId);

	/// <summary>
	/// Sends data to adx
	/// </summary>
	Task<bool> SendToADX(EventHubServiceDto payload, CancellationToken? waitOrSkipToken = null);

	/// <summary>
	/// Does the provided id exist as a point id in the rule instance
	/// </summary>
	bool CapabilityTwinExistsInRule(string id);

	/// <summary>
	/// Gets an ML Model
	/// </summary>
	/// <param name="fullName"></param>
	/// <returns></returns>
	IMLRuntime? GetMLModel(string fullName);

	/// <summary>
	/// Indicator whether to apply compression to buffers
	/// </summary>
	bool ApplyCompression { get; }

	/// <summary>
	/// Indicator whether to optimize compression to buffers
	/// </summary>
	bool OptimizeCompression { get; }
}

/// <summary>
/// Abstracts away the implementation of accessing a list of TimedValues.
/// </summary>
/// <remarks>
///	Instead of providing the templates a specific parameter type of time series data
///	(e.g. Dictionary<string, IEnumerable<TimedValue>> timeSeriesData),
///	we provide an interface with the methods the templates would need to access the data.
///	This ensures the template classes do not have to change again when types or logic changes
///	around the time series data
/// </remarks>
public class RuleTemplateDependencies : IRuleTemplateDependencies
{
	private readonly ITimeSeriesManager timeSeriesManager;
	private readonly IEventHubService eventHubService;
	private readonly Dictionary<string, IMLRuntime> mlModels;
	private readonly Dictionary<string, TimeSeries> ruleInstanceBuffers = new();
	private readonly bool sendToAdx = true;

	public int Count => ruleInstanceBuffers.Count(v => v.Value is not null);

	public bool ApplyCompression { get; set; }

	public bool OptimizeCompression { get; set; }

	public double? Compression { get; set; } = null;

	public RuleTemplateDependencies(RuleInstance ruleInstance,
		ITimeSeriesManager timeSeriesManager,
		IEventHubService eventHubService,
		Dictionary<string, IMLRuntime> mlModels,
		bool sendToAdx = true)
	{
		this.timeSeriesManager = timeSeriesManager;
		this.eventHubService = eventHubService;
		this.mlModels = mlModels;
		this.ApplyCompression = true;
		this.OptimizeCompression = true;
		this.sendToAdx = sendToAdx;

		foreach (var point in ruleInstance.PointEntityIds)
		{
			if (!ruleInstanceBuffers.ContainsKey(point.Id))  // not already seen (expression can refer to same point more than once)
			{
				timeSeriesManager.TryGetByTwinId(point.Id, out var timeSeries);

				//add nulls. We can take advantage of fast key lookups for CapabilityExistsInRule method
				ruleInstanceBuffers.Add(point.Id, timeSeries!);
			}
		}
	}

	public bool TryGetTimeSeriesByTwinId(string twinId, out TimeSeries? values)
	{
		values = null;

		if (ruleInstanceBuffers.TryGetValue(twinId, out var ts) && ts is not null)
		{
			values = ts;
			return true;
		}

		return false;
	}

	public IEnumerable<KeyValuePair<string, TimeSeries>> GetAllTimeSeries(RuleInstance ruleInstance)
	{
		return ruleInstanceBuffers.Where(v => v.Value is not null);
	}

	public bool HasTimeSeriesData(string twinId)
	{
		return TryGetTimeSeriesByTwinId(twinId, out _);
	}

	public Task<TimeSeries?> GetOrAdd(string pointId, string connectorId, string externalId)
	{
		return timeSeriesManager.GetOrAdd(pointId, connectorId, externalId);
	}

	public async Task<bool> SendToADX(EventHubServiceDto payload, CancellationToken? waitOrSkipToken = null)
	{
		if (sendToAdx)
		{
			return await eventHubService.WriteAsync(payload, waitOrSkipToken: waitOrSkipToken);
		}

		return true;
	}

	public IMLRuntime? GetMLModel(string fullName)
	{
		mlModels.TryGetValue(fullName, out var mnodel);

		return mnodel;
	}

	public bool CapabilityTwinExistsInRule(string id)
	{
		return ruleInstanceBuffers.ContainsKey(id);
	}
}
