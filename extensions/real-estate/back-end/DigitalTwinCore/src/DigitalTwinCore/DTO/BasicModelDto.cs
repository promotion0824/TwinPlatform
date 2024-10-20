using DTDLParser.Models;
using System.Collections.Generic;

namespace DigitalTwinCore.DTO
{
	public class BasicModelDto
	{
		public IReadOnlyDictionary<string, string> DisplayName { get; set; }
		public DTSchemaInfo Schema { get; set; }
	}
}
