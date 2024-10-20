using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Willow.Rules.Cache;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using Willow.Rules.Sources;
using WillowRules.DTO;
using WillowRules.Extensions;
using WillowRules.Migrations;
using WillowRules.RepositoryConfiguration;
using static Willow.Rules.Services.FileService;

namespace Willow.Rules.Services;

public enum FileServiceSourceType
{
	Rule,
	Global,
	MLModel
}

/// <summary>
/// Service for creating and serving ZIP files containing Rules or Markdown files for Insights
/// </summary>
public interface IFileService
{
	/// <summary>
	/// Zips all the rules and globals into a temp file and returns the file location
	/// </summary>
	Task<string> GetOrCreateZippedRules(params FileServiceSourceType[] sourceTypes);

	/// <summary>
	/// Deletes and entity from disk based on id
	/// </summary>
	Task<ProcessResult> DeleteIdFromDisk(string id, string directory, bool deleteEmptyDirectories = false);

	/// <summary>
	/// Writes entity to disk based on id
	/// </summary>
	Task<ProcessResult> WriteEntityToDisk(string id, string directory);

	/// <summary>
	/// Writes all rules and globals to disk
	/// </summary>
	Task<ProcessResult> WriteAllEntitiesToDisk(string directory);

	/// <summary>
	/// Puts all the insights in Markdown and returns the file location
	/// </summary>
	/// <returns></returns>
	Task<string> GetOrCreateInsights();

	/// <summary>
	/// Get a short-lived token
	/// </summary>
	/// <returns></returns>
	/// <remarks>
	/// This is NOT a great implementation. It avoids issues with machine keys and
	/// symmetric encryption but it will not work on a multi-instance web site
	/// unless sticky-sessions are in use. TODO: Better solution here.
	/// </remarks>
	string GetShortLivedToken();

	/// <summary>
	/// Validate a short-lived token
	/// </summary>
	/// <remarks>
	/// This is NOT a great implementation. It avoids issues with machine keys and
	/// symmetric encryption but it will not work on a multi-instance web site
	/// unless stick-sessions are in use. TODO: Better solution here.
	/// </remarks>
	bool ValidateShortLivedToken(string token);

	/// <summary>
	/// Uploads/Deletes Rules and/or Globals from files in json format.
	/// </summary>
	/// <remarks>
	/// It currently caters for zip files with multiple rules or text based file for a single rule in json format.
	/// </remarks>
	/// <param name="filePath">The path to the file that contains the rules</param>
	/// <returns></returns>
	Task<ProcessResult> UploadEntities(IEnumerable<string> files, IEnumerable<string> deleteFiles, string user, bool save = true, bool overwrite = true, params FileServiceSourceType[] sourceTypes);

	/// <summary>
	/// Zips a rule instance json and related data
	/// </summary>
	Task<string> ZipRuleInstanceDebugInfo(string ruleInstanceId, bool downloadTelemetry, DateTime start, DateTime end);

	/// <summary>
	/// Zips all rule instance json and related data
	/// </summary>
	Task<string> ZipRuleInstances(IAsyncEnumerable<RuleInstance> ruleInstances);

	/// <summary>
	/// Uploads the related data for a rule instance analysis
	/// </summary>
	Task UploadRuleInstanceDebugInfo(string filepath);
}

/// <summary>
/// File service extnesions
/// </summary>
public static class FileServiceExtensions
{
	/// <summary>
	/// Get All files from disk including subdirectories
	/// </summary>
	internal static IEnumerable<string> GetAllFilesFromDirectory(string directory)
	{
		var files = Directory.EnumerateFiles(directory);

		var subFolderFiles = Directory.EnumerateDirectories(directory, "*", SearchOption.AllDirectories)
			.SelectMany(v => Directory.EnumerateFiles(v));

		files = files.Concat(subFolderFiles);

		return files;
	}

	/// <summary>
	/// Uploads from a directory
	/// </summary>
	public static Task<ProcessResult> UploadRulesFromDirectory(this IFileService fileService, string directory, string user, bool save = true, bool overwrite = true, params FileServiceSourceType[] sourceTypes)
	{
		var files = GetAllFilesFromDirectory(directory);

		return fileService.UploadRules(files, user, save, overwrite, sourceTypes);
	}

	/// <summary>
	/// Upload rules from a set of files
	/// </summary>
	public static Task<ProcessResult> UploadRules(this IFileService fileService, IEnumerable<string> files, string user, bool save = true, bool overwrite = true, params FileServiceSourceType[] sourceTypes)
	{
		return fileService.UploadEntities(files, Array.Empty<string>(), user, save, overwrite, sourceTypes);
	}

