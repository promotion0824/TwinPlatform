using System;

namespace InsightCore.Models;

public class Dependency
{
	/// <summary>
	/// Relationship type
	/// </summary>
	public string Relationship { get; set; }

	/// <summary>
	/// insight id in InsightCore
	/// </summary>
	public Guid InsightId { get; set; }
}
