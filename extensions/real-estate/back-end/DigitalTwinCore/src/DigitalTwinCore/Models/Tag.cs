
namespace DigitalTwinCore.Models
{
    public class Tag
    {
        public string Name { get; set; }
        public TagType Type { get; set; }
    }

    public enum TagType
    {
        General = 0,
        TwoD = 1,
        ThreeD = 2
    }
}
