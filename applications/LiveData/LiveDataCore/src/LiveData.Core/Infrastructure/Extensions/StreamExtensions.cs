namespace PlatformWebXL.Infrastructure.Extensions;

using System.IO;

internal static class StreamExtensions
{
    public static byte[] ToByteArray(this Stream input)
    {
        using MemoryStream ms = new();
        input.CopyTo(ms);
        return ms.ToArray();
    }
}
