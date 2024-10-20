using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Extensions.Logging;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace Willow.Rules.Services;

/// <summary>
/// A service that integrates with machine learning models
/// </summary>
public interface IMLService
{
	/// <summary>
	/// Fills missing metdata of a model <see cref="MLModel"/> metadata
	/// </summary>
	/// <param name="model"></param>
	/// <returns></returns>
	MLModel FillModel(MLModel model);

	/// <summary>
	/// Validates wheter a model can load
	/// </summary>
	(bool ok, string error) ValidateModel(MLModel model);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="ruleInstances"></param>
	/// <returns></returns>
	Task<Dictionary<string, IMLRuntime>> ScanForModels(IEnumerable<RuleInstance> ruleInstances);
}

/// <summary>
/// Machine Learning related service which can currently read ONNX models
/// </summary>
public class MLService : IMLService
{
	private readonly IRepositoryMLModel repositoryMLModel;
	private readonly ILogger<MLService> logger;
	private readonly ILogger throttledLogger;

	/// <summary>
	/// Constructor
	/// </summary>
	public MLService(
		IRepositoryMLModel repositoryMLModel,
		ILogger<MLService> logger)
	{
		this.repositoryMLModel = repositoryMLModel ?? throw new ArgumentNullException(nameof(repositoryMLModel));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.throttledLogger = logger.Throttle(TimeSpan.FromSeconds(60));
	}

	public MLModel FillModel(MLModel model)
	{
		try
		{
			var session = new InferenceSession(model.ModelData);

			if (session.ModelMetadata is not null)
			{
				model.ModelVersion = session.ModelMetadata.Version.ToString();
			}

			var inputParams = new List<MLModelParam>();

			foreach((var key, var input) in session.InputMetadata)
			{
				//get product of total values
				var inputCount = input.GetArrayInputCount();
				string description = $"Input dimensions: [{string.Join(", ", input.Dimensions)}].";

				inputParams.Add(new MLModelParam()
				{
					Name = key,
					Unit = input.ElementType.Name,
					Size = inputCount,
					Description = description
				});
			}

			model.ExtensionData.InputParams = inputParams.ToArray();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Could not fill metadata for model id {id}", model.Id);
		}

		return model;
	}

	public async Task<Dictionary<string, IMLRuntime>> ScanForModels(IEnumerable<RuleInstance> ruleInstances)
	{
		var result = new Dictionary<string, IMLRuntime>();

		var visitor = new FunctionNameScannerVisitor();

		foreach (var ruleInstance in ruleInstances)
		{
			foreach(var parameter in ruleInstance.GetAllBoundParameters())
			{
				visitor.Visit(parameter.PointExpression);
			}
		}

		foreach((var id, var name) in await repositoryMLModel.GetModelNames())
		{
			if(visitor.FunctionNames.Contains(name))
			{
				try
				{
					var model = repositoryMLModel.GetModel(id);

					var session = new InferenceSession(model!.ModelData);

					result[model.FullName] = new OnnxRuntime(session);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Could not load model id {id}", id);
				}
			}
		}

		return result;
	}

	public (bool ok, string error) ValidateModel(MLModel model)
	{
		(bool ok, string validation) = model.ValidateMLModel();

		if (ok)
		{
			try
			{
				var session = new InferenceSession(model.ModelData);

				foreach((var key, var input) in session.InputMetadata)
				{
					if(input.Dimensions.Any(v => v < 0))
					{
						//disabled for now, dimension  are forced to psoitive
						//return (false, $"Input param '{key}' dimensions must be non-negative");
					}
				}
			}
			catch (Exception ex)
			{
				return (false, $"Could not load model '{model.FullName}': {ex.Message}");
			}
		}

		return (true, validation);
	}
}
