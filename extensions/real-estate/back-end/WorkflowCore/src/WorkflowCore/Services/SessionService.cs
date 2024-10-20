using System;
using WorkflowCore.Models;

namespace WorkflowCore.Services;

public interface ISessionService
{
    Guid? SourceId { get; }
    SourceType? SourceType { get; }

    /// <summary>
    /// Get site setting for ticket mapped integration 
    /// </summary>
    public MappedSiteSetting MappedSiteSetting { get; }
    void SetSessionData(SourceType? sourceType = null, Guid? sourceId = null);

    /// <summary>
    /// Set Session Data for Mapped site setting
    /// </summary>
    /// <param name="mappedSiteSetting"></param>
    void SetMappedSiteSetting(MappedSiteSetting mappedSiteSetting);
}


/// <summary>
/// store the data of the current request
/// either request is from user or from app
/// this data can be accessed from anywhere in the app
/// </summary>
public class SessionService : ISessionService
{
    /// <summary>
    /// the id of the user or app
    /// </summary>
    public Guid? SourceId { get; private set; }

    public SourceType? SourceType { get; private set; }

    /// <summary>
    /// Get site setting for ticket mapped integration 
    /// </summary>
    public MappedSiteSetting MappedSiteSetting { get; private set; }

    public void SetSessionData(SourceType? sourceType = null, Guid? sourceId = null)
    {
        if (sourceType.HasValue)
        {
            SourceType = sourceType.Value;
        }
        if (sourceId.HasValue)
        {
            SourceId = sourceId.Value;
        }
    }

    /// <summary>
    /// Set Session Data for Mapped site setting
    /// </summary>
    /// <param name="mappedSiteSetting"></param>
    public void SetMappedSiteSetting(MappedSiteSetting mappedSiteSetting)
    {
        MappedSiteSetting = mappedSiteSetting;
    }

}

public record MappedSiteSetting(Guid siteId, bool IsTicketMappedIntegrationEnabled);

