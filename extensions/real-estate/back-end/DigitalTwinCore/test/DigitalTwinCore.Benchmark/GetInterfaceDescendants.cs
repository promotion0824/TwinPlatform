using BenchmarkDotNet.Attributes;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Test.Services.DigitalTwinModelParser;
using DTDLParser.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace DigitalTwinCore.Benchmark
{

	public class GetInterfaceDescendants
	{

		private readonly DigitalTwinModelParser _modelParser;
		public GetInterfaceDescendants()
		{
			var assembly = Assembly.GetAssembly(typeof(DigitalTwinModelParserTests));

			var resourceName = assembly!.GetManifestResourceNames()
				.Single(str => str.EndsWith("wil-prd-lda-msft-eu22-adt-models.json"));

			using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException("Requires benchmark source data");
			using StreamReader reader = new StreamReader(stream!);
			var result = reader.ReadToEnd();

			var modelData = JsonSerializer.Deserialize<List<Model>>(result);

			_modelParser = DigitalTwinModelParser.CreateAsync(modelData, new Mock<ILogger<IDigitalTwinService>>().Object).Result;

		}


		/*
         Prior to change to use of ILookup
        |                       Method |     Mean |   Error |  StdDev |
        |----------------------------- |---------:|--------:|--------:|
        | GetInterfaceDescendantsTests | 152.7 ms | 2.82 ms | 2.36 ms |
    
        With ILookup improvements
        |                       Method |     Mean |     Error |    StdDev |
        |----------------------------- |---------:|----------:|----------:|
        | GetInterfaceDescendantsTests | 1.391 ms | 0.0274 ms | 0.0584 ms |
         */
		[Benchmark]
		public IReadOnlyDictionary<string, DTInterfaceInfo> GetInterfaceDescendantsTests() => _modelParser.GetInterfaceDescendants(new[] { WillowInc.PointModelId });
	}
}
