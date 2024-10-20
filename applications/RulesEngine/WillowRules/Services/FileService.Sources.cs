using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using WillowRules.Extensions;

namespace Willow.Rules.Services;

public partial class FileService
{
	/// <summary>
	/// an interface used by the file service to save, load and zip objects
	/// </summary>
	interface IEntitySource
	{
		/// <summary>
		/// The source type if the file service needs a specific one
		/// </summary>
		FileServiceSourceType SourceType { get; }

		/// <summary>
		/// Try to deserialize the json if it is the right json
		/// </summary>
		bool TryDeserialize(string filePath, MemoryStream stream, out IId? entity, out bool isValid);

		/// <summary>
		/// Save entity results to DB
		/// </summary>
		Task SaveResult(ProcessResult result, string user, bool overwrite = true);

		/// <summary>
		/// Get entities from repo
		/// </summary>
		/// <returns></returns>
		IAsyncEnumerable<IId> GetEntities();

		/// <summary>
		/// Serialize the object
		/// </summary>
		byte[] Serialize(IId entity);

		/// <summary>
		/// Folder for entity
		/// </summary>
		/// <remarks>
		/// For rules this will be model id, for globals a static name
		/// </remarks>
		string GetFolder(IId entity);

		/// <summary>
		/// Returns preferred extension for source
		/// </summary>
		string GetExtension();

		/// <summary>
		/// Try to read if exists
		/// </summary>
		Task<IId?> GetEntity(string id);

		/// <summary>
		/// Try to delete if exists
		/// </summary>
		Task<bool> Delete(IId entity);
	}

	public class RuleSource : IEntitySource
	{
		private readonly IRepositoryRules repositoryRules;
		private readonly IRepositoryRuleMetadata repositoryRuleMetadata;
		private readonly ILogger logger;

