using System.IO;

namespace DigitalTwinCore.Extensions
{
	public static class MemoryStreamExtensions
	{
        public static MemoryStream FromString(this MemoryStream stream, string s)
        {
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
