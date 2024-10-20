using Abodit.Graph;
using Abodit.Mutable;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Willow.Expressions;
using Willow.Rules.Repository;
using WillowRules.Extensions;
using WillowRules.Filters;

// POCO class serialized to DB
#nullable disable
namespace Willow.Rules.Model;

/// <summary>
/// TimeSeries validity statuses
/// </summary>
[Flags]
public enum TimeSeriesStatus
{
	/// <summary>Timeseries is valid</summary>
	Valid = 0,
	/// <summary>The sensor appears to be offline, no data has been received for > 2 x expected period</summary>
	Offline = 1,
	/// <summary>The sensor has been stuck on the same value for a long time</summary>
	Stuck = 2,
	/// <summary>The value is above the maximum or below the minimum for this type</summary>
	ValueOutOfRange = 4,
	/// <summary>The period is too far from the expected interval</summary>
	PeriodOutOfRange = 8,
	/// <summary>The sensor time series has no twin Id</summary>
	NoTwin = 16
}

/// <summary>
/// A buffered window of time series values for a single point / capability
/// </summary>
/// <remarks>
/// The Id is the trendId which will later need to evolve for cases where we have
/// connectorId and equipmentId instead.
///
/// The timeseries buffers are used during rule execution and for calculating statistics over time.
/// </remarks>
public class TimeSeries : TimeSeriesBuffer, IId
{
	/// <summary>
	/// Deserialization only constructor, do not call
	/// </summary>
	[JsonConstructor]
	public TimeSeries()
	{
	}

	/// <summary>
	/// Creates a new TimeSeries
	/// </summary>
	public TimeSeries(string id, string unit)
	{
		this.Id = id ?? throw new ArgumentNullException(nameof(id));
		if (string.IsNullOrEmpty(id)) throw new ArgumentException("Id cannot be empty", nameof(id));

		UnitOfMeasure = unit;
	}

	/// <summary>
	/// The value is above the maximum or below the minimum for this type
	/// </summary>
	public bool IsValueOutOfRange { get; set; }

	/// <summary>
	/// The period is too far from the expected interval
	/// </summary>
	public bool IsPeriodOutOfRange { get; set; }

	/// <summary>
	/// The sensor has been stuck on the same value for a long time
	/// </summary>
	public bool IsStuck { get; set; }

	/// <summary>
	/// The sensor appears to be offline, no data has been received for > 2 x expected period
	/// </summary>
	public bool IsOffline { get; set; }

	/// <summary>
	/// Sets the status values according to the period and value vs expected
	/// </summary>
	public void SetStatus(DateTimeOffset now)
	{
		string modelId = this.ModelId ?? "";  // need a not-null string

		if (ValueOutOfRangeEstimator is not null)
		{
			this.IsValueOutOfRange = ValueOutOfRangeEstimator.State > ValueOutOfRangeEstimator.BenchMark;
		}
		else
		{
			// In case sensor changes type and no longer needs to be checked
			this.IsValueOutOfRange = false;
		}

		bool isTextValue = !string.IsNullOrEmpty(LastValueString);

		// Don't declare it stuck until we have seen enough values (now 50)
		this.IsStuck = this.TotalValuesProcessed > 50 && (this.MinValue == this.MaxValue) &&
			// Ignore sensors sat on zero - most of these are legitimate:
			// damper closed, no electricity flowing, no people, ...
			(this.MinValue != 0) &&
			!isTextValue &&
			// ignore setpoints, and actuators they often don't change
			!(modelId.EndsWith("Actuator;1")) &&
			!(modelId.EndsWith("Setpoint;1")) &&
			!(modelId.EndsWith("Energy;1")) &&  // ignore energy sensors, they can stay on the same value for a long time
			!(modelId.Equals("dtmi:com:willowinc:Sensor;1")) && // ignore very generic sensors
			!string.Equals(UnitOfMeasure, "bool", StringComparison.OrdinalIgnoreCase);//ignore capabilities for bool

		//some telemetry comes in slower than the configured interval so we take the largest of trendinverval vs estimatedperiod
		double trendInterval = Math.Max(EstimatedPeriod.TotalSeconds, TrendInterval ?? 0);

		this.IsOffline = ((now - LastSeen > TimeSpan.FromDays(7)) //data is older than 7 days
				|| (TrendInterval.HasValue && (now - LastSeen).TotalSeconds > (trendInterval * 10)))//last seen time is 10x greater than the twin's interval
				&& !isTextValue;//text based values (usually events) can be very far in between so they cant go offline

		this.IsPeriodOutOfRange = this.TotalValuesProcessed > 1 && (this.TrendInterval.HasValue &&
			((this.EstimatedPeriod.TotalSeconds < 0.1 * this.TrendInterval.Value) ||
			(this.EstimatedPeriod.TotalSeconds > 1.9 * this.TrendInterval.Value)));

		// TODO: Lots more validation code
	}

