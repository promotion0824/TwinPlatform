namespace Willow.Api.Common.Domain;

using Newtonsoft.Json;

/// <summary>
/// A base class for entities.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<DomainEvent> domainEvents;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity"/> class.
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        domainEvents = new List<DomainEvent>();
    }

    /// <summary>
    /// Gets the entity identifier.
    /// </summary>
    [JsonProperty("id")]
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the domain events.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<DomainEvent> DomainEvents => domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event.
    /// </summary>
    /// <param name="notification">The notification to add to the collection of domain events.</param>
    public void AddDomainEvent(DomainEvent notification)
    {
        domainEvents.Add(notification);
    }

    /// <summary>
    /// Removes all domain events.
    /// </summary>
    public void ClearDomainEvent()
    {
        domainEvents.Clear();
    }
}
