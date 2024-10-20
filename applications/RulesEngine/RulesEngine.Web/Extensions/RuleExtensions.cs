using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Willow.ExpressionParser;
using Willow.Expressions.Visitor;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace RulesEngine.Web;

/// <summary>
/// Rule extensions
/// </summary>
public static class RuleExtensions
{
    /// <summary>
    /// Gets Rule Templates with defaults
    /// </summary>
    public static RuleTemplateDto[] GetPopulatedRuleTemplateDtos()
    {
        var duration = new ExpressionField("Duration", "duration")
        {
            ValueString = "TIME/60/60",
            Units = "hours"
        };

        var totalEnergyToDate = new ExpressionField("Total Energy to Date", "total_energy_to_date")
        {
            ValueString = "0",
            Units = "kWh",
        };

        var totalCostToDate = new ExpressionField("Total Cost to Date", "total_cost_to_date")
        {
            ValueString = "CalcUtilityCostFromEnergy(total_energy_to_date)",
            Units = "USD"
        };

        var dailyAvoidableEnergy = new ExpressionField("Daily Avoidable Energy", "daily_avoidable_energy")
        {
            ValueString = "DailyAvoidableEnergy(total_energy_to_date)",
            Units = "kWh",
        };

        var dailyAvoidableCost = new ExpressionField("Daily Avoidable Cost", "daily_avoidable_cost")
        {
            ValueString = "CalcUtilityCostFromEnergy(daily_avoidable_energy)",
            Units = "USD"
        };

        var priority = new ExpressionField("Priority", "priority_impact")
        {
            ValueString = "50"
        };

        var defaultScores = new RuleUIElementDto[]
        {
           new RuleUIElementDto(duration),
           new RuleUIElementDto(totalEnergyToDate),
           new RuleUIElementDto(totalCostToDate),
           new RuleUIElementDto(dailyAvoidableEnergy),
           new RuleUIElementDto(dailyAvoidableCost),
           new RuleUIElementDto(priority)
        };

        var ruleTemplates = new[] {
            new RuleTemplateDto
            {
                Id = RuleTemplateAnyFault.ID,
                Name = RuleTemplateAnyFault.NAME,
                Description = RuleTemplateAnyFault.DESCRIPTION,
                Parameters = new RuleUIElementDto[]
                {
                    new RuleUIElementDto(Fields.Sensor.With("FAHRENHEIT(OPTION([dtmi:com:willowinc:ZoneAirTemperatureSensor;1]))")),
                    new RuleUIElementDto(Fields.Setpoint.With("FAHRENHEIT(OPTION([dtmi:com:willowinc:AirTemperatureSetpoint;1]))")),
                    new RuleUIElementDto(Fields.Occupied.With("[OCCUPANCYSENSOR]=1")),
                    new RuleUIElementDto(Fields.Result.With("sensor > setpoint + 5 | sensor < setpoint - 5")),
                },
                ImpactScores = defaultScores,
                Elements = new RuleUIElementDto[] {
                    new RuleUIElementDto(Fields.PercentageOfTime.With(0.25)),
                    new RuleUIElementDto(Fields.OverHowManyHours.With(12)),
                    new RuleUIElementDto(Fields.PercentageOfTimeOff.With(0.25)),
                }
            },
            new RuleTemplateDto
            {
                Id = RuleTemplateCalculatedPoint.ID,
                Name = RuleTemplateCalculatedPoint.NAME,
                Description = RuleTemplateCalculatedPoint.DESCRIPTION,
                Parameters = new RuleUIElementDto[]
                {
                    new RuleUIElementDto(Fields.Result.With("FAHRENHEIT([dtmi:com:willowinc:ZoneAirTemperatureSensor;1])-FAHRENHEIT([dtmi:com:willowinc:ZoneAirTemperatureSetpoint;1])")),
                },
                Elements = Array.Empty<RuleUIElementDto>()
            }
        };

        return ruleTemplates;
    }

    /// <summary>
    /// Scans a rule's expressions for model ids
    /// </summary>
    public static IEnumerable<string> GetModelIdsForRule(this Rule rule, bool firstPropertyOnly = false)
    {
        var modelIdScanner = new ModelIdScannerVisitor(firstPropertyOnly);

        foreach (var parameter in rule.Parameters)
        {
            try
            {
                var expression = Parser.Deserialize(parameter.PointExpression);
                modelIdScanner.Visit(expression);
            }
            catch (ParserException)
            {
            }
        }

        return modelIdScanner.ModelIds;
    }