	/// <summary>
	/// Gets the status of the timeseries
	/// </summary>
	public TimeSeriesStatus GetStatus()
	{
		TimeSeriesStatus status = TimeSeriesStatus.Valid;

		if (IsStuck)
		{
			status |= TimeSeriesStatus.Stuck;
		}

		if (IsOffline)
		{
			status |= TimeSeriesStatus.Offline;
		}

		if (IsValueOutOfRange)
		{
			status |= TimeSeriesStatus.ValueOutOfRange;
		}

		if (IsPeriodOutOfRange)
		{
			status |= TimeSeriesStatus.PeriodOutOfRange;
		}

		if (string.IsNullOrWhiteSpace(DtId))
		{
			status |= TimeSeriesStatus.NoTwin;
		}

		return status;
	}

	/// <summary>
	/// Time series is used by a rule so keep more than three and enable validation objects
	/// </summary>
	/// <remarks>
	/// New buffers are created with a default of keeping just three values,
	/// need to tell them to keep all history up to the max time limit
	/// which is set eparately
	/// </remarks>
	public void SetUsedByRule()
	{
		this.MaxCountToKeep = null;
	}

	/// <summary>
	/// The TrendId or some hacked external id plus connector id
	/// </summary>
	[JsonProperty("id")]
	public string Id { get; set; }

	/// <summary>
	/// The digital twin Id (or null)
	/// </summary>
	/// <remarks>
	/// When this is not present in the timeseries data we have to go do a (slow) lookup
	/// to find the trendId or the external Id in ADT. This may be null until it's been
	/// looked up!
	///
	/// We set this value during a full cache scan of the Twin.
	/// </remarks>
	public string DtId { get; set; }

	/// <summary>
	/// The DTDL model id, e.g. ...Sensor;1
	/// </summary>
	public string ModelId { get; set; }

	/// <summary>
	/// The Connector Id for the twin. Optionally used to identify a twin with ExternalId
	/// </summary>
	public string ConnectorId { get; set; }

	/// <summary>
	/// The External Id for the twin. Optionally used to identify a twin with ConnectorId
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	/// The Trend interval (in seconds) copied over from the twin
	/// </summary>
	public int? TrendInterval { get; set; }

	/// <summary>
	/// The first time a point was seen
	/// </summary>
	public DateTimeOffset EarliestSeen { get; set; } = DateTimeOffset.MaxValue;

	/// <summary>
	/// The last time a point was seen
	/// </summary>
	public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.MinValue;

	/// <summary>
	/// The total values processed so far
	/// </summary>
	public long TotalValuesProcessed { get; set; }

	/// <summary>
	/// Kalman filter state for estimating average period
	/// </summary>
	public Kalman AveragePeriodEstimator { get; set; }

	/// <summary>
	/// Kalman filter state for estimating out of range value
	/// </summary>
	public BinaryKalmanFilter ValueOutOfRangeEstimator { get; set; }

	/// <summary>
	/// A Kalman filtered estimate of the average period
	/// </summary>
	public TimeSpan EstimatedPeriod { get; set; } = TimeSpan.Zero;