	/// <summary>
	/// Upload rules from a single file
	/// </summary>
	public static Task<ProcessResult> UploadRules(this IFileService fileService, string filePath, string user, bool save = true, bool overwrite = true, params FileServiceSourceType[] sourceTypes)
	{
		return fileService.UploadRules(new string[] { filePath }, user, save, overwrite, sourceTypes);
	}
}

/// <summary>
/// Creates and serves ZIP files
/// </summary>
public partial class FileService : IFileService
{
	private const string zip = ".zip";
	private static string[] ruleFileExtensions = new[] { ".txt", ".json" };
	private readonly IMemoryCache memoryCache;
	private readonly IRepositoryRules repositoryRules;
	private readonly IRepositoryInsight repositoryInsights;
	private readonly IRepositoryRuleInstances repositoryRuleInstances;
	private readonly IRepositoryCalculatedPoint repositoryCalculatedPoint;
	private readonly IRepositoryActorState repositoryActorState;
	private readonly IRepositoryTimeSeriesMapping repositoryTimeSeriesMapping;
	private readonly IRepositoryTimeSeriesBuffer repositoryTimeSeriesBuffer;
	private readonly IRepositoryCommand repositoryCommand;
	private readonly ITwinService twinService;
	private readonly ITwinSystemService twinSystemService;
	private readonly IDataCacheFactory dataCacheFactory;
	private readonly IADXService adxService;
	private readonly WillowEnvironment willowEnvironment;
	private readonly ILogger<FileService> logger;
	private readonly IEntitySource[] sources;

	/// <summary>
	/// Creates a new <see cref="FileService"/> for serving ZIP files with rules
	/// </summary>
	public FileService(
		IMemoryCache memoryCache,
		IRepositoryRules repositoryRules,
		IRepositoryInsight repositoryInsights,
		IRepositoryRuleMetadata repositoryRuleMetadata,
		IRepositoryRuleInstances repositoryRuleInstances,
		IRepositoryActorState repositoryActorState,
		IRepositoryTimeSeriesMapping repositoryTimeSeriesMapping,
		IRepositoryTimeSeriesBuffer repositoryTimeSeriesBuffer,
		IRepositoryCalculatedPoint repositoryCalculatedPoint,
		IRepositoryGlobalVariable repositoryGlobalVariable,
		IRepositoryCommand repositoryCommand,
		IRepositoryMLModel repositoryMLModel,
		IMLService mlService,
		IDataCacheFactory dataCacheFactory,
		ITwinSystemService twinSystemService,
		ITwinService twinService,
		IADXService adxService,
		WillowEnvironment willowEnvironment,
		ILogger<FileService> logger)
	{
		this.memoryCache = memoryCache;
		this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
		this.repositoryInsights = repositoryInsights ?? throw new ArgumentNullException(nameof(repositoryInsights));
		this.repositoryRuleInstances = repositoryRuleInstances ?? throw new ArgumentNullException(nameof(repositoryRuleInstances));
		this.repositoryCalculatedPoint = repositoryCalculatedPoint ?? throw new ArgumentNullException(nameof(repositoryCalculatedPoint));
		this.repositoryActorState = repositoryActorState ?? throw new ArgumentNullException(nameof(repositoryActorState));
		this.repositoryTimeSeriesMapping = repositoryTimeSeriesMapping ?? throw new ArgumentNullException(nameof(repositoryTimeSeriesMapping));
		this.repositoryTimeSeriesBuffer = repositoryTimeSeriesBuffer ?? throw new ArgumentNullException(nameof(repositoryTimeSeriesBuffer));
		this.repositoryCommand = repositoryCommand ?? throw new ArgumentNullException(nameof(repositoryCommand));
		this.dataCacheFactory = dataCacheFactory ?? throw new ArgumentNullException(nameof(dataCacheFactory));
		this.twinSystemService = twinSystemService ?? throw new ArgumentNullException(nameof(twinSystemService));
		this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.adxService = adxService ?? throw new ArgumentNullException(nameof(adxService));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.sources = new IEntitySource[]
		{
			new RuleSource(repositoryRules, repositoryRuleMetadata, logger),
			new GlobalSource(repositoryGlobalVariable, logger),
			new MLModelSource(repositoryMLModel, mlService, logger)
		};
	}

	private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
	{
		Converters = ConfigurationExtensions.JsonSettings.Converters,
		Formatting = Formatting.Indented,
		NullValueHandling = NullValueHandling.Ignore,
		TypeNameHandling = TypeNameHandling.Auto
	};

	private static readonly JsonSerializerSettings timeSeriesJsonSettings = new JsonSerializerSettings
	{
		Converters = new List<JsonConverter>() { new TimedValueJsonConverter() },
		Formatting = Formatting.Indented,
		NullValueHandling = NullValueHandling.Ignore,
		TypeNameHandling = TypeNameHandling.Auto
	};