		public RuleSource(IRepositoryRules repositoryRules, IRepositoryRuleMetadata repositoryRuleMetadata, ILogger logger)
		{
			this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
			this.repositoryRuleMetadata = repositoryRuleMetadata ?? throw new ArgumentNullException(nameof(repositoryRuleMetadata));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public static string GetFolder(Rule rule)
		{
			return CoerceValidDirectoryName(rule.PrimaryModelId?.TrimModelId() ?? "");
		}

		public FileServiceSourceType SourceType => FileServiceSourceType.Rule;

		public IAsyncEnumerable<IId> GetEntities()
		{
			return repositoryRules.GetAll();
		}

		public async Task SaveResult(ProcessResult result, string user, bool overwrite = true)
		{
			if (result.Rules.Any())
			{
				var toAddorUpdate = overwrite ? result.Rules.ToList() : new List<Rule>();
				var rulesInfo = new List<(string ruleId, bool tagsChanged, bool isNew)>();

				foreach (var rule in result.Rules)
				{
					var processRule = false;
					var tagsChanged = false;
					var isNew = false;

					//During this step manual updates to rules can be rectified,
					//e.g. Tags entered as single string with multiple comma delimited values or duplicate tags
					if (rule.Tags != null && (rule.Tags.Any(t => t.Split(',').Length > 1) || rule.Tags.Count != rule.Tags.Distinct().Count()))
					{
						rule.Tags = rule.Tags
								.SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
								.Select(t => t.Trim())
								.Where(t => !string.IsNullOrWhiteSpace(t))
								.Distinct()
								.ToList();

						tagsChanged = true;
						processRule = true;
					}

					if (!overwrite)
					{
						var previous = await repositoryRules.GetOne(rule.Id, updateCache: false);
						if (previous == null)
						{
							isNew = true;
							processRule = true;
						}
					}

					if (processRule)
					{
						rulesInfo.Add((rule.Id, tagsChanged, isNew));

						if (!toAddorUpdate.Contains(rule))
						{
							toAddorUpdate.Add(rule);
						}
					}
				}

				if (toAddorUpdate.Count > 0)
				{
					var toAddorUpdateLog = $"{toAddorUpdate.Count} skills.";

					if (rulesInfo.Count > 0)
					{
						toAddorUpdateLog += $" Updates: {string.Join(", ", rulesInfo.Select(r => $"[{r.ruleId}, tagsChanged: {r.tagsChanged}, isNew: {r.isNew}]"))}";
					}

					logger.LogInformation("Uploading/Updating {entry}", toAddorUpdateLog);

					var config = new BulkConfig()
					{
						PropertiesToExcludeOnUpdate =
						[
							nameof(Rule.CommandEnabled),
							nameof(Rule.ADTEnabled)
						]
					};

					await repositoryRules.BulkMerge(toAddorUpdate, config: config);

					var metadata = toAddorUpdate.Select(v => new RuleMetadata(v.Id, user)).ToList();

					var metaConfig = new BulkConfig()
					{
						PropertiesToIncludeOnUpdate =
						[
							nameof(RuleMetadata.ModifiedBy),
							nameof(RuleMetadata.LastModified),
						]
					};

					await repositoryRuleMetadata.BulkMerge(metadata, config: metaConfig);

					result.SaveCount += toAddorUpdate.Count;
				}
			}

			if (result.Duplicates.Any())
			{
				logger.LogWarning("Duplicates ({duplicateCount}): {duplicateFiles}", result.DuplicateCount, string.Join(", ", result.Duplicates.Select(f => $"[{f}]")));
			}

			if (result.DeletedRules.Any())
			{
				logger.LogInformation("Deleting {count} skills", result.DeletedRules.Count());

				await repositoryRules.BulkDelete(result.DeletedRules.ToList());

				await repositoryRuleMetadata.BulkDelete(result.DeletedRules.Select(v => new RuleMetadata(v.Id)).ToList());

				result.SaveCount += result.DeletedRules.Count();
			}
		}

		public string GetFolder(IId entity)
		{
			var rule = (Rule)entity;
			return GetFolder(rule);
		}

		public byte[] Serialize(IId entity)
		{
			var rule = (Rule)entity;

			if (rule.LanguageNames is null || rule.LanguageNames.Count == 0)
			{
				rule.LanguageNames = new Dictionary<string, string>()
				{
					["fr"] = rule.Name + " [TODO translate]"
				};
			}

			if (rule.LanguageDescriptions is null || rule.LanguageDescriptions.Count == 0)
			{
				rule.LanguageDescriptions = new Dictionary<string, string>()
				{
					["fr"] = rule.Description + " [TODO translate]"
				};
			}

			if (rule.LanguageRecommendations is null || rule.LanguageRecommendations.Count == 0)
			{
				rule.LanguageRecommendations = new Dictionary<string, string>()
				{
					["fr"] = rule.Recommendations + " [TODO translate]"
				};
			}

			string json = JsonConvert.SerializeObject(rule, jsonSettings);
			return Encoding.UTF8.GetBytes(json);
		}

		public async Task<bool> Delete(IId entity)
		{
			var rule = await repositoryRules.GetOne(entity.Id, updateCache: false);

			if (rule is not null)
			{
				await repositoryRules.DeleteOne(rule);
				return true;
			}

			return false;
		}

		public bool TryDeserialize(string filePath, MemoryStream stream, out IId? entity, out bool isValid)
		{
			entity = null;
			isValid = true;

			if (ruleFileExtensions.Contains(Path.GetExtension(filePath)))
			{
				if (TryReadFromStream<Rule>(stream, out var rule, settings: jsonSettings, leaveOpen: true))
				{
					if (!string.IsNullOrWhiteSpace(rule!.Id) &&
						!string.IsNullOrWhiteSpace(rule!.TemplateId))
					{
						entity = rule;

						isValid = ValidateRuleAndFile(filePath, rule);

						return true;
					}
				}
			}

			return false;
		}

		private bool ValidateRuleAndFile(string filePath, Rule rule)
		{
			var validationMessages = new StringBuilder();
			var fileInfo = new FileInfo(filePath);
			var currentLocation = fileInfo.Directory?.Name;

			//Validate the rule entity
			(bool isValid, string validationResult) = rule.ValidateRule(logger);
			if (!isValid)
			{
				validationMessages.AppendLine($"Validation failed for rule '{rule.Id}': {validationResult}");
			}

			var expectedLocation = rule.PrimaryModelId.TrimModelId();

			//Validate the rule file location
			var matchFolder = string.Equals(currentLocation, expectedLocation, StringComparison.CurrentCultureIgnoreCase);
			if (!matchFolder)
			{
				validationMessages.AppendLine($"The rule '{fileInfo.Name}' is not in the correct folder. Current: '{currentLocation}' Expected: '{expectedLocation}'");
			}

			//Validate the rule file name matches the rule Id
			var matchRuleIdFileName = string.Equals(rule.Id, Path.GetFileNameWithoutExtension(fileInfo.Name), StringComparison.CurrentCultureIgnoreCase);
			if (!matchRuleIdFileName)
			{
				validationMessages.AppendLine($"The rule Id '{rule.Id}' differs from the file name '{fileInfo.Name}'.");
			}

			//Validate the Id naming standard
			if (!rule.Id.IsToIdStandard())
			{
				validationMessages.AppendLine($"The rule Id '{rule.Id}' does not adhere to id naming standards. Expected: '{rule.Id.ToIdStandard()}'.");
			}

			if (validationMessages.Length > 0)
			{
				logger.LogWarning(validationMessages.ToString());
			}

			return isValid;
		}

		public async Task<IId?> GetEntity(string id)
		{
			var rule = await repositoryRules.GetOne(id, updateCache: false);
			return rule;
		}

		public string GetExtension()
		{
			return ".json";
		}
	}