    /// <summary>
    /// Gets any new UI elements or fixes backward compatibility issues for existing rules
    /// </summary>
    public static IEnumerable<RuleUIElementDto> GetRuleElements(this Rule rule)
    {
        var ruleTemplate = GetPopulatedRuleTemplateDtos().FirstOrDefault(v => v.Id == rule.TemplateId);

        if (ruleTemplate is null)
        {
            foreach (var element in rule.Elements)
            {
                yield return new RuleUIElementDto(element);
            }
        }
        else
        {
            //sometimes there's duplicates. Should really be fixed somewhere else, but might be hard if it comes from the repo
            var ruleElementsLookup = rule.Elements.GroupBy(v => v.Id).ToDictionary(v => v.Key, v => v.First());

            foreach (var templateElement in ruleTemplate.Elements)
            {
                //backward compatibilty checks here
                if (!ruleElementsLookup.TryGetValue(templateElement.Id, out var ruleElement))
                {
                    if (templateElement.Id == Fields.PercentageOfTimeOff.Id)
                    {
                        if (ruleElementsLookup.TryGetValue(Fields.PercentageOfTime.Id, out var percentageOfTime))
                        {
                            //if PercentageOfTime exists, default PercentageOfTimeOff to the same value which means PercentageOfTimeOff is essentailly ignored
                            yield return new RuleUIElementDto(Fields.PercentageOfTimeOff.With(percentageOfTime.ValueDouble));
                            continue;
                        }
                    }
                    else if (templateElement.Id == Fields.OverHowManyHours.Id)
                    {
                        if (ruleElementsLookup.TryGetValue(Fields.Hours.Id, out var hours))
                        {
                            //for old rules, the "Hours" element was added during rule creation from UI
                            //but it should actually be OverHowManyHours so let's replace it.
                            //TODO this logic can be removed once we are comfortable most rules now have OverHowManyHours instead of Hours
                            yield return new RuleUIElementDto(Fields.OverHowManyHours.With(hours.ValueInt));
                            continue;
                        }
                    }

                    yield return templateElement;
                }
                else
                {
                    yield return new RuleUIElementDto(ruleElement);
                }
            }
        }
    }

    /// <summary>
    /// Creates a <see cref="RuleUIElement" /> from its dto
    /// </summary>
    public static RuleUIElement CreateUIElement(this RuleUIElementDto element)
    {
        switch (element.ElementType)
        {
            case RuleUIElementType.DoubleField:
                return new DoubleField(element.Name, element.Id, element.Units) { ValueDouble = element.ValueDouble };
            case RuleUIElementType.PercentageField:
                return new PercentageField(element.Name, element.Id) { ValueDouble = element.ValueDouble };
            case RuleUIElementType.IntegerField:
                return new IntegerField(element.Name, element.Id, element.Units) { ValueInt = element.ValueInt };
            case RuleUIElementType.StringField:
                return new StringField(element.Name, element.Id) { ValueString = element.ValueString };
            case RuleUIElementType.ExpressionField:
                return new ExpressionField(element.Name, element.Id) { ValueString = element.ValueString };
            default:
                throw new InvalidOperationException($"{element.ElementType} is not a valid element type");
        }
    }

