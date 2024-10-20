namespace Willow.LiveData.Core.Domain;

using System;

/// <summary>
/// Represents a stored query result.
/// </summary>
public class StoredQueryResult
{
    /// <summary>
    /// Gets or sets the identifier of the stored query result.
    /// </summary>
    public Guid StoredQueryResultId { get; set; }

    /// <summary>
    /// Gets or sets the name of the stored query result.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    public string DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the name of the collection.
    /// </summary>
    public string PrincipalIdentity { get; set; }

    /// <summary>
    /// Gets or sets the size in bytes.
    /// </summary>
    public long SizeInBytes { get; set; }

    /// <summary>
    /// Gets or sets the count.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the created on date.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the expires on date.
    /// </summary>
    public DateTime ExpiresOn { get; set; }
}
