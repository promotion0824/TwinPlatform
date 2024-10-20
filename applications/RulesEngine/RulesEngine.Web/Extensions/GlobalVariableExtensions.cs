using Kusto.Cloud.Platform.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Willow.ExpressionParser;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// GlobalVariable extensions
/// </summary>
public static class GlobalVariableExtensions
{
    /// <summary>
    /// Updates a <see cref="GlobalVariable"/> from a <see cref="GlobalVariableDto"/>
    /// </summary>
    public static bool Update(this GlobalVariable global, GlobalVariableDto data, out List<ValidationReponseElementDto> validationResult)
    {
        var validations = new List<ValidationReponseElementDto>();
        validationResult = new List<ValidationReponseElementDto>();

        if (global.IsBuiltIn)
        {
            validations.Add(new ValidationReponseElementDto(nameof(GlobalVariableDto.Expression), "Built-in globals aren't editable"));
        }

        if (data.Parameters.Select(v => v.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != data.Parameters.Count())
        {
            validations.Add(new ValidationReponseElementDto(nameof(GlobalVariableDto.Parameters), "Parameter names must be unique"));
        }

        if (validations.Any())
        {
            validationResult = validations;

            return false;
        }

        bool changed = false;

        changed |= data.Name.SetValue(global, (x) => global.Name);
        changed |= data.Description.SetValue(global, (x) => global.Description);
        //update the units from the last expression
        changed |= data.Parameters.LastOrDefault()?.Units.SetValue(global, (x) => global.Units) ?? false;
        changed |= data.Tags.SetValue(global, (x) => global.Tags);

        var updateFunctionParameters = (IEnumerable<FunctionParameterDto> source, IList<FunctionParameter> target) =>
        {
            //nothing provided, ignore
            if (source is null)
            {
                return target;
            }

            //easier to recreate the list as the order is important
            var updatedParameters = new List<FunctionParameter>();
            bool paramterUpdated = false;

            if (source.Count() == 0 && target.Count != 0)
            {
                //return empty, it's  been cleared
                changed = true;
                return updatedParameters;
            }

            var parameterLookup = target.ToDictionary(v => v.Name);

            foreach (var parameter in source)
            {
                try
                {
                    if (!parameterLookup.TryGetValue(parameter.Name, out var existingValue))
                    {
                        existingValue = new FunctionParameter(parameter.Name, parameter.Description, parameter.Units);
                    }
                    else
                    {
                        paramterUpdated |= parameter.Name.SetValue(existingValue, (x) => existingValue.Name);
                        paramterUpdated |= parameter.Description.SetValue(existingValue, (x) => existingValue.Description);
                        paramterUpdated |= parameter.Units.SetValue(existingValue, (x) => existingValue.Units);
                    }

                    (bool ok, string validation) = existingValue.ValidateFunctionName();

                    if (!ok)
                    {
                        validations.Add(new ValidationReponseElementDto(parameter.Name, validation));
                    }

                    updatedParameters.Add(existingValue);
                }
                catch (Exception pex)
                {
                    validations.Add(new ValidationReponseElementDto(parameter.Name, pex.Message));
                }
            }

            if (validations.Count == 0)
            {
                //catch order and size changes
                var collectionChanged = !target.Select(v => v.Name).SequenceEqual(updatedParameters.Select(v => v.Name));

                if (paramterUpdated || collectionChanged)
                {
                    changed = true;
                    return updatedParameters;
                }
            }

            return target;
        };

        global.Parameters = updateFunctionParameters(data.Parameters, global.Parameters);

        global.Expression = data.Expression.UpdateParameters(global.Expression, validations, ref changed);

        (bool ok,  string validation) = global.ValidateGlobalName();

        if (!ok)
        {
            validations.Add(new ValidationReponseElementDto(nameof(GlobalVariable.Name), validation));
        }

        validationResult = validations;

        return changed;
    }
}