    /// <summary>
    /// Updates a rule from a <see cref="RuleDto"/>
    /// </summary>
    public static bool Update(this Rule rule, RuleDto data, out List<ValidationReponseElementDto> validationsResult)
    {
        var validations = new List<ValidationReponseElementDto>();
        validationsResult = new List<ValidationReponseElementDto>();

        var requiredFields = new Dictionary<string, string>()
        {
            [nameof(data.Name)] = data.Name,
            [nameof(data.PrimaryModelId)] = data.PrimaryModelId
        };

        if (data.IsCalculatedPoint)
        {
            requiredFields.Add(nameof(data.RelatedModelId), data.RelatedModelId);
        }
        else
        {
            requiredFields.Add(nameof(data.Category), data.Category);
            requiredFields.Add(nameof(data.Recommendations), data.Recommendations);
        }

        foreach (var (key, value) in requiredFields)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                //key must be lowercase to match with UI fields registered
                validations.Add(new ValidationReponseElementDto(key.ToLowerInvariant(), "Value is required."));
            }
        }

        if (validations.Any())
        {
            validationsResult = validations;

            return false;
        }

        bool changed = false;
        changed |= data.Name.SetValue(rule, (x) => rule.Name);
        changed |= data.Description.SetValue(rule, (x) => rule.Description);
        changed |= data.Recommendations.SetValue(rule, (x) => rule.Recommendations);
        changed |= data.Category.SetValue(rule, (x) => rule.Category);
        changed |= data.PrimaryModelId.SetValue(rule, (x) => rule.PrimaryModelId);
        changed |= data.RelatedModelId.SetValue(rule, (x) => rule.RelatedModelId);
        changed |= data.LanguageDescriptions.SetValue(rule, (x) => rule.LanguageDescriptions);
        changed |= data.LanguageNames.SetValue(rule, (x) => rule.LanguageNames);
        changed |= data.LanguageRecommendations.SetValue(rule, (x) => rule.LanguageRecommendations);
        changed |= data.Tags.SetValue(rule, (x) => rule.Tags);

        //first remove old elements
        if (data.Elements is not null)
        {
            changed |= rule.Elements.RemoveAll(v => !data.Elements.Any(e => e.Id == v.Id)) > 0;
        }

        var ruleElements = rule.Elements.ToDictionary(v => v.Id);

        //run through the dto's list first if there are any new elements
        if (data.Elements is not null)
        {
            foreach (var obj2 in data.Elements)
            {
                if (!ruleElements.TryGetValue(obj2.Id, out var ui))
                {
                    changed = true;
                    ui = obj2.CreateUIElement();
                    rule.Elements.Add(ui);
                }

                string parameterId = ui.Id;

                if (ui.ElementType == RuleUIElementType.DoubleField)
                {
                    changed |= obj2.ValueDouble.SetValue(ui, (v) => v.ValueDouble);
                }
                else if (ui.ElementType == RuleUIElementType.IntegerField)
                {
                    changed |= obj2.ValueDouble.SetValue(ui, (v) => v.ValueDouble);
                    changed |= obj2.ValueInt.SetValue(ui, (v) => v.ValueInt);
                }
                else if (ui.ElementType == RuleUIElementType.PercentageField)
                {
                    if (obj2.ValueDouble >= 0.0 && obj2.ValueDouble <= 1)
                    {
                        changed |= obj2.ValueDouble.SetValue(ui, (v) => v.ValueDouble);
                    }
                    else
                    {
                        validations.Add(new ValidationReponseElementDto(parameterId, "Must be a number between 0 and 1"));
                    }
                }
                else if (ui.ElementType == RuleUIElementType.ExpressionField)
                {
                    changed |= obj2.ValueString.SetValue(ui, (v) => v.ValueString);
                }
                else if (ui.ElementType == RuleUIElementType.StringField)
                {
                    changed |= obj2.ValueString.SetValue(ui, (v) => v.ValueString);
                }
            }
        }

        var updateDependencies = (IEnumerable<RuleDependencyDto> source, IList<RuleDependency> target) =>
        {
            //nothing provided, ignore
            if (source is null)
            {
                return target;
            }

            var result = new List<RuleDependency>();

            if (source.Count() == 0 && target.Count != 0)
            {
                //return empty, it's  been cleared
                changed = true;
                return result;
            }

            var lookup = target.DistinctBy(v => v.RuleId).ToDictionary(v => v.RuleId);

            foreach (var item in source)
            {
                if (string.IsNullOrWhiteSpace(item.Relationship))
                {
                    validations.Add(new ValidationReponseElementDto(item.Relationship, "A relationship is required"));
                }
                else
                {
                    if (!lookup.TryGetValue(item.RuleId, out var existingValue))
                    {
                        changed = true;
                        existingValue = new RuleDependency(item.RuleId, item.Relationship);
                    }
                    else
                    {
                        changed |= item.Relationship.SetValue(existingValue, (x) => existingValue.Relationship);
                    }

                    result.Add(existingValue);
                }
            }

            if (validations.Count == 0)
            {
                //catch order and size changes
                var collectionChanged = !target.Select(v => v.RuleId).SequenceEqual(result.Select(v => v.RuleId));

                changed |= collectionChanged;
            }

            return result;
        };

        var updateRuleTriggers = (IEnumerable<RuleTriggerDto> source, IList<RuleTrigger> target) =>
        {
            //nothing provided, ignore
            if (source is null)
            {
                return target;
            }

            var result = new List<RuleTrigger>();

            if (source.Count() == 0 && target.Count != 0)
            {
                //return empty, it's  been cleared
                changed = true;
                return result;
            }

            changed |= source.Count() != target.Count();

            int index = 0;

            foreach (var item in source)
            {
                RuleTrigger value;

                if (target.Count <= index)
                {
                    value = new RuleTrigger(item.TriggerType);
                }
                else
                {
                    value = target[index];

                    if(value.TriggerType != item.TriggerType)
                    {
                        value = new RuleTrigger(item.TriggerType);
                    }
                }

                changed |= item.Name.SetValue(value, (x) => value.Name);

                value.Condition = item.Condition.UpdateParameter(value.Condition, validations, ref changed);
                value.Point = item.Point.UpdateParameter(value.Point, validations, ref changed);
                value.Value = item.Value.UpdateParameter(value.Value, validations, ref changed);

                changed |= item.CommandType.SetValue(value, (x) => value.CommandType);

                result.Add(value);

                index++;
            }

            return result;
        };

        rule.Parameters = data.Parameters.UpdateParameters(rule.Parameters, validations, ref changed);

        rule.ImpactScores = data.ImpactScores.UpdateParameters(rule.ImpactScores, validations, ref changed);

        rule.Filters = data.Filters.UpdateParameters(rule.Filters, validations, ref changed);

        rule.Dependencies = updateDependencies(data.Dependencies, rule.Dependencies);

        rule.RuleTriggers = updateRuleTriggers(data.RuleTriggers, rule.RuleTriggers);

        validationsResult = validations;

        return changed;
    }
}
