
namespace Authorization.Common.Models;

/// <summary>
/// File Import Record Actions
/// </summary>
public enum FileImportRecordAction
{
    Create,
    Delete,
    Update,
    NoAction
}

/// <summary>
/// Implementation for File Import Model
/// </summary>
public abstract class BaseFileImportModel
{
    /// <summary>
    /// Unique Identifier of the Model
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The type of import action to perform on the record.
    /// </summary>
    public FileImportRecordAction? Action { get; set; } = FileImportRecordAction.NoAction;

    /// <summary>
    /// Error or warning message that occurred during the record import.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
