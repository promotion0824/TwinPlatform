using Abodit.Mutable;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Services;

namespace WillowRules.Test.Bugs.Mocks;

internal class RuleServiceMock : IRulesService
{
	private IRulesService rulesService;
	private bool optimizeExpressions;

	public RuleServiceMock(IRulesService rulesService, bool optimizeExpressions)
	{
		this.rulesService = rulesService ?? throw new ArgumentNullException(nameof(rulesService));
		this.optimizeExpressions = optimizeExpressions;
	}

	public Task<Env> AddGlobalsToEnv(Env env)
	{
		return rulesService.AddGlobalsToEnv(env);
	}

	public Env AddGlobalsToEnv(Env env, IEnumerable<GlobalVariable> globals)
	{
		return rulesService.AddGlobalsToEnv(env, globals);
	}

	public Task<Env> AddMLModelsToEnv(Env env)
	{
		return rulesService.AddMLModelsToEnv(env);
	}

	public Task<IEnumerable<RuleInstance>> GenerateADTCalculatedPoints(ProgressTrackerForRuleGeneration tracker, Env globalEnv, CancellationToken cancellationToken = default)
	{
		return rulesService.GenerateADTCalculatedPoints(tracker, globalEnv, cancellationToken);
	}

	public Task<(bool ok, string error, TokenExpression? expression)> ParseGlobal(GlobalVariable global)
	{
		return rulesService.ParseGlobal(global);
	}

	public Task ProcessCalculatedPoints(ProgressTrackerForRuleGeneration tracker, IEnumerable<Rule> rulesLookup)
	{
		return rulesService.ProcessCalculatedPoints(tracker, rulesLookup);
	}

	public Task<RuleInstance> ProcessOneTwin(Rule rule, TwinDataContext twinContext, Env env, Dictionary<string, Rule> rulesLookup, Dictionary<string, Graph<BasicDigitalTwinPoco, WillowRelation>>? graphLookup = null, bool optimizeExpressions = true)
	{
		return rulesService.ProcessOneTwin(rule, twinContext, env, rulesLookup, graphLookup, this.optimizeExpressions);
	}
}
