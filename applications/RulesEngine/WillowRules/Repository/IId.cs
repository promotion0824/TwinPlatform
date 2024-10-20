namespace Willow.Rules.Repository;

/// <summary>
/// All items in the database need an Id
/// </summary>
public interface IId
{
	/// <summary>
	/// The Id of the item (used for database GetOne and partitioning)
	/// </summary>
	public string Id { get; }
}