	/// <summary>
	/// The average value in the current buffer (calculated on the fly)
	/// </summary>
	[NotMapped]
	public double AverageInBuffer { get { return this.points.Count > 0 ? this.points.Average(x => x.NumericValue) : 0; } private set { } }//private setter for EF

	/// <summary>
	/// The average value over time
	/// </summary>
	public double? AverageValue { get; set; }

	/// <summary>
	/// Unit of measure
	/// </summary>
	public double? LastValueDouble { get; set; }

	/// <summary>
	/// Unit of measure
	/// </summary>
	public bool? LastValueBool { get; set; }

	/// <summary>
	/// Unit of measure
	/// </summary>
	public string LastValueString { get; set; }

	/// <summary>
	/// The maximum value over time
	/// </summary>
	public double MaxValue { get; set; } = -1E300;

	/// <summary>
	/// The minimum value over time
	/// </summary>
	public double MinValue { get; set; } = 1E300;

	/// <summary>
	/// The sum of all values over time
	/// </summary>
	public double TotalValue { get; set; }

	/// <summary>
	/// Default compression - changed only by unit tests
	/// </summary>
	private TrajectoryCompressor trajectoryCompressor = defaultTrajectoryCompressor;

	/// <summary>
	/// Kalman filter state for estimating latency
	/// </summary>
	public OneDimensionalKalman LatencyEstimator { get; set; }

	/// <summary>
	/// Kalman filtered latency
	/// </summary>
	public TimeSpan Latency { get; set; } = TimeSpan.Zero;

	/// <summary>
	/// Parent chain by locatedIn and isPartOf
	/// </summary>
	/// <remarks>
	public IList<TwinLocation> TwinLocations { get; set; } = new List<TwinLocation>(0);

	/// <summary>
	/// Disables latency kalman state
	/// </summary>
	public void DisableLatencyEstimator()
	{
		LatencyEstimator = null;
	}

	/// <summary>
	/// Disables compression and clears related kalman filters
	/// </summary>
	public void DisableCompression()
	{
		CompressionState = null;
	}

	/// <summary>
	/// Disables validation and clears related kalman filters
	/// </summary>
	public void DisableValidation()
	{
		ValueOutOfRangeEstimator = null;
	}

	/// <summary>
	/// Enables applicable validation
	/// </summary>
	public void EnableValidation()
	{
		if (Unit.HasRange(UnitOfMeasure, ModelId))
		{
			ValueOutOfRangeEstimator ??= new BinaryKalmanFilter();
		}
		else
		{
			ValueOutOfRangeEstimator = null;
		}
	}

	/// <summary>
	/// Disables estimated period and clears related kalman filter
	/// </summary>
	public void DisableEstimatedPeriod()
	{
		AveragePeriodEstimator = null;
		EstimatedPeriod = TimeSpan.Zero;
	}

	/// <summary>
	/// Disables estimated period and clears related kalman filter
	/// </summary>
	public void EnableEstimatedPeriod()
	{
		AveragePeriodEstimator ??= new Kalman();
	}

	/// <summary>
	/// Verify data quality and adds a point to the buffer
	/// </summary>
	public bool AddPoint(in TimedValue newValue, bool applyCompression, bool includeDataQualityCheck = true, bool reApplyCompression = true)
	{
		if (!IsValidIncomingPoint(newValue))
		{
			return false;
		}

		if (includeDataQualityCheck)
		{
			EnableValidation();
			EnableEstimatedPeriod();
		}

		var qualityCheckPassed = ValidateDataQualityAndUpdateFilters(newValue);

		if (includeDataQualityCheck && !qualityCheckPassed)
		{
			//still need to update lastseen as it is used to check if timeseries is offline
			LastSeen = newValue.Timestamp;
			return false;
		}

		bool added = AddPoint(in newValue, applyCompression, trajectoryCompressor ?? defaultTrajectoryCompressor, reApplyCompression: reApplyCompression);

		// On save we will apply limits for maxCapacity and minDate
		UpdateCounters(in newValue);

		return added;
	}

