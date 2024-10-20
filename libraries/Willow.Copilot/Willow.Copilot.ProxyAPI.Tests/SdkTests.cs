namespace Willow.Copilot.ProxyAPI.Tests;

public class SdkTests
{
    private CopilotClient _client;

    const string URI = "http://localhost:8080/";

    public SdkTests()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(URI)
        };

        _client = new CopilotClient(httpClient);
    }

    [Fact]
    public async Task Test_DocInfoAsync()
    {
        var request = new GetIndexDocumentInfoRequest() { Blob_files = [
            "troubleshooting-guide.pdf",
            "foo.bar"
           ] };

        var response = await _client.DocInfoAsync(request);

        Assert.NotNull(response);
    }
}
