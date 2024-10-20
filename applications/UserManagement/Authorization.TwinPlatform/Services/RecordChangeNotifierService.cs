using Authorization.Common;
using Authorization.TwinPlatform.Abstracts;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Record Change Notifier Service Implementation.
/// </summary>
/// <param name="listeners"></param>
public class RecordChangeNotifierService (IEnumerable<IRecordChangeListener>? listeners) : IRecordChangeNotifier
{
    /// <summary>
    /// Triggered when a database record changes.
    /// </summary>
    /// <param name="targetRecord">Instance of the changed record.</param>
    /// <param name="recordAction">Type of change action.</param>
    /// <returns>Awaitable task.</returns>
    public Task AnnounceChange(object targetRecord, RecordAction recordAction)
    {
       // If no notifiers registered, return
       if(listeners is null || !listeners.Any()) {
            return Task.CompletedTask;
        }

        var allNotificationTasks = listeners.Select(s => s.Notify(targetRecord, recordAction));

        return Task.WhenAll(allNotificationTasks);
    }
}
