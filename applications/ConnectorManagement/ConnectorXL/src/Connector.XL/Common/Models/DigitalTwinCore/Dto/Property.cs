namespace Connector.XL.Common.Models.DigitalTwinCore.Dto;

[Serializable]
internal class Property
{
    public string DisplayName { get; set; }

    public object Value { get; set; }

    public PropertyKind Kind { get; set; }
}
