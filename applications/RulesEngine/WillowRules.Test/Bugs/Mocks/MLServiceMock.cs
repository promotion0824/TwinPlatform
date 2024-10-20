using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Services;

namespace WillowRules.Test.Bugs.Mocks;

public class MLServiceMock : IMLService
{
	public string lastModelName = "";

	public double lastValue
	{
		get
		{
			return fakeModel?.lastValue?.ToDouble(null) ?? 0;
		}
	}

	private FakeModel? fakeModel;

	private MLService mlService { get; set; }

	public MLServiceMock(MLService mlService)
	{
		this.mlService = mlService;
		this.fakeModel = null;
	}

	public MLModel FillModel(MLModel model)
	{
		return mlService.FillModel(model);
	}

	public async Task<Dictionary<string, IMLRuntime>> ScanForModels(IEnumerable<RuleInstance> ruleInstances)
	{
		var result = await mlService.ScanForModels(ruleInstances);

		if (result.Keys.Any())
		{
			lastModelName = result.Keys.First();

			fakeModel = new FakeModel(result[lastModelName]);

			result[lastModelName] = fakeModel;
		}

		return result;
	}

	public (bool ok, string error) ValidateModel(MLModel model)
	{
		return mlService.ValidateModel(model);
	}

	private class FakeModel : IMLRuntime
	{
		private readonly IMLRuntime model;
		public IConvertible? lastValue;

		public FakeModel(IMLRuntime model)
		{
			this.model = model;
		}

		public IConvertible Run(IConvertible[][] input)
		{
			var result = model.Run(input);

			lastValue = result;

			return result;
		}
	}
}
