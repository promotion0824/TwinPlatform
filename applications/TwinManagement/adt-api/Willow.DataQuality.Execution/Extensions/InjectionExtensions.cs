using Microsoft.Extensions.DependencyInjection;
using Willow.DataQuality.Execution.Checkers;
using Willow.DataQuality.Execution.Parsers;
using Willow.DataQuality.Model.Rules;
using Willow.DataQuality.Model.Validation;

namespace Willow.DataQuality.Execution.Extensions;

public static class InjectionExtensions
{
    public static IServiceCollection AddRuleCheckers(this IServiceCollection services)
    {
        services.AddTransient<IRuleTemplateChecker, RuleTemplateChecker>();
        services.AddTransient<IPathParser, PathParser>();
        services.AddTransient<IRuleBodyChecker<RuleTemplateExpression, ExpressionValidationResult>, ExpressionsChecker>();
        services.AddTransient<IRuleBodyChecker<RuleTemplatePath, PathValidationResult>, PathsChecker>();
        services.AddTransient<IRuleBodyChecker<RuleTemplateProperty, PropertyValidationResult>, PropertiesChecker>();

        return services;
    }
}
