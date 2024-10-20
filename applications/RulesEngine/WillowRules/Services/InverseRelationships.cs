namespace Willow.Rules.Services;

public class InverseRelationships
{
	/// <summary>
	/// Gets the inverse relationship name for a relationship name
	/// </summary>
	public static string GetInverse(string name) => name switch
	{
		"isCapabilityOf" => "hasCapability",
		"isPartOf" => "comprises",
		"isFedBy" => "feeds",
		"isServedBy" => "serves",
		"servedBy" => "serves",
		"locatedIn" => "encloses",
		"includedIn" => "contains",
		"hasDocument" => "documentFor",
		"architectedBy" => "architectOf",
		"constructedBy" => "constructorOf",
		"ownedBy" => "owns",
		"installedBy" => "installed",
		"manufacturedBy" => "manufactures",
		"operatedBy" => "operates",

		_ => $"inverse({name})"
	};
}
