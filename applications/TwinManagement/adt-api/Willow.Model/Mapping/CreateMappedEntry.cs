namespace Willow.Model.Mapping;

public class CreateMappedEntry
{
    public required string MappedId { get; set; }

    public required string MappedModelId { get; set; }

    public string? WillowModelId { get; set; }

    public string? ParentMappedId { get; set; }

    public string? ParentWillowId { get; set; }

    public string? WillowParentRel { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? ModelInformation { get; set; }

    public string? StatusNotes { get; set; }

    public Status Status { get; set; }

    public string? AuditInformation { get; set; }

    /// <summary>
    /// Get or set ConnectorId, which is the id of the connector that is associated with the mapped twins.
    /// </summary>
    public string? ConnectorId { get; set; }

    /// <summary>
    /// Get or set BuildingId, which is the id of the building that is associated with the mapped twins.
    /// </summary>
    public string? BuildingId { get; set; }

    /// <summary>
    /// Get or set WillowId, which is the id that will be used for the Willow twin when it is created.
    /// </summary>
    public string? WillowId { get; set; }

    /// <summary>
    /// Get or set IsExistingTwin, which is a flag to indicate if the Willow twin has its externalId already defined and should not be created.
    /// </summary>
    public bool IsExistingTwin { get; set; } = false;

    /// <summary>
    /// Get or set Mapped's units of measurement for point data, such as "Degrees"
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Get or set Mapped's class or subclass of the point, like "Alarm" and "TemperatureAlarm".
    /// </summary>
    public string? DataType { get; set; }
}