	public class GlobalSource : IEntitySource
	{
		private readonly IRepositoryGlobalVariable repositoryGlobalVariable;
		private readonly ILogger logger;

		public GlobalSource(IRepositoryGlobalVariable repositoryGlobalVariable, ILogger logger)
		{
			this.repositoryGlobalVariable = repositoryGlobalVariable ?? throw new ArgumentNullException(nameof(repositoryGlobalVariable));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public FileServiceSourceType SourceType => FileServiceSourceType.Global;

		public const string Folder = "globals";

		public async Task<bool> Delete(IId entity)
		{
			var global = await repositoryGlobalVariable.GetOne(entity.Id, updateCache: false);

			if (global is not null)
			{
				await repositoryGlobalVariable.DeleteOne(global);
				return true;
			}

			return false;
		}

		public string GetFolder(IId entity)
		{
			return Folder;
		}

		public async Task SaveResult(ProcessResult result, string user, bool overwrite = true)
		{
			if (result.Globals.Any())
			{
				var toAddorUpdate = overwrite ? result.Globals.ToList() : new List<GlobalVariable>();
				var globalsInfo = new List<(string globalId, bool tagsChanged, bool isNew)>();

				foreach (var global in result.Globals)
				{
					var processGlobal = false;
					var tagsChanged = false;
					var isNew = false;

					//During this step manual updates to globals can be rectified,
					//e.g. Tags entered as single string with multiple comma delimited values or duplicate tags
					if (global.Tags != null && (global.Tags.Any(t => t.Split(',').Length > 1) || global.Tags.Count != global.Tags.Distinct().Count()))
					{
						global.Tags = global.Tags
							.SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
							.Select(t => t.Trim())
							.Where(t => !string.IsNullOrWhiteSpace(t))
							.Distinct()
							.ToList();

						tagsChanged = true;
						processGlobal = true;
					}

					if (!overwrite)
					{
						var previous = await repositoryGlobalVariable.GetOne(global.Id, updateCache: false);
						if (previous == null)
						{
							isNew = true;
							processGlobal = true;
						}
					}

					if (processGlobal)
					{
						globalsInfo.Add((global.Id, tagsChanged, isNew));

						if (!toAddorUpdate.Contains(global))
						{
							toAddorUpdate.Add(global);
						}
					}
				}

				if (toAddorUpdate.Count > 0)
				{
					var toAddorUpdateLog = $"{toAddorUpdate.Count} globals.";

					if (globalsInfo.Count > 0)
					{
						toAddorUpdateLog += $" Updates: {string.Join(", ", globalsInfo.Select(g => $"[{g.globalId}, tagsChanged: {g.tagsChanged}, isNew: {g.isNew}]"))}";
					}

					logger.LogInformation("Uploading/Updating {entry}", toAddorUpdateLog);

					await repositoryGlobalVariable.BulkMerge(toAddorUpdate);

					result.SaveCount += toAddorUpdate.Count;
				}
			}

			if (result.DeletedGlobals.Any())
			{
				logger.LogInformation("Deleting {count} globals", result.DeletedGlobals.Count());

				await repositoryGlobalVariable.BulkDelete(result.DeletedGlobals.ToList());

				result.SaveCount += result.DeletedGlobals.Count();
			}
		}

		public bool TryDeserialize(string filePath, MemoryStream stream, out IId? entity, out bool isValid)
		{
			entity = null;
			isValid = true;

			if (ruleFileExtensions.Contains(Path.GetExtension(filePath)))
			{
				if (TryReadFromStream<JObject>(stream, out var jObject, leaveOpen: true))
				{
					var serializer = JsonSerializer.Create(jsonSettings);

					var global = jObject!.ToObject<GlobalVariable>(serializer);

					if (!string.IsNullOrWhiteSpace(global!.Id) &&
						jObject.Children().Any(v => string.Equals(v.Path, nameof(GlobalVariable.VariableType), StringComparison.OrdinalIgnoreCase)))
					{
						using var logScope = logger.BeginScope("Validate global {id}", global.Id);
						entity = global;

						isValid = ValidateGlobalVariableAndFile(filePath, global);

						return true;
					}
				}
			}

			return false;
		}

		private bool ValidateGlobalVariableAndFile(string filePath, GlobalVariable global)
		{
			var validationMessages = new StringBuilder();
			var fileInfo = new FileInfo(filePath);

			//Validate the global entity
			(bool isValid, string validationResult) = global.ValidateGlobal();
			if (!isValid)
			{
				validationMessages.AppendLine($"Validation failed for global '{global.Id}': {validationResult}");
			}

			//Validate the global file name matches the rule Id
			var matchRuleIdFileName = string.Equals(global.Id, Path.GetFileNameWithoutExtension(fileInfo.Name), StringComparison.CurrentCultureIgnoreCase);
			if (!matchRuleIdFileName)
			{
				validationMessages.AppendLine($"The global variable Id '{global.Id}' differs from the file name '{fileInfo.Name}'.");
			}

			//Validate the Id naming standard
			if (!global.Id.IsToIdStandard())
			{
				validationMessages.AppendLine($"The global Id '{global.Id}' does not adhere to id naming standards. Expected: '{global.Id.ToIdStandard()}'.");
			}

			if (validationMessages.Length > 0)
			{
				logger.LogWarning(validationMessages.ToString());
			}

			return isValid;
		}

		public IAsyncEnumerable<IId> GetEntities()
		{
			return this.repositoryGlobalVariable.GetAll();
		}

		public byte[] Serialize(IId entity)
		{
			string json = JsonConvert.SerializeObject(entity, jsonSettings);
			return Encoding.UTF8.GetBytes(json);
		}

		public async Task<IId?> GetEntity(string id)
		{
			var global = await repositoryGlobalVariable.GetOne(id, updateCache: false);
			return global;
		}

		public string GetExtension()
		{
			return ".json";
		}
	}

