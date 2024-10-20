namespace PlatformPortalXL.Dto
{
    public class EnumKeyValueDto
    {
        public EnumKeyValueDto(int key, string value)
        {
            Key=key;
            Value=value;
        }

        public int Key { get; set; }
        public string Value { get; set; }
    }
}
