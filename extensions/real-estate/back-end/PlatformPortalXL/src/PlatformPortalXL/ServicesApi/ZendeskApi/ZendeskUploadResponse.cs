using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlatformPortalXL.ServicesApi.ZendeskApi;

public class ZendeskUploadResponse
{
    [JsonPropertyName("upload")]
    public ZendeskUpload Upload { get; set; }
}
public class ZendeskUpload
{
    [JsonPropertyName("token")]
    public string Token { get; set; }
    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }
    [JsonPropertyName("attachments")]
    public IList<ZendeskAttachment> Attachments { get; set; }
}

public class ZendeskAttachment
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("file_name")]
    public string FileName { get; set; }

    [JsonPropertyName("content_url")]
    public string ContentUrl { get; set; }

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

}