	/// <summary>
	/// Uploads/Deletes Rules and/or Globals from files in json format.
	/// </summary>
	/// <remarks>
	/// It currently caters for zip files with multiple rules or text based file for a single rule in json format.
	/// </remarks>
	/// <param name="filePath">The path to the file that contains the rules</param>
	/// <returns></returns>
	public async Task<ProcessResult> UploadEntities(IEnumerable<string> files, IEnumerable<string> deleteFiles, string user, bool save = true, bool overwrite = true, params FileServiceSourceType[] sourceTypes)
	{
		var result = new ProcessResult();

		//upserts
		foreach (var filePath in files)
		{
			if (Path.GetExtension(filePath) == zip)
			{
				ReadFromZip(filePath, result, sourceTypes);
			}
			else
			{
				ReadFromFile(filePath, result, sourceTypes);
			}
		}

		//deletes
		foreach (var filePath in deleteFiles)
		{
			var id = Path.GetFileNameWithoutExtension(filePath);
			var fileInfo = new FileInfo(filePath);
			var currentLocation = fileInfo.Directory?.Name;

			foreach (var source in sources)
			{
				var entity = await source.GetEntity(id);
				if (entity is not null)
				{
					result.DeletedFileNames.Add(Path.GetFileName(filePath));

					//If the file deleted was in the incorrect location we do not want to delete the entity.
					if (entity is Rule rule)
					{
						var matchFolder = string.Equals(currentLocation, rule.PrimaryModelId.TrimModelId(), StringComparison.CurrentCultureIgnoreCase);
						if (!matchFolder)
						{
							//At this point the file has already been removed from the file system by the GitService Pull Command
							logger.LogWarning("File {ruleId} deleted from incorrect location {location}", id, currentLocation);
							continue;
						}
					}

					result.DeletedEntities.Add(entity);
					if (save)
					{
						bool deleted = await source.Delete(entity);

						if (deleted)
						{
							logger.LogInformation("Deleting {type} id {id} from file {file}", source.SourceType, id, filePath);
							break;
						}
					}
				}
			}
		}

		if (save)
		{
			foreach (var source in GetSources(sourceTypes))
			{
				await source.SaveResult(result, user, overwrite);
			}
		}

		result.UniqueCount = result.Entities.Count + result.DeletedEntities.Count;
		result.DeleteCount = result.DeletedEntities.Count;
		result.ChangeCount = result.Entities.Count;
		result.ProcessedCount = result.DuplicateCount + result.FailureCount + result.Entities.Count;

		return result;
	}



	/// <summary>
	/// Zips all the rules and globals into a temp file and returns the file location
	/// </summary>
	public async Task<string> GetOrCreateZippedRules(params FileServiceSourceType[] sourceTypes)
	{
		string tmpDirectory = Path.GetTempPath();
		string randomName = Path.ChangeExtension(Path.GetRandomFileName(), "zip");
		string filePath = Path.Combine(tmpDirectory, randomName);

		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}

		using (var zipFile = ZipFile.Open(filePath, ZipArchiveMode.Create))
		{
			foreach (var source in GetSources(sourceTypes))
			{
				await foreach (var entity in source.GetEntities())
				{
					string zipFolderName = source.GetFolder(entity);
					string zipFileName = Path.Combine(zipFolderName, GetEntityFileName(source, entity.Id));

					var zipEntry = zipFile.CreateEntry(zipFileName, CompressionLevel.Optimal);
					using (var stream = zipEntry.Open())
					{
						var bytes = source.Serialize(entity);
						await stream.WriteAsync(bytes);
					}
				}
			}
		}

