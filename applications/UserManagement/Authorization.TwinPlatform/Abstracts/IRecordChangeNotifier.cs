using Authorization.Common;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Record Change Notifier contract
/// </summary>
public interface IRecordChangeNotifier
{
    /// <summary>
    /// Triggered when a database record changes.
    /// </summary>
    /// <param name="targetRecord">Instance of the changed record.</param>
    /// <param name="recordAction">Type of change.</param>
    /// <returns>Awaitable task.</returns>
    Task AnnounceChange(object targetRecord, RecordAction recordAction);
}