	/// <summary>
	/// Gets the compression for the buffer
	/// </summary>
	/// <returns></returns>
	public double GetCompression()
	{
		return trajectoryCompressor?.Percentage ?? 0;
	}

	/// <summary>
	/// Sets the compression rate for the time series' trajectory comprression
	/// </summary>
	public void SetCompression(double compressionRate)
	{
		if (this.trajectoryCompressor.Percentage != compressionRate)
		{
			this.trajectoryCompressor = new TrajectoryCompressor(compressionRate);
		}
	}

	/// <summary>
	/// Sets the compression rate based on the ontology
	/// </summary>
	public void SetCompression(Graph<ModelData, Relation> ontology)
	{
		SetCompression(ontology, defaultTrajectoryCompressor.Percentage);
	}

	/// <summary>
	/// Sets the compression rate based on the ontology or fallback compression
	/// </summary>
	public void SetCompression(Graph<ModelData, Relation> ontology, double defaultCompression)
	{
		double compression = defaultCompression;

		if (ontology.IsAncestorOrEqualTo(ModelId, "dtmi:com:willowinc:RefrigerantPressureSensor;1"))
		{
			compression = 0.3;
		}

		SetCompression(compression);
	}

	private void UpdateCounters(in TimedValue lastPoint)
	{
		this.EarliestSeen = lastPoint.Timestamp < EarliestSeen ? lastPoint.Timestamp : EarliestSeen;
		this.LastSeen = lastPoint.Timestamp;
		this.LastValueBool = lastPoint.ValueBool;
		this.LastValueDouble = lastPoint.ValueDouble;
		this.LastValueString = lastPoint.ValueText;

		// cannot just compare to >0 as many values are zero
		if (lastPoint.ValueDouble.HasValue || lastPoint.ValueBool.HasValue)
		{
			var numericValue = lastPoint.NumericValue;

			TotalValuesProcessed++;
			MaxValue = Math.Max(numericValue, MaxValue);
			MinValue = Math.Min(numericValue, MinValue);
			double totalValue = TotalValue + numericValue;

			if (!double.IsPositiveInfinity(totalValue)
			 && !double.IsNegativeInfinity(totalValue)
			 && !double.IsNaN(totalValue))
			{
				TotalValue = totalValue;
				if (TotalValue != 0
				 && TotalValuesProcessed > 0)
				{
					AverageValue = TotalValue / TotalValuesProcessed;
				}
				else
				{
					AverageValue = 0;
				}
			}
		}

		if (LastGap > TimeSpan.Zero && AveragePeriodEstimator is not null)
		{
			double est = KalmanFilter.Update(this.AveragePeriodEstimator, LastGap.TotalSeconds);
			EstimatedPeriod = TimeSpan.FromSeconds(est);
		}
	}

	private bool ValidateDataQualityAndUpdateFilters(TimedValue newValue)
	{
		bool timedValuePointIsOutOfRange = false;

		if (ValueOutOfRangeEstimator is not null)
		{
			(_, timedValuePointIsOutOfRange) = Unit.IsOutOfRange(this.UnitOfMeasure, this.ModelId ?? "", newValue.NumericValue);
			BinaryKalmanFilterFunctions.Update(this.ValueOutOfRangeEstimator, timedValuePointIsOutOfRange);
		}

		if (timedValuePointIsOutOfRange)
		{
			return false;
		}

		return true;
	}

	/// <summary>
	/// Sets the estimated latency using the this.LatencyEstimator
	/// </summary>
	public void SetLatencyEstimate(TimeSpan variance)
	{
		if (LatencyEstimator == null)
		{
			//Initialize the estimator with the current latency setting
			LatencyEstimator ??= new(initialEstimate: Latency.TotalSeconds);
		}

		var latencyEstimate = OneDimensionalKalmanFunctions.Update(LatencyEstimator, variance.TotalSeconds);
		Latency = TimeSpan.FromSeconds(latencyEstimate);
	}
}
