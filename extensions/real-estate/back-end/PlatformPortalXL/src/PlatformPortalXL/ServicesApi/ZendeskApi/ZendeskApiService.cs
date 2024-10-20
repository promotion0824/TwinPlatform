using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace PlatformPortalXL.ServicesApi.ZendeskApi;

public interface IZendeskApiService
{
    Task<List<ZendeskUpload>> UploadAttachmentsAsync(IFormFileCollection attachments);
    Task<ZendeskTicketResponse> CreateTicket(ZendeskCreateTicketRequest zendeskTicketRequest);
}
public class ZendeskApiService: IZendeskApiService
{
    private readonly  IOptions<ZendeskOptions> _options;
    private readonly IHttpClientFactory _httpClientFactory;
    public ZendeskApiService(IOptions<ZendeskOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ZendeskTicketResponse> CreateTicket(ZendeskCreateTicketRequest zendeskTicketRequest)
    {
        var url = $"api/v2/tickets";
        using var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_options.Value.BaseAddress); 
        client.DefaultRequestHeaders.Add("Authorization", GetBasicAuthHeader());
        var result=await client.PostAsync(url, JsonContent.Create(new ZendeskTicketRequest<ZendeskCreateTicketRequest>(zendeskTicketRequest)));
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadAsAsync<ZendeskTicketResponse>();
    }
    public async Task<List<ZendeskUpload>> UploadAttachmentsAsync(IFormFileCollection attachments)
    {
        if (attachments == null || !attachments.Any())
            return null;

        var uploadTasks = attachments.Select(attachment => UploadAttachmentAsync(attachment));
         
        return (await Task.WhenAll(uploadTasks)).ToList();
    }
    private async Task<ZendeskUpload> UploadAttachmentAsync(
        IFormFile attachment,string token=null)
    {
        var url = $"api/v2/uploads?filename={attachment.FileName}";
        if(!string.IsNullOrEmpty(token))
            url+=$"&token={token}";

        using var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_options.Value.BaseAddress);
        client.DefaultRequestHeaders.Add("Authorization", GetBasicAuthHeader());
        using var stream = new MemoryStream();
        await attachment.OpenReadStream().CopyToAsync(stream);
        using var content = new ByteArrayContent(stream.ToArray());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/binary");

        var result= await client.PostAsync(
            url,
            content);
        result.EnsureSuccessStatusCode();
        var response= await result.Content.ReadAsAsync<ZendeskUploadResponse>();
        return response?.Upload;
    }
    private string GetBasicAuthHeader()
    {
        var authorizationHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Value.AuthUsername}/token:{_options.Value.AuthPassword}"));
        return $"Basic {authorizationHeader}";
    }
}