		return filePath;
	}

	/// <summary>
	/// Get a short-lived token
	/// </summary>
	/// <returns></returns>
	/// <remarks>
	/// This is NOT a great implementation. It avoids issues with machine keys and
	/// symmetric encryption but it will not work on a multi-instance web site
	/// unless stick-sessions are in use. TODO: Better solution here.
	/// </remarks>
	public string GetShortLivedToken()
	{
		string randomToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));
		_ = memoryCache.GetOrCreate<string>("token" + randomToken, (c) =>
		{
			c.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
			return "OK";
		});
		return randomToken;
	}

	/// <summary>
	/// Validate a short-lived token
	/// </summary>
	/// <remarks>
	/// This is NOT a great implementation. It avoids issues with machine keys and
	/// symmetric encryption but it will not work on a multi-instance web site
	/// unless sticky-sessions are in use. TODO: Better solution here.
	/// </remarks>
	public bool ValidateShortLivedToken(string token)
	{
		return memoryCache.TryGetValue("token" + token, out _);
	}

	public async Task<string> GetOrCreateInsights()
	{
		string tmpDirectory = Path.GetTempPath();
		string randomName = Path.ChangeExtension(Path.GetRandomFileName(), "zip");
		string filePath = Path.Combine(tmpDirectory, randomName);

		if (System.IO.File.Exists(filePath))
		{
			System.IO.File.Delete(filePath);
		}

		using (var zipFile = ZipFile.Open(filePath, ZipArchiveMode.Create))
		{
			await foreach (var insight in this.repositoryInsights.GetAll())
			{
				var zipEntry = zipFile.CreateEntry(insight.Id + ".md", CompressionLevel.Optimal);
				using (var stream = zipEntry.Open())
				{
					StringBuilder md = new StringBuilder();
					md.AppendLine("# " + insight.Id);
					md.AppendLine(insight.Text);

					md.AppendLine();
					md.AppendLine("# Rule");
					md.AppendLine($"Name: {insight.RuleName}");
					md.AppendLine();
					md.AppendLine($"Invocations: {insight.Invocations}");

					md.AppendLine();
					md.AppendLine("# Recommendations");
					md.AppendLine(insight.RuleRecomendations);

					md.AppendLine();
					md.AppendLine("# Equipment");
					md.AppendLine($"Id: {insight.EquipmentId}");
					md.AppendLine();
					md.AppendLine($"Unique Id: {insight.EquipmentUniqueId}");

					md.AppendLine();
					md.AppendLine("# Occurrences");
					md.AppendLine("|Started|Ended|Text|");
					md.AppendLine("|---|---|---|");
					foreach (var x in insight.Occurrences)
					{
						md.AppendLine($"|{x.Started.ToUniversalTime().DateTime}|{x.Ended.ToUniversalTime().DateTime}|{x.Text}|");
					}

					md.AppendLine();
					md.AppendLine("# Scores");
					md.AppendLine("|Comfort|Cost|Reliability|");
					md.AppendLine("|---|---|---|");
					md.AppendLine($"|{string.Join("|", insight.ImpactScores.Select(v => $"{v.Name}:{v.Score:0.0}"))}|");

					md.AppendLine();
					md.AppendLine(insight.CommandEnabled ? "Synchronized with command" : "Not yet synchronized with command");

					var bytes = Encoding.UTF8.GetBytes(md.ToString());
					await stream.WriteAsync(bytes);
				}
			}
		}
		return filePath;
	}

	private static string GetEntityFileName(IEntitySource source, IId entity)
	{
		return GetEntityFileName(source, entity.Id);
	}

	private static string GetEntityFileName(IEntitySource source, string id)
	{
		return id + source.GetExtension();
	}

	private static bool TryReadFromByteArray<T>(byte[] bytes, out T? result, JsonSerializerSettings? settings = null)
	{
		using (var stream = new MemoryStream(bytes))
		{
			return TryReadFromStream<T>(stream, out result, settings: settings);
		}
	}

	private static bool TryReadFromStream<T>(Stream stream, out T? result, JsonSerializerSettings? settings = null, bool leaveOpen = false)
	{
		result = default(T);

		using (var reader = new StreamReader(stream, leaveOpen: leaveOpen))
		using (var jsonReader = new JsonTextReader(reader))
		{
			try
			{
				var serializer = JsonSerializer.Create(settings ?? jsonSettings);
				result = serializer.Deserialize<T>(jsonReader)!;
			}
			catch (Exception)
			{
				//Do not throw and process what is possible
				return false;
			}

		}

		return result is not null;
	}

	private void ReadFromZip(string filePath, ProcessResult result, IEnumerable<FileServiceSourceType> sourceTypes)
	{
		using var logScope = logger.BeginScope(new Dictionary<string, object> { ["filePath"] = filePath });

		try
		{
			using (var zipFile = ZipFile.OpenRead(filePath))
			{
				foreach (var entry in zipFile.Entries)
				{
					if (entry.Length == 0)
					{
						//folders
						continue;
					}

					using var logScope2 = logger.BeginScope(new Dictionary<string, object> { ["zipEntry"] = entry.FullName });

					using (var stream = entry.Open())
					{
						ReadAndAddFromStream(stream, entry.FullName, result, sourceTypes);
					}
				}
			}
		}
		catch (Exception ex)
		{
			result.Failures.Add($"Failed to process zip file {filePath} for upload");
			logger.LogError(ex, $"Failed to open zip file for rules upload {filePath}. {ex.Message}");
		}
	}

	private void ReadFromFile(string filePath, ProcessResult result, IEnumerable<FileServiceSourceType> sourceTypes)
	{
		try
		{
			ReadAndAddFromFile(filePath, result, sourceTypes);
		}
		catch (Exception ex)
		{
			result.Failures.Add($"Failed to process {filePath} for upload");
			logger.LogError(ex, $"Failed to deserialize rule from file {filePath}. {ex.Message}");
		}
	}

	/// <summary>
	/// Helper method to read rule from specified file path. Adds the rule to the list of rules
	/// if it is not a duplicate and is valid. Updates the result.
	/// </summary>
	private void ReadAndAddFromFile(string filePath, ProcessResult result, IEnumerable<FileServiceSourceType> sourceTypes)
	{
		using var logScope = logger.BeginScope(new Dictionary<string, object> { ["filePath"] = filePath });

		using (var stream = File.OpenRead(filePath))
		{
			ReadAndAddFromStream(stream, filePath, result, sourceTypes);
		}
	}

	/// <summary>
	/// Deserializes an entity from a stream
	/// </summary>
	private void ReadAndAddFromStream(Stream stream, string filePath, ProcessResult result, IEnumerable<FileServiceSourceType> sourceTypes)
	{
		using (MemoryStream ms = new MemoryStream())
		{
			stream.CopyTo(ms);

			ms.Seek(0, SeekOrigin.Begin);

			foreach (var source in sources.Where(s => !sourceTypes.Any() || sourceTypes.Contains(s.SourceType)))
			{
				try
				{
					if (source.TryDeserialize(filePath, ms, out var entity, out var isValid))
					{
						var fileName = Path.GetFileName(filePath);

						if (result.Entities.Any(r => r.Id == entity!.Id))
						{
							result.Duplicates.Add(fileName);
						}
						else
						{
							if (!isValid)
							{
								result.Failures.Add(fileName);
							}
							else
							{
								result.FileNames.Add(fileName);
								result.Entities.Add(entity);
							}
						}

						return;
					}
				}
				catch (JsonReaderException)
				{
					// Have seen many of these on single-tenant Prod

				}
				catch (Exception ex)
				{
					logger.LogWarning(ex, $"Failed to deserialize to {source}. Trying next source.", source.GetType().Name);
				}

				ms.Seek(0, SeekOrigin.Begin);
			}
		}

		result.Failures.Add(Path.GetFileName(filePath));
	}

	/// <summary>
	/// Strip illegal chars and reserved words from a candidate filename (should not include the directory path)
	/// </summary>
	/// <remarks>
	/// http://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name
	/// </remarks>
	public static string CoerceValidFileName(string filename)
	{
		var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
		var invalidReStr = string.Format(@"[{0}]+", invalidChars);

		var reservedWords = new[]
		{
		"CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
		"COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
		"LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
	};

		var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");
		foreach (var reservedWord in reservedWords)
		{
			var reservedWordPattern = string.Format("^{0}\\.", reservedWord);
			sanitisedNamePart = Regex.Replace(sanitisedNamePart, reservedWordPattern, "_reservedWord_.", RegexOptions.IgnoreCase);
		}

		return sanitisedNamePart;
	}

	/// <summary>
	/// Strip illegal chars and reserved words from a candidate filename (should not include the directory path)
	/// </summary>
	/// <remarks>
	/// http://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name
	/// </remarks>
	public static string CoerceValidDirectoryName(string filename)
	{
		var invalidChars = Regex.Escape(new string(Path.GetInvalidPathChars()));
		var invalidReStr = string.Format(@"[{0}]+", invalidChars);
		var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");
		return sanitisedNamePart;
	}

	public Task<ProcessResult> DeleteIdFromDisk(string id, string directory, bool deleteEmptyDirectory = false)
	{
		var result = new ProcessResult();
		var files = FileServiceExtensions.GetAllFilesFromDirectory(directory);

		var fileName = $"{id}.json";

		var fileDeletes = files
							.Where(v => string.Equals(Path.GetFileName(v), fileName, StringComparison.OrdinalIgnoreCase))
							.ToList();

		foreach (var file in fileDeletes)
		{
			try
			{
				File.Delete(file);

				result.DeletedFileNames.Add(file);
				result.DeleteCount++;
				result.ProcessedCount++;

				if (deleteEmptyDirectory)
				{
					var parentDirectory = Path.GetDirectoryName(file);

					if (!Directory.EnumerateFileSystemEntries(parentDirectory!).Any())
					{
						Directory.Delete(parentDirectory!);
					}
				}
			}
			catch (Exception e)
			{
				result.Failures.Add(file);

				logger.LogError(e, "Failed to delete file from disk {path}", file);
			}
		}

		if (deleteEmptyDirectory)
		{
			if (!Directory.EnumerateFileSystemEntries(directory).Any())
			{
				Directory.Delete(directory);
			}
		}

		if (result.DeleteCount > 1)
		{
			logger.LogWarning("Multiple files {count} deleted for file name {fileName}. {files}", result.DeleteCount, fileName, string.Join(",", result.FileNames));
		}

		return Task.FromResult(result);
	}

	public async Task<ProcessResult> WriteEntityToDisk(string id, string directory)
	{
		var result = new ProcessResult();

		foreach (var source in sources)
		{
			var entity = await source.GetEntity(id);

			if (entity is not null)
			{
				string folder = source.GetFolder(entity);

				await WriteEntityToDisk(source, entity, directory, result, folder);

				break;
			}
		}

		return result;
	}

	public async Task<ProcessResult> WriteAllEntitiesToDisk(string directory)
	{
		var result = new ProcessResult();

		foreach (var source in sources)
		{
			await foreach (var entity in source.GetEntities())
			{
				string folder = source.GetFolder(entity);
				await WriteEntityToDisk(source, entity, directory, result, folder);
			}
		}

		return result;
	}

	public async Task<string> ZipRuleInstances(IAsyncEnumerable<RuleInstance> ruleInstances)
	{
		string tmpDirectory = Path.GetTempPath();
		string randomName = Path.ChangeExtension(Path.GetRandomFileName(), "zip");
		string filePath = Path.Combine(tmpDirectory, randomName);

		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}

		var addEntry = async (ZipArchive zipFile, string directorName, string fileName, object entry) =>
		{
			if (!string.IsNullOrEmpty(directorName))
			{
				directorName = CoerceValidDirectoryName(directorName);
				fileName = Path.Combine(directorName, fileName);
			}

			var zipEntry = zipFile.CreateEntry(fileName, CompressionLevel.Optimal);
			using (var stream = zipEntry.Open())
			{
				string json = JsonConvert.SerializeObject(entry, entry is TimeSeries ? timeSeriesJsonSettings : jsonSettings);
				var bytes = Encoding.UTF8.GetBytes(json);
				await stream.WriteAsync(bytes);
			}
		};

		using (var zipFile = ZipFile.Open(filePath, ZipArchiveMode.Create))
		{
			await foreach (var ruleInstance in ruleInstances)
			{
				await addEntry(zipFile, "", $"{ruleInstance.Id}.json", ruleInstance);
			}
		}

		return filePath;
	}

	public async Task<string> ZipRuleInstanceDebugInfo(string ruleInstanceId, bool downloadTelemetry, DateTime start, DateTime end)
	{
		var ruleInstance = await repositoryRuleInstances.GetOne(ruleInstanceId, updateCache: false);

		if (ruleInstance is null)
		{
			throw new ArgumentNullException(nameof(ruleInstanceId), ruleInstanceId);
		}

		CalculatedPoint? calculatedPoint = null;

		if (ruleInstance.RuleTemplate == RuleTemplateCalculatedPoint.ID)
		{
			calculatedPoint = await repositoryCalculatedPoint.GetOne(ruleInstanceId, updateCache: false);
		}

		var rule = await repositoryRules.GetOne(ruleInstance.RuleId, updateCache: false);
		var actor = await repositoryActorState.GetOne(ruleInstanceId, updateCache: false);
		var insight = await repositoryInsights.GetOne(ruleInstanceId, updateCache: false);
		var commands = await repositoryCommand.Get(v => v.RuleInstanceId == ruleInstanceId);
		var timeseries = new List<TimeSeries>();
		var mappings = new List<TimeSeriesMapping>();
		var twins = new List<BasicDigitalTwinPoco>();
		var twinGraphs = new List<(string twinId, SerializableGraph<BasicDigitalTwinPoco, WillowRelation> graph)>();
		var backEdges = new List<(string twinId, CollectionWrapper<Edge> edges)>();
		var forwardEdges = new List<(string twinId, CollectionWrapper<Edge> edges)>();
		var rawData = new List<RawData>();

		if (!string.IsNullOrEmpty(ruleInstance.EquipmentId))
		{
			var equipment = await twinService.GetCachedTwin(ruleInstance.EquipmentId);

			if (equipment is not null)
			{
				twins.Add(equipment);
			}
		}

		if (!string.IsNullOrEmpty(calculatedPoint?.Id))
		{
			var point = await twinService.GetCachedTwin(calculatedPoint.Id);

			if (point is not null)
			{
				twins.Add(point);
			}
		}

		foreach (var point in ruleInstance.PointEntityIds)
		{
			var twin = await twinService.GetCachedTwin(point.Id);

			if (twin is null)
			{
				continue;
			}

			twins.Add(twin);
		}

		var filters = new List<IdFilter>();

		foreach (var twin in twins)
		{
			var twinBackEdges = await twinService.GetCachedBackwardRelatedTwins(twin.Id);

			if (twinBackEdges is not null)
			{
				backEdges.Add((twin.Id, new CollectionWrapper<Edge>(twinBackEdges.ToList())));
			}

			var twinForwardEdges = await twinService.GetCachedForwardRelatedTwins(twin.Id);

			if (twinForwardEdges is not null)
			{
				forwardEdges.Add((twin.Id, new CollectionWrapper<Edge>(twinForwardEdges.ToList())));
			}

			var twinGraph = await twinSystemService.GetTwinSystemGraph(new string[] { twin.Id });

			if (twinGraph is not null)
			{
				var ser = SerializableGraph<BasicDigitalTwinPoco, WillowRelation>.FromGraph(twinGraph, n => n.Id);
				twinGraphs.Add((twin.Id, ser));
			}

			var mapping = await repositoryTimeSeriesMapping.GetOne(twin.Id, updateCache: false);

			if (mapping is not null)
			{
				mappings.Add(mapping);

				filters.Add(new IdFilter(twin.trendID, twin.externalID, twin.connectorID));
			}

			string timeSeriesId = !string.IsNullOrEmpty(twin.trendID) ? twin.trendID : $"{twin.externalID}_{twin.connectorID}";

			var ts = await repositoryTimeSeriesBuffer.GetOne(timeSeriesId, updateCache: false);

			if (ts is null)
			{
				ts = await repositoryTimeSeriesBuffer.GetOne($"{twin.externalID}_{twin.connectorID}", updateCache: false);
			}

			if (ts is not null)
			{
				timeseries.Add(ts);
			}
		}

		if (filters.Count > 0 && downloadTelemetry)
		{
			//limit download not to overburden ADX
			if (filters.Count > 50)
			{
				start = end.AddHours(-24);
			}

			await foreach (var line in adxService.RunRawQuery(start, end, filters))
			{
				rawData.Add(line);
			}
		}

		string tmpDirectory = Path.GetTempPath();
		string randomName = Path.ChangeExtension(Path.GetRandomFileName(), "zip");
		string filePath = Path.Combine(tmpDirectory, randomName);

		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}

		var addEntry = async (ZipArchive zipFile, string directorName, string fileName, object entry) =>
		{
			if (!string.IsNullOrEmpty(directorName))
			{
				directorName = CoerceValidDirectoryName(directorName);
				fileName = Path.Combine(directorName, fileName);
			}

			var zipEntry = zipFile.CreateEntry(fileName, CompressionLevel.Optimal);
			using (var stream = zipEntry.Open())
			{
				string json = JsonConvert.SerializeObject(entry, entry is TimeSeries ? timeSeriesJsonSettings : jsonSettings);
				var bytes = Encoding.UTF8.GetBytes(json);
				await stream.WriteAsync(bytes);
			}
		};

		using (var zipFile = ZipFile.Open(filePath, ZipArchiveMode.Create))
		{
			if (rule is not null)
			{
				await addEntry(zipFile, "", $"Rule.{rule.Id}.json", rule);
			}

			await addEntry(zipFile, "", $"RuleInstance.{ruleInstance!.Id}.json", ruleInstance!);

			if (calculatedPoint is not null)
			{
				await addEntry(zipFile, "", $"CalculatedPoint.{calculatedPoint!.Id}.json", calculatedPoint!);
			}

			if (actor is not null)
			{
				await addEntry(zipFile, "", $"Actor.{actor!.Id}.json", actor!);
			}

			if (insight is not null)
			{
				await addEntry(zipFile, "", $"Insight.{insight!.Id}.json", insight!);
			}

			foreach (var command in commands)
			{
				await addEntry(zipFile, "", $"Command.{command.Id}.json", command);
			}

			foreach (var item in twins)
			{
				await addEntry(zipFile, "", $"Twin.{item!.Id}.json", item!);
			}

			foreach (var item in twinGraphs)
			{
				await addEntry(zipFile, "", $"TwinGraph.{item!.twinId}.json", item.graph!);
			}

			foreach (var item in backEdges)
			{
				await addEntry(zipFile, "", $"BackEdges.{item!.twinId}.json", item.edges!);
			}

			foreach (var item in forwardEdges)
			{
				await addEntry(zipFile, "", $"ForwardEdges.{item!.twinId}.json", item.edges!);
			}

			foreach (var item in timeseries)
			{
				await addEntry(zipFile, "", $"TimeSeries.{item!.Id}.json", item!);
			}

			foreach (var item in mappings)
			{
				await addEntry(zipFile, "", $"TimeSeriesMapping.{item!.Id}.json", item!);
			}

			if (rawData.Count > 0)
			{
				var zipEntry = zipFile.CreateEntry("Telemetry.csv", CompressionLevel.Optimal);

				using (var stream = zipEntry.Open())
				{
					StringBuilder md = new StringBuilder();

					md.AppendLine("\"TrendId\",\"SourceTimestamp\",\"ScalarValue\",\"ConnectorId\",\"ExternalId\"");

					foreach (var line in rawData)
					{
						md.AppendLine($"\"{line.PointEntityId}\",\"{line.SourceTimestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")}\",\"{line.Value}\",\"{line.ConnectorId}\",\"{line.ExternalId}\"");
					}

					var bytes = Encoding.UTF8.GetBytes(md.ToString());

					await stream.WriteAsync(bytes);
				}

			}
		}

		return filePath;
	}

	public async Task UploadRuleInstanceDebugInfo(string filepath)
	{
		using (var zipFile = ZipFile.OpenRead(filepath))
		{
			foreach (var entry in zipFile.Entries)
			{
				using (var stream = entry.Open())
				{
					if (entry.FullName.StartsWith("Rule."))
					{
						if (TryReadFromStream<Rule>(stream, out var item))
						{
							await repositoryRules.UpsertOne(item!);
						}
					}
					else if (entry.FullName.StartsWith("RuleInstance."))
					{
						if (TryReadFromStream<RuleInstance>(stream, out var item))
						{
							await repositoryRuleInstances.UpsertOne(item!);
						}
					}
					else if (entry.FullName.StartsWith("CalculatedPoint."))
					{
						if (TryReadFromStream<CalculatedPoint>(stream, out var item))
						{
							await repositoryCalculatedPoint.UpsertOne(item!);
						}
					}
					else if (entry.FullName.StartsWith("Actor."))
					{
						if (TryReadFromStream<ActorState>(stream, out var item))
						{
							await repositoryActorState.UpsertOne(item!);
						}
					}
					else if (entry.FullName.StartsWith("Insight."))
					{
						if (TryReadFromStream<Insight>(stream, out var item))
						{
							await repositoryInsights.UpsertOne(item!);
						}
					}
					else if (entry.FullName.StartsWith("Command."))
					{
						if (TryReadFromStream<Command>(stream, out var item))
						{
							await repositoryCommand.UpsertOne(item!);
						}
					}
					else if (entry.FullName.StartsWith("TimeSeries."))
					{
						if (TryReadFromStream<TimeSeries>(stream, out var item, settings: timeSeriesJsonSettings))
						{
							await repositoryTimeSeriesBuffer.UpsertOne(item!);
						}
					}
					else if (entry.FullName.StartsWith("TimeSeriesMapping."))
					{
						if (TryReadFromStream<TimeSeriesMapping>(stream, out var item))
						{
							await repositoryTimeSeriesMapping.UpsertOne(item!);
						}
					}
					else if (entry.FullName.StartsWith("Twin."))
					{
						if (TryReadFromStream<BasicDigitalTwinPoco>(stream, out var item))
						{
							await dataCacheFactory.TwinCache.AddOrUpdate(willowEnvironment.Id, item!.Id, item);
						}
					}
					else if (entry.FullName.StartsWith("TwinGraph."))
					{
						if (TryReadFromStream<SerializableGraph<BasicDigitalTwinPoco, WillowRelation>>(stream, out var item))
						{
							var twinId = entry.FullName.Replace("TwinGraph.", "").Replace(".json", "");
							await dataCacheFactory.TwinSystemGraphCache.AddOrUpdate(willowEnvironment.Id, twinId, item);
						}
					}
					else if (entry.FullName.StartsWith("BackEdges."))
					{
						if (TryReadFromStream<CollectionWrapper<Edge>>(stream, out var item))
						{
							var twinId = entry.FullName.Replace("BackEdges.", "").Replace(".json", "");
							await dataCacheFactory.BackEdgeCache.AddOrUpdate(willowEnvironment.Id, twinId, item!);
						}
					}
					else if (entry.FullName.StartsWith("ForwardEdges."))
					{
						if (TryReadFromStream<CollectionWrapper<Edge>>(stream, out var item))
						{
							var twinId = entry.FullName.Replace("ForwardEdges.", "").Replace(".json", "");
							await dataCacheFactory.ForwardEdgeCache.AddOrUpdate(willowEnvironment.Id, twinId, item!);
						}
					}
				}
			}
		}
	}

	private async Task WriteEntityToDisk(IEntitySource source, IId entity, string directory, ProcessResult result, string folder)
	{
		string folderPath = Path.Combine(directory, folder);
		string fullPath = Path.Combine(directory, folder, GetEntityFileName(source, entity));

		try
		{
			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
			{
				var bytes = source.Serialize(entity);
				await stream.WriteAsync(bytes);
			}

			result.Entities.Add(entity);
			result.FileNames.Add(Path.GetFileName(fullPath));
			result.ProcessedCount++;
		}
		catch (Exception e)
		{
			result.Failures.Add(fullPath);

			logger.LogError(e, "Failed to write {id} to disk {path}", entity.Id, fullPath);
		}
	}

	private IEnumerable<IEntitySource> GetSources(params FileServiceSourceType[] sourceTypes)
	{
		if (sourceTypes is not null && sourceTypes.Length > 0)
		{
			return sources.Where(v => sourceTypes.Contains(v.SourceType));
		}

		return sources;
	}
}
