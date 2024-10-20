namespace Authorization.TwinPlatform.Web.Abstracts;

/// <summary>
/// Service to manage Import / Export of Authorization Instance Data
/// </summary>
public interface IImportExportManager
{
    /// <summary>
    /// Gets the entity types supported for import/export operation;
    /// </summary>
    /// <returns>Array of supported entity types.</returns>
    public string[] GetSupportedRecordTypes();

    /// <summary>
    /// Export all record types matching the input entity type list.
    /// </summary>
    /// <param name="recordTypes">Array of Entity Types.</param>
    /// <returns>ByteArray of Zip data.</returns>
    public Task<byte[]> ExportRecordsByTypesAsync(string[] recordTypes);

    /// <summary>
    /// Method to import Authorization Data
    /// </summary>
    /// <param name="zipFileStream">ZipArchive File Stream</param>
    /// <returns>Report File byte array.</returns>
    public Task<byte[]> ImportRecordsAsync(Stream zipFileStream);

}
