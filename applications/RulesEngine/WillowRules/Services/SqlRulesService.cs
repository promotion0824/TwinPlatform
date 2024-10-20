using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Willow.Rules.Model;

namespace Willow.Rules.Services;

/// <summary>
/// Mock Rules service that reads rules from CSV files
/// </summary>
public class SqlRulesService
{
	public IEnumerable<FaultRecord> Faults => faults.Value;
	private Lazy<IEnumerable<FaultRecord>> faults;

	public IEnumerable<ProcessRecord> Processes => processes.Value;
	private Lazy<IEnumerable<ProcessRecord>> processes;
	private readonly IModelService modelsService;
	private readonly ILogger<SqlRulesService> logger;

	public SqlRulesService(IModelService modelsService, ILogger<SqlRulesService> logger)
	{
		this.faults = new Lazy<IEnumerable<FaultRecord>>(ReadFaults);
		this.processes = new Lazy<IEnumerable<ProcessRecord>>(ReadProcesses);
		this.modelsService = modelsService ?? throw new ArgumentNullException(nameof(modelsService));
		this.logger = logger;
	}

	private IEnumerable<FaultRecord> ReadFaults()
	{
		var csv1 = new CsvReader(File.OpenText("FaultLibrary.csv"), CultureInfo.InvariantCulture);
		var faults = csv1.GetRecords<FaultRecord>().ToList();
		logger.LogInformation("Loaded {count} faults", faults.Count);
		return faults;
	}

	private IEnumerable<ProcessRecord> ReadProcesses()
	{
		var csv2 = new CsvReader(File.OpenText("ProcessLibrary.csv"), CultureInfo.InvariantCulture);
		var processes = csv2.GetRecords<ProcessRecord>().ToList();
		logger.LogInformation("Loaded {count} processes", processes.Count);
		return processes;
	}

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
	public async Task DumpRules()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	{
		logger.LogInformation("\n\nPROCESSES");
		foreach (var process in this.Processes)
		{
			logger.LogInformation($"\nClass: {process.Class}\nProcess: {process.Process}\nSyntax: {process.Syntax}");
		}

		logger.LogInformation("\n\nFAULTS");
		foreach (var fault in this.Faults)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"\nClass: {fault.Class}\nPossibility: {fault.PossibilitySyntax}\nRule syntax: {fault.RuleSyntax}");

			bool expands = this.Processes.Any(p =>
				fault.PossibilitySyntax.Contains(p.Process, StringComparison.OrdinalIgnoreCase) ||
				fault.RuleSyntax.Contains(p.Process, StringComparison.OrdinalIgnoreCase));

