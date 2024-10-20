namespace Willow.LiveData.Pipeline;

using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// The MessageProcessingHelper class provides helper methods for processing messages.
/// </summary>
public static class MessageProcessingHelper
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    /// <summary>
    /// Decompresses and deserializes a message into the specified type.
    /// </summary>
    /// <typeparam name="TTelemetry">The type to deserialize the message into.</typeparam>
    /// <param name="messageData">The byte array containing the compressed message data.</param>
    /// <returns>The deserialized object of type TTelemetry.</returns>
    public static async Task<TTelemetry> DecompressAndDeserializeMessage<TTelemetry>(byte[] messageData)
    {
        using var memoryStream = new MemoryStream(messageData);
        await using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();
        await gzipStream.CopyToAsync(decompressedStream);
        var decompressedMsg = Encoding.UTF8.GetString(decompressedStream.ToArray());

        return JsonSerializer.Deserialize<TTelemetry>(decompressedMsg, JsonSerializerOptions)!;
    }
}