	public class MLModelSource : IEntitySource
	{
		private readonly IRepositoryMLModel repositoryMLModel;
		private readonly IMLService mlService;
		private readonly ILogger logger;

		public MLModelSource(IRepositoryMLModel repositoryMLModel, IMLService mlService, ILogger logger)
		{
			this.repositoryMLModel = repositoryMLModel ?? throw new ArgumentNullException(nameof(repositoryMLModel));
			this.mlService = mlService ?? throw new ArgumentNullException(nameof(mlService));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public FileServiceSourceType SourceType => FileServiceSourceType.MLModel;

		public const string Folder = "ml_models";

		public async Task<bool> Delete(IId entity)
		{
			var model = repositoryMLModel.GetModelWithoutBinary(entity.Id);

			if (model is not null)
			{
				await repositoryMLModel.DeleteOne(model);
				return true;
			}

			return false;
		}

		public string GetFolder(IId entity)
		{
			return Folder;
		}

		public async Task SaveResult(ProcessResult result, string user, bool overwrite = true)
		{
			if (result.MLModels.Count() > 0)
			{
				IList<MLModel> toAdd = overwrite ? result.MLModels.ToList() : new List<MLModel>();

				if (!overwrite)
				{
					foreach (var model in result.MLModels)
					{
						var previous = repositoryMLModel.GetModel(model.Id);
						if (previous == null)
						{
							toAdd.Add(model);
						}
					}
				}

				if (toAdd.Count > 0)
				{
					logger.LogInformation("Uploading {count} ml models", toAdd.Count);

					await repositoryMLModel.BulkMerge(toAdd);

					result.SaveCount += toAdd.Count;
				}
			}

			if (result.DeletedMLModels.Count() > 0)
			{
				logger.LogInformation("Deleting {count} ml models", result.DeletedMLModels.Count());

				await repositoryMLModel.BulkDelete(result.DeletedMLModels.ToList());

				result.SaveCount += result.DeletedMLModels.Count();
			}
		}

		public bool TryDeserialize(string filePath, MemoryStream stream, out IId? entity, out bool isValid)
		{
			entity = null;
			isValid = true;

			if (Path.GetExtension(filePath) == ".onnx")
			{
				var id = Path.GetFileNameWithoutExtension(filePath);

				var model = new MLModel()
				{
					Id = id,
					FullName = id,
					ModelName = id,
					ModelData = stream.ToArray()
				};

				using var logScope = logger.BeginScope("Validate ml model {id}", model.Id);

				(isValid, string validation) = mlService.ValidateModel(model);

				if (!isValid)
				{
					logger.LogWarning("Validation failed for ml model '{id}': {message}", model.Id, validation);
				}
				else
				{
					model = mlService.FillModel(model);
				}

				entity = model;

				return true;
			}

			return false;
		}

		public IAsyncEnumerable<IId> GetEntities()
		{
			return this.repositoryMLModel.GetAllModels().ToAsyncEnumerable();
		}

		public byte[] Serialize(IId entity)
		{
			var model = (MLModel)entity;
			return model.ModelData;
		}

		public Task<IId?> GetEntity(string id)
		{
			var model = (IId?)repositoryMLModel.GetModel(id);
			return Task.FromResult(model);
		}

		public string GetExtension()
		{
			return ".onnx";
		}
	}