			if (expands)
			{
				foreach (var process in this.Processes)
				{
					//if (!fault.Class.Equals(process.Class)) continue;

					if (fault.PossibilitySyntax.Contains(process.Process, StringComparison.OrdinalIgnoreCase))
					{
						sb.AppendLine($"   {process.Class} {process.ClassProcess} {process.Process} => {process.Syntax}");
					}
					if (fault.RuleSyntax.Contains(process.Process, StringComparison.OrdinalIgnoreCase))
					{
						sb.AppendLine($"    {process.Class} {process.ClassProcess} {process.Process} => {process.Syntax}");
					}
				}
				logger.LogInformation(sb.ToString());
			}
			else
			{
				//logger.LogInformation($"Rule: {sb}");
			}
		}

		logger.LogInformation("\n\nEND FAULTS\n\n");

	}

	internal class RuleComparison
	{
		public RuleComparison(string buildingTag, CanonicalTagSet ct, string ruleTag, string dtid)
		{
			BuildingTag = buildingTag;
			BuildingCount = ct.Count;
			RuleTag = ruleTag;
			this.dtid = dtid;
			this.Substance = ct.Substance;
			this.DescribesSubstance = ct.DescribesSubstance;
			this.Position = ct.Position;
			this.DescribesEquipment = ct.DescribesEquipment;
			this.Equipment = ct.Equipment;
			this.DescribesMeasure = ct.DescribesMeasure;
			this.Measure = ct.Measure;
			this.Units = ct.Units;
			this.Value = ct.Value;
			this.DescribesCapability = ct.DescribesCapability;
			this.CapabilityType = ct.CapabilityType;
			this.Qualifier = ct.Qualifier;
		}

		public string BuildingTag { get; set; }
		public int BuildingCount { get; }
		public string RuleTag { get; set; }

		public string Substance { get; init; } = "";
		public string DescribesSubstance { get; init; } = "";
		public string Position { get; init; } = "";
		public string DescribesEquipment { get; init; } = "";
		public string Equipment { get; init; } = "";
		public string DescribesMeasure { get; init; } = "";
		public string Measure { get; init; } = "";
		public string Units { get; init; } = "";
		public string Value { get; init; } = "";
		public string DescribesCapability { get; init; } = "";
		public string CapabilityType { get; init; } = "";
		public string Qualifier { get; init; } = "";
		// old dtids
		public string dtid { get; set; }

	}


	public async Task CompareRulesAndBuilding(IEnumerable<KeyValuePair<string, int>> tagsUsedSummary,
		IDictionary<string, HashSet<string>> mapTagtoDTID,
		IDictionary<string, HashSet<string>> mapDTIDtoTag
		)
	{
		await Task.Delay(10000);  // Console.WriteLine issue

		var rulePointNames = this.GetAllPointNames()
			.Select(p => CanonicalTagSet.Create(p, 1).ToString())
			; // these are the tags from the SQL rules

		var descriptionsUsed = tagsUsedSummary
			.Select(d => CanonicalTagSet.Create(d.Key, d.Value))
			.Select(d => new KeyValuePair<string, CanonicalTagSet>(d.ToString(), d))
			.ToList()
			; // these are the tags from the building

		// Really want to do some equivalences here and replace forms that are the same by the earliest one

		// Really want to try the smart matching code here and see what we can match using heuristics

		var enumerator1 = descriptionsUsed.Where(x => x.Key is string).OrderBy(x => x.Key.ToUpperInvariant()).GetEnumerator();
		var enumerator2 = rulePointNames.OrderBy(x => x.ToUpperInvariant()).GetEnumerator();

		bool has1 = enumerator1.MoveNext();
		bool has2 = enumerator2.MoveNext();

		int colWidth = 60;
		string blank = new string('-', colWidth);

		string mydocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		string path = Path.Combine(mydocuments, "building_rules_comparison.csv");

		List<RuleComparison> records = new List<RuleComparison>();

		string wrap(string s) => $"\"{s}\"";
		string line(string s1, string s2, string s3) => $"{wrap(s1)},{wrap(s2)},{wrap(s3)}";

		Console.WriteLine("");
		Console.WriteLine(blank);
		Console.WriteLine(line("FOUND ON EQUIPMENT", "FOUND IN RULES", "DTID"));
		Console.WriteLine(blank);


		while (has1 && has2)
		{
			var mapped = mapTagtoDTID.TryGetValue(enumerator1.Current.Key, out var dtids) ? dtids : new HashSet<string>();
			string dtid = string.Join(", ", mapped);

			int comparison = enumerator1.Current.Key.ToUpperInvariant().CompareTo(enumerator2.Current.ToUpperInvariant());
			if (comparison == 0)
			{
				records.Add(new RuleComparison(enumerator1.Current.Key, enumerator1.Current.Value, enumerator2.Current, dtid));
				//Console.WriteLine(line(enumerator1.Current, enumerator2.Current, dtid));
				has1 = enumerator1.MoveNext();
				has2 = enumerator2.MoveNext();
			}
			else if (comparison < 0)
			{
				// Hack, why are these coming through
				if (dtid != "dtmi:com:willowinc:OccupancyZone;1")
				{
					records.Add(new RuleComparison(enumerator1.Current.Key, enumerator1.Current.Value, blank, dtid));
				}
				//Console.WriteLine(line(enumerator1.Current, blank, dtid));
				has1 = enumerator1.MoveNext();
			}
			else
			{
				records.Add(new RuleComparison(blank, CanonicalTagSet.Empty, enumerator2.Current, ""));
				//Console.WriteLine(line(blank, enumerator2.Current, ""));
				has2 = enumerator2.MoveNext();
			}
		}

		while (has1)
		{
			var mapped = mapTagtoDTID.TryGetValue(enumerator1.Current.Key, out var dtids) ? dtids : new HashSet<string>();
			string dtid = string.Join(", ", mapped);

			// Hack, why are these coming through
			if (dtid != "dtmi:com:willowinc:OccupancyZone;1")
			{
				records.Add(new RuleComparison(enumerator1.Current.Key, enumerator1.Current.Value, blank, dtid));
			}
			//Console.WriteLine(line(enumerator1.Current, blank, dtid));
			has1 = enumerator1.MoveNext();
		}
		while (has2)
		{
			records.Add(new RuleComparison(blank, CanonicalTagSet.Empty, enumerator2.Current, ""));
			//Console.WriteLine(line(blank, enumerator2.Current, ""));
			has2 = enumerator2.MoveNext();
		}

		Console.WriteLine();
		Console.WriteLine();
		Console.WriteLine();

		Console.ForegroundColor = ConsoleColor.Red;
		foreach (var mapping in mapDTIDtoTag)
		{
			if (mapping.Value.Count > 1)
			{
				Console.WriteLine($"{mapping.Key} has multiple tags: [{string.Join(",", mapping.Value)}]");
			}
		}

		foreach (var mapping in mapTagtoDTID)
		{
			if (mapping.Value.Count > 1)
			{
				Console.WriteLine($"{mapping.Key} has multiple dtmi: [{string.Join(",", mapping.Value)}]");
			}
		}

		Console.ResetColor();
		// And then look these up in DTDL to see if we can find matches

		CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			InjectionOptions = InjectionOptions.Escape
		};

		using (var writer = new StreamWriter(path))
		using (var csv = new CsvWriter(writer, config))
		{
			csv.WriteRecords(records);
		}
		await Task.Delay(5);
		// Open the CSV file in excel
		var p = new Process();
		p.StartInfo = new ProcessStartInfo(path)
		{
			UseShellExecute = true
		};
		p.Start();
	}

	private HashSet<string> allPointNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static Regex pointName = new Regex(@"\[([^\]]*)\]", RegexOptions.Compiled);

	public IReadOnlyCollection<string> GetAllPointNames()
	{
		foreach (var fault in this.Faults)
		{
			// Probably double including a capture group here but hashset will take care
			foreach (var capture in pointName.Matches(fault.PossibilitySyntax + " " + fault.RuleSyntax).SelectMany(x => x.Groups.OfType<Group>()))
			{
				allPointNames.Add(capture.Value.TrimStart('[').TrimEnd(']'));
			}
		}

		foreach (var process in this.Processes)
		{
			foreach (var capture in pointName.Matches(process.Syntax).SelectMany(x => x.Groups.OfType<Group>()))
			{
				allPointNames.Add(capture.Value.TrimStart('[').TrimEnd(']'));
			}
		}
		return allPointNames;
	}
}

#nullable disable
public class FaultRecord
{
	public string ID { get; set; }
	public string Class { get; set; }
	public string Marker { get; set; }
	public string Name { get; set; }
	public string RuleSyntax { get; set; }
	public string PossibilitySyntax { get; set; }
	public int EnergyRisk { get; set; }
	public int OperationsRisk { get; set; }
	public string Description { get; set; }
	public string PossibleCauses { get; set; }
	public string PossibleSolutions { get; set; }
}


public class ProcessRecord
{
	public string ID { get; set; }
	public string ClassProcess { get; set; }
	public string Class { get; set; }
	public string Process { get; set; }

	public string @Type { get; set; }

	public string KPI { get; set; }
	public string Syntax { get; set; }
}
