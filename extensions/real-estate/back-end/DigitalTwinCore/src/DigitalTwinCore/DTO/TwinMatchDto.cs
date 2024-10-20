namespace DigitalTwinCore.Dto
{
    public class TwinMatchDto
    {
        public string OriginalTwinId { get; set; }

        public string ResolvedTwinId { get; set; }

        public object ResolvedTwinCustomPropertyValue { get; set; }
    }
}
