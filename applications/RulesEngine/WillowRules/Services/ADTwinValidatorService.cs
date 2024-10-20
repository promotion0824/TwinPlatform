using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Sources;

namespace Willow.Rules.Services;

/// <summary>
/// Not real, just a POC for some of the rules we could run against a Twin graph to validate it
/// </summary>
public interface IADTTwinValidatorService
{
	/// <summary>
	/// Sample code showing some validation rules on the Twin graph
	/// </summary>
	Task<int> SampleValidator(WillowEnvironment willowEnvironment);
}

/// <inheritdoc />
public class ADTwinValidatorService : IADTTwinValidatorService
{
	private readonly IModelService modelService;
	private readonly ITwinService twinService;

	public ADTwinValidatorService(IModelService modelService, ITwinService twinService)
	{
		this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
		this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
	}


	// Check for agreement between BACNETID, DTMI and Tags

	private Dictionary<string, string[]> mutualExclusions = new Dictionary<string, string[]>
	{
		["current"] = "temp".Split(' '),
		["sensor"] = "setpoint actuator".Split(' '),
		["sp"] = "sensor actuator".Split(' '),
		["setpoint"] = "sensor actuator".Split(' '),
		["cmd"] = "sensor setpoint".Split(' '),
		["actuator"] = "sensor setpoint".Split(' ')
	};

	// IF THIS           CANNOT BE THIS
	// =====================================
	// current           temp
	// sensor            setpoint | actuator
	// setpoint | sp     sensor | actuator
	// cmd               sensor | setpoint
	// actuator          sensor | setpoint
	// CTL               sensor

	// Some that go together
	// IF THIS           THEN SHOULD BE
	// DIFF              delta
	// DEVIATIONLIMITSP  parameter?
	// RESETACTIVE       cmd




	public class ErrorLine
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string ModelId { get; }
		public string Tags { get; set; }
		public string Error { get; set; }

		/// <summary>
		/// Creates a new <see cref="ErrorLine"/>
		/// </summary>
		public ErrorLine(string id, string name, string modelId, string tags, string error)
		{
			Id = id;
			Name = name;
			ModelId = modelId;
			Tags = tags;
			Error = error;
		}
	}

	/// <inheritdoc />
	public async Task<int> SampleValidator(WillowEnvironment willowEnvironment)
	{
		List<ErrorLine> errorLines = new();

		var twins = this.twinService.GetAllCached();
		await foreach (var tw in twins)
		{
			var model = await this.modelService.GetSingleModelAsync(tw.Metadata.ModelId);
			var modelName = model?.LanguageDisplayNames?.FirstOrDefault().Value ?? "????";
			var modelId = tw.Metadata.ModelId;

			// foreach (var capabilityDto in tw.capabilities.Values)
			// {
			// 	List<string> errors = new();

			// 	foreach (var invalid in mutualExclusions)
			// 	{
			// 		bool tagsContainsKey = capabilityDto.tags.Any(x => String.Equals(x, invalid.Key));
			// 		bool modelIdContainsKey = invalid.Key.Length > 2 && capabilityDto.modelId.Contains(invalid.Key);
			// 		bool nameContainsKey = invalid.Key.Length > 2 && capabilityDto.modelId.Contains(invalid.Key);

			// 		bool tagsContainsValue = invalid.Value.Any(v => capabilityDto.tags.Any(x => String.Equals(x, v)));
			// 		bool modelIdContainsValue = invalid.Value.Any(v => capabilityDto.modelId.Contains(v, StringComparison.OrdinalIgnoreCase));
			// 		bool nameContainsValue = invalid.Value.Any(v => capabilityDto.modelId.Contains(v, StringComparison.OrdinalIgnoreCase));

			// 		if (tagsContainsKey && tagsContainsValue)
			// 		{
			// 			errors.Add("tags");
			// 		}
			// 		if (tagsContainsKey && modelIdContainsValue)
			// 		{
			// 			errors.Add("tags and model id");
			// 		}
			// 		if (tagsContainsKey && nameContainsValue)
			// 		{
			// 			errors.Add("tags and name");
			// 		}

			// 		if (modelIdContainsKey && tagsContainsValue)
			// 		{
			// 			errors.Add("model id and tags");
			// 		}
			// 		if (modelIdContainsKey && modelIdContainsValue)
			// 		{
			// 			errors.Add("model id");
			// 		}
			// 		if (modelIdContainsKey && nameContainsValue)
			// 		{
			// 			errors.Add("model id and name");
			// 		}

			// 		if (nameContainsKey && tagsContainsValue)
			// 		{
			// 			errors.Add("name and tags");
			// 		}
			// 		if (nameContainsKey && modelIdContainsValue)
			// 		{
			// 			errors.Add("name and model id");
			// 		}
			// 		if (nameContainsKey && nameContainsValue)
			// 		{
			// 			errors.Add("name");
			// 		}
			// 	}

			// 	if (errors.Any())
			// 	{
			// 		Console.ForegroundColor = ConsoleColor.Red;
			// 		Console.WriteLine($"{c++} {string.Join(", ", errors)} for {capabilityDto.Id} - {capabilityDto.tagstring} - {capabilityDto.modelId}");
			// 		Console.ResetColor();

			// 		errorLines.Add(new ErrorLine(
			// 			capabilityDto.Id,
			// 			capabilityDto.name,
			// 			capabilityDto.modelId,
			// 			capabilityDto.tagstring,
			// 			string.Join(" | ", errors)
			// 		));
			// 	}

			// }
		}

		string mydocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		string path = Path.Combine(mydocuments, "building_errors.csv");

		CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			InjectionOptions = InjectionOptions.Escape
		};

		using (var writer = new StreamWriter(path))
		using (var csv = new CsvWriter(writer, config))
		{
			csv.WriteRecords(errorLines);
		}
		await Task.Delay(5);
		// Open the CSV file in excel
		var p = new Process();
		p.StartInfo = new ProcessStartInfo(path)
		{
			UseShellExecute = true
		};
		p.Start();

		return 0;
	}
}