	/// <summary>
	/// Result object used for feedback of service processing
	/// </summary>
	public class ProcessResult
	{
#nullable disable

		/// <summary>
		/// Name of the file(s) uploaded
		/// </summary>
		public IList<string> FileNames { get; set; } = new List<string>();

		/// <summary>
		/// Name of the file(s) uploaded
		/// </summary>
		public IList<string> DeletedFileNames { get; set; } = new List<string>();

		/// <summary>
		/// List of failed file names
		/// </summary>
		public IList<string> Failures { get; set; } = new List<string>();

		/// <summary>
		/// List of duplicate file names
		/// </summary>
		public IList<string> Duplicates { get; set; } = new List<string>();

		/// <summary>
		/// Number of rules processed
		/// </summary>
		public int ProcessedCount { get; set; }

		// <summary>
		/// Number of unique rules
		/// </summary>
		public int UniqueCount { get; set; }

		/// <summary>
		/// Number of duplicates
		/// </summary>
		public int DuplicateCount { get => Duplicates.Count; }

		/// <summary>
		/// Number of failures
		/// </summary>
		public int FailureCount { get => Failures.Count; }

		/// <summary>
		/// Overall status of process
		/// </summary>
		public bool Success { get => FailureCount == 0; }

		/// <summary>
		/// Number of deletes
		/// </summary>
		public int DeleteCount { get; set; }

