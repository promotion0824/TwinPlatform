// -----------------------------------------------------------------------
// <copyright file="MappedDtdlTests.cs" Company="Willow">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.TopologyIngestion.Mapped.Test
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using DTDLParser;
    using DTDLParser.Models;
    using Xunit.Abstractions;

    public class MappedDtdlTests
    {
        private readonly ITestOutputHelper output;

        public MappedDtdlTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        // [Fact]
        public async Task LoadDtdlMapped()
        {
            ModelParser parser = new ModelParser();

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("mapped_dtdl.json"));
            List<string> dtdls = new List<string>();

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string result = reader.ReadToEnd();
                        dtdls.Add(result);
                    }
                }
                else
                {
                    throw new FileNotFoundException(resourceName);
                }
            }

            try
            {
                var objectModel = await parser.ParseAsync(dtdls.ToAsyncEnumerable());

                var interfaces = objectModel.Values.Where(m => m.EntityKind == DTEntityKind.Interface);
                var relationshipNames = new Dictionary<string, string>();

                foreach (var intef in interfaces)
                {
                    var i = intef as DTInterfaceInfo;
                    if (i != null)
                    {
                        var relationships = i.Contents.Where(c => c.Value.EntityKind == DTEntityKind.Relationship);
                        foreach (var rel in relationships)
                        {
                            var r = rel.Value as DTRelationshipInfo;

                            if (r != null)
                            {
                                var relName = r.Name;
                                relationshipNames[relName] = r.Name;
                            }
                        }
                    }
                }

                foreach (var relationship in relationshipNames.Values)
                {
                    output.WriteLine(relationship);
                }
            }
            catch (ParsingException ex)
            {
                output.WriteLine(ex.ToString());

                foreach (var err in ex.Errors)
                {
                    output.WriteLine(err.ToString());
                }

                throw;
            }
        }
    }
}
