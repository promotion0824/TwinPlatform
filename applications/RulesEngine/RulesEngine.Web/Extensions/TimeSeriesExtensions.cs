using Kusto.Cloud.Platform.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions;
using Willow.Rules;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using WillowRules.DTO;

namespace RulesEngine.Web;

/// <summary>
/// TimeSeries extensions
/// </summary>
public static class TimeSeriesExtensions
{
    private static readonly string[] OutputValueIds = {
        RuleTemplate.AREA_INCREMENTAL, RuleTemplate.TIME_OUTSIDE_WHILE_OCCUPIED, RuleTemplate.AREA_OUTSIDE_WHILE_OCCUPIED,
        RuleTemplate.PERCENTAGE_FAULTED_24, RuleTemplate.TOTAL_OUTSIDE_24};

    /// <summary>
    /// Creates a <see cref="TimeSeriesDataDto"/> for a rule instance
    /// </summary>
    public static TimeSeriesDataDto GetTimeseriesDataForRuleInstance(this RuleInstance ruleInstance,
        ActorState actorState,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        RuleTemplate template = null,
        Insight insight = null,
        IEnumerable<InsightChange> changes = null,
        IEnumerable<Command> commands = null,
        Dictionary<(string, DateTimeOffset), string> pointLog = null)
    {
        var tz = TimeZoneInfoHelper.From(ruleInstance.TimeZone);
        pointLog ??= new Dictionary<(string, DateTimeOffset), string>();

        List<AxisDto> axes = new();

        var paramIds = ruleInstance.GetAllBoundParameters()
                                   .Select(v => v.FieldId)
                                   .ToHashSet();

        var fieldTriggersBound = ruleInstance.RuleTriggersBound
                                    .SelectMany(trigger => trigger.GetBoundParameters()
                                    .Select(parameter => (trigger, parameter)))
                                    .ToDictionary(v => v.parameter.FieldId);

        var commandLookup = commands?.ToDictionary(v => v.CommandId) ?? new Dictionary<string, Command>();

        TrendlineDto GetTrendLineDto(
            string id,
            TimeSeriesBuffer timeSeries)
        {
            var values = timeSeries.Points;
            var namedPoint = ruleInstance.PointEntityIds.FirstOrDefault(x => x.Id == id);
            string name = namedPoint?.VariableName ?? id;
            var unit = Unit.Get(timeSeries.UnitOfMeasure);
            bool isOutput = id == "result";
            bool isRanking = OutputValueIds.Contains(id);
            bool isPercentageFaulted = id == RuleTemplate.PERCENTAGE_FAULTED;
            bool isDeviation = id.Contains("deviation");
            bool isVariance = id.Contains("variance");
            bool isSystemGenerated = namedPoint is null && !paramIds.Contains(id);

            string key = "";

            if (string.IsNullOrEmpty(key))
            {
                key = isOutput ? "result"
                    : isRanking ? "rank"
                    : isPercentageFaulted ? "% faulted"
                    : isDeviation ? $"{unit.Name} deviation"   // small -2 to +2 values are hard to see against large ones
                    : isVariance ? $"{unit.Name} variance"   // small -2 to +2 values are hard to see against large ones
                    : unit.Name;
            }

            if (string.IsNullOrEmpty(key))
            {
                if (values.All(x => x.ValueBool.HasValue)) { key = name; unit = Unit.boolean; }
                else if (id.EndsWith("ratio")) { key = "ratio"; unit = Unit.Get("ratio"); }
                else if (values.All(x => x.NumericValue > 0.0 && x.NumericValue < 1.0)) key = "small";
                else key = "scalar";//don't allow empty keys
            }

            AxisDto axis = axes.FirstOrDefault(x => x.Key == key);

            if (axis is null)
            {
                int axisIndex = axes.Count();

                axis = new AxisDto
                {
                    Key = key,
                    ShortName = axisIndex == 0 ? "y" : "y" + (axisIndex + 1),
                    LongName = axisIndex == 0 ? "yaxis" : "yaxis" + (axisIndex + 1),
                    Title = isOutput ? "Result" : isRanking ? "Ranking" : unit.Name
                };

                axes.Add(axis);
            }

            // If all the values are bool values this is a stepped line
            // except they are all set as we don't distinguish bool or not
            // when reading the time series data, only when calculating expressions
            bool isStepped = unit == Unit.boolean;

            var data = values.Select(v =>
            {
                var dto = new TimedValueDto(v)
                {
                    Text = pointLog.GetValueOrDefault((id, v.Timestamp), null)
                };

                return dto;
            });

            if (timeSeries.IsCapability())
            {
                //capability buffers are in UTC take them to the rule instance's timezone
                data = values.Select(v => new TimedValueDto(v.Timestamp.UtcDateTime.ConvertToDateTimeOffset(tz!), v));
            }

            bool percFaultedTriggerred = false;

            data = data
                .Select(value =>
                {
                    if (fieldTriggersBound.TryGetValue(id, out var boundTrigger) &&
                       commandLookup.TryGetValue(boundTrigger.trigger.Id, out var command))
                    {
                        var occurrence = command.Occurrences.LastOrDefault(occ => value.Timestamp >= occ.Started);

                        if (occurrence is not null)
                        {
                            value.Triggered = occurrence.IsTriggered;
                            if (value.Triggered)
                            {
                                value.Text = $"{command.CommandType}: {occurrence.Value}<br>{boundTrigger.trigger.Condition.PointExpression.Serialize()}";
                            }
                        }
                    }

                    //show trigger on/off on perc faulted line
                    if (isPercentageFaulted && template is RuleTemplateAnyFault faultTemplate)
                    {
                        bool triggered = value.ValueDouble > faultTemplate.GetPercentageOn() || value.ValueDouble > faultTemplate.GetPercentageOff();

                        if (triggered)
                        {
                            if (!percFaultedTriggerred)
                            {
                                percFaultedTriggerred = true;
                                value.Triggered = true;
                            }
                        }
                        else
                        {
                            if (percFaultedTriggerred)
                            {
                                percFaultedTriggerred = false;
                                value.Triggered = true;
                            }
                        }
                    }

                    return value;
                });

            string trendLineName = $"{name} {unit.Name}".TrimEnd(' ');
            bool isTrigger = false;

            if (fieldTriggersBound.TryGetValue(id, out var trigger))
            {
                isTrigger = true;
                trendLineName = $"{trigger.trigger.TwinName} ({trigger.trigger.Name}: {trigger.trigger.CommandType})";
            }

            return new TrendlineDto
            {
                Id = id,
                Name = trendLineName,
                Unit = unit.Name,
                IsOutput = isOutput,
                IsRanking = isRanking,
                IsSystemGenerated = isSystemGenerated,
                Axis = axis.ShortName,
                Shape = isStepped ? "hv" : "linear",
                IsTrigger = isTrigger,
                Data = data.ToList()
            };
        }

        //order gives consistent line colors if comparing the same chart on to different pages
        var lines = actorState.TimedValues
            .Select(x => GetTrendLineDto(x.Key, x.Value))
            .ToList();

        var lineData = lines.SelectMany(v => v.Data);

        startTime = lineData.Any() ? lineData.Min(v => v.Timestamp) : startTime;
        endTime = lineData.Any() ? lineData.Max(v => v.Timestamp) : endTime;

        var insightLines = new List<TrendlineInsightDto>();

        if (insight is not null)
        {
            var annotations = changes?
                .Where(v => v.Timestamp >= startTime && v.Timestamp <= endTime)
                .Select(v => new TrendlineAnnotationDto()
                {
                    Text = v.Status.ToString(),
                    Timestamp = v.Timestamp
                }).ToList() ?? new List<TrendlineAnnotationDto>();

            foreach (var line in lines.Where(v => v.Data.Any() && v.IsOutput))
            {
                line.Annotations = annotations;
            }

            foreach (var occurrence in insight.Occurrences.Where(v => v.IsFaulted || !v.IsValid))
            {
                if (occurrence.Ended < startTime || occurrence.Started > endTime)
                {
                    continue;
                }

                //dont go outside the min and max of the trendlines else it'll squash the chart
                var started = occurrence.Started < startTime ? startTime : occurrence.Started;
                var ended = occurrence.Ended > endTime ? endTime : occurrence.Ended;

                insightLines.Add(new TrendlineInsightDto()
                {
                    StartTimestamp = started,
                    EndTimestamp = ended,
                    Hours = Math.Round((occurrence.Ended - occurrence.Started).TotalHours, 2),
                    IsValid = occurrence.IsValid,
                });
            }
        }

        return new TimeSeriesDataDto
        {
            StartTime = startTime.ToUniversalTime(),
            EndTime = endTime.ToUniversalTime(),
            Id = ruleInstance.Id,
            Trendlines = lines.ToArray(),
            Insights = insightLines,
            Axes = axes.OrderBy(v => v.ShortName).ToList()
        };
    }
}