		/// <summary>
		/// Number of changes
		/// </summary>
		public int ChangeCount { get; set; }

		/// <summary>
		/// Number of saves to the DB
		/// </summary>
		public int SaveCount { get; set; }

		/// <summary>
		/// Rules changes
		/// </summary>
		[JsonIgnore]
		public IList<IId> Entities { get; set; } = new List<IId>();

		/// <summary>
		/// Rules deletes
		/// </summary>
		[JsonIgnore]
		public IList<IId> DeletedEntities { get; set; } = new List<IId>();

		/// <summary>
		/// Globals changes
		/// </summary>
		[JsonIgnore]
		public IEnumerable<GlobalVariable> Globals
		{
			get
			{
				return Entities.OfType<GlobalVariable>();
			}
		}
		/// <summary>
		/// MLModel changes
		/// </summary>
		[JsonIgnore]
		public IEnumerable<MLModel> MLModels
		{
			get
			{
				return Entities.OfType<MLModel>();
			}
		}

		/// <summary>
		/// Globals deletes
		/// </summary>
		[JsonIgnore]
		public IEnumerable<GlobalVariable> DeletedGlobals
		{
			get
			{
				return DeletedEntities.OfType<GlobalVariable>();
			}
		}

		/// <summary>
		/// MLModel deletes
		/// </summary>
		[JsonIgnore]
		public IEnumerable<MLModel> DeletedMLModels
		{
			get
			{
				return DeletedEntities.OfType<MLModel>();
			}
		}

		/// <summary>
		/// Rules changes
		/// </summary>
		[JsonIgnore]
		public IEnumerable<Rule> Rules
		{
			get
			{
				return Entities.OfType<Rule>();
			}
		}

		/// <summary>
		/// Rules deletes
		/// </summary>
		[JsonIgnore]
		public IEnumerable<Rule> DeletedRules
		{
			get
			{
				return DeletedEntities.OfType<Rule>();
			}
		}

		/// <summary>
		/// Were there any changes
		/// </summary>
		public bool HasChanges()
		{
			return UniqueCount > 0;
		}

		/// <summary>
		/// Summary of result
		/// </summary>
		public string Summary
		{
			get
			{
				string summary = "";

				if (ChangeCount == 1)
				{
					summary += $"Changed {FileNames[0]},";
				}
				else if (ChangeCount > 1)
				{
					summary += $"Changes: {FileNames.Count},";
				}

				if (DeleteCount == 1)
				{
					summary += $"Deleted {DeletedFileNames[0]},";
				}
				else if (DeleteCount > 1)
				{
					summary += $"Deleted: {DeletedFileNames.Count},";
				}

				if (Failures.Count > 0)
				{
					summary += $"Failures ({FailureCount}): {string.Join(",", Failures)},";
				}

				if (Duplicates.Count > 0)
				{
					summary += $"Duplicates ({DuplicateCount}): {string.Join(",", Duplicates)},";
				}

				return summary.TrimEnd(',');
			}
		}
	}
}
