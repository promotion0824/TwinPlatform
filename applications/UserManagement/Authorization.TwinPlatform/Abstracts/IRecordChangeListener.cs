using Authorization.Common;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Record Change Listener Contract.
/// </summary>
public interface IRecordChangeListener
{
    /// <summary>
    /// Triggered when a record changes.
    /// </summary>
    /// <param name="targetRecord">Instance of the changed record.</param>
    /// <param name="recordAction">Type of change.</param>
    /// <returns>Awaitable task.</returns>
    Task Notify(object targetRecord, RecordAction recordAction);
}
