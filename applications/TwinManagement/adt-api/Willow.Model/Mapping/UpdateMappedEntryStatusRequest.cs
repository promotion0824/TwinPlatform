namespace Willow.Model.Mapping;

public class UpdateMappedEntryStatusRequest
{
    public required List<string> MappedIds { get; set; } = new List<string>();
    public required Status Status { get; set; }

}
