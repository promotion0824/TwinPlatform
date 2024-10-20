using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// Rule extensions
/// </summary>
public static class ObjectMappingExtensions
{
    /// <summary>
    /// Maps parameters
    /// </summary>
    public static RuleParameter UpdateParameter(this RuleParameterDto source,
        RuleParameter target,
        IList<ValidationReponseElementDto> validations,
        ref bool changed)
    {
        var sourceList = new List<RuleParameterDto>();

        if (source is not null)
        {
            sourceList.Add(source);
        }

        var targetList = new List<RuleParameter>();

        if (target is not null)
        {
            targetList.Add(target);
        }

        var result = sourceList.UpdateParameters(targetList, validations, ref changed);

        if (result.Count > 0)
        {
            return result[0];
        }

        return target;
    }

    /// <summary>
    /// Maps parameters
    /// </summary>
    public static IList<RuleParameter> UpdateParameters(this IList<RuleParameterDto> source,
        IList<RuleParameter> target,
        IList<ValidationReponseElementDto> validations,
        ref bool changed)
    {
        //nothing provided, ignore
        if (source is null)
        {
            return target;
        }

        //easier to recreate the list as the order is important
        var updatedParameters = new List<RuleParameter>();

        if (source.Count() == 0 && target.Count != 0)
        {
            changed = true;
            //return empty, it's  been cleared
            return updatedParameters;
        }

        var parameterLookup = target.ToDictionary(v => v.FieldId);

        foreach (var parameter in source)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parameter.Name))
                {
                    validations.Add(new ValidationReponseElementDto(nameof(RuleParameterDto.Name), "Paramter name is required"));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(parameter.FieldId))
                {
                    validations.Add(new ValidationReponseElementDto(nameof(RuleParameterDto.FieldId), "Paramter fieldId is required"));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(parameter.PointExpression))
                {
                    validations.Add(new ValidationReponseElementDto(parameter.FieldId, "An expression is required"));
                }
                else
                {
                    Willow.ExpressionParser.Parser.Deserialize(parameter.PointExpression);
                }

                if (!parameterLookup.TryGetValue(parameter.FieldId, out var existingValue))
                {
                    existingValue = new RuleParameter(parameter.Name, parameter.FieldId, parameter.PointExpression, parameter.Units, parameter.CumulativeSetting);
                }
                else
                {
                    changed |= parameter.Name.SetValue(existingValue, (x) => existingValue.Name);
                    changed |= parameter.Units.SetValue(existingValue, (x) => existingValue.Units);
                    changed |= parameter.PointExpression.SetValue(existingValue, (x) => existingValue.PointExpression);
                    changed |= parameter.CumulativeSetting.SetValue(existingValue, (x) => existingValue.CumulativeSetting);
                }

                updatedParameters.Add(existingValue);
            }
            catch (Willow.ExpressionParser.ParserException pex)
            {
                validations.Add(new ValidationReponseElementDto(parameter.FieldId, pex.Message));
            }
        }

        if (validations.Count == 0)
        {
            //catch order and size changes
            changed |= !target.Select(v => v.FieldId).SequenceEqual(updatedParameters.Select(v => v.FieldId));

            if (changed)
            {
                return updatedParameters;
            }
        }

        return target;
    }

    /// <summary>
    /// Set the value for a field and returns whether that value changed
    /// </summary>
    public static bool SetValue<T>(this string updatedValue,
        T value,
        Expression<Func<T, string>> getter)
    {
        updatedValue = updatedValue?.Trim();
        return updatedValue.SetValue<T, string>(value, getter);
    }

    /// <summary>
    /// Set the value for a field and returns whether that value changed
    /// </summary>
    public static bool SetValue<T, TValue>(this TValue updatedValue,
        T value,
        Expression<Func<T, TValue>> getter)
    {
        var existingValue = getter.Compile()(value);

        (string fieldName, var setter) = GetSetter(getter);

        bool changed = false;

        if (updatedValue is null)
        {
            return false;
        }

        if (!updatedValue.Equals(existingValue))
        {
            setter(value, updatedValue);
            changed = true;
        }

        return changed;
    }

    /// <summary>
    /// Convert a lambda expression for a getter into a setter
    /// </summary>
    private static (string fieldName, Action<T, U> setter) GetSetter<T, U>(Expression<Func<T, U>> expression)
    {
        var memberExpression = (MemberExpression)expression.Body;
        var property = (PropertyInfo)memberExpression.Member;
        string fieldName = property.Name;
        var setMethod = property.GetSetMethod();

        var parameterT = Expression.Parameter(typeof(T), "x");
        var parameterU = Expression.Parameter(typeof(U), "y");

        var newExpression = Expression.Lambda<Action<T, U>>(
            Expression.Call(parameterT, setMethod, parameterU), parameterT, parameterU);

        return (fieldName, newExpression.Compile());
    }

}
