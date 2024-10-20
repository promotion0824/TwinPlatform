using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Copilot.ProxyAPI;

namespace PlatformPortalXL.Services.Copilot
{
    public interface ICopilotService
    {
        Task<CopilotChatResponse> ChatAsync(CopilotChatRequest request);
        Task<List<CopilotDocInfoResponse>> GetDocInfoAsync(CopilotDocInfoRequest request);
    }

    public class CopilotService : ICopilotService
    {
        private readonly ICopilotClient _copilotClient;
        private readonly ILogger<CopilotService> _logger;

        public CopilotService(ICopilotClient copilotClient, ILogger<CopilotService> logger)
        {
            _copilotClient = copilotClient;
            _logger = logger;
        }

        public async Task<CopilotChatResponse> ChatAsync(CopilotChatRequest request)
        {
            var response = await _copilotClient.ChatAsync(CopilotChatRequest.MapTo(request));

            return CopilotChatResponse.MapFrom(response);
        }

        public async Task<List<CopilotDocInfoResponse>> GetDocInfoAsync(CopilotDocInfoRequest request)
        {
            var response = await _copilotClient.DocInfoAsync(CopilotDocInfoRequest.MapTo(request));

            return CopilotDocInfoResponse.MapTo(response);
        }
    }

    public class CopilotContextOptions
    {
        public string ModelHint { get; set; }
        public List<string> PromptHints { get; set; }
        public List<string> RunFlags { get; set; }

        public static ChatRequestOptions MapTo(CopilotContextOptions options)
        {
            return new ChatRequestOptions()
            {
                Model_hint = options?.ModelHint,
                Prompt_hints = options?.PromptHints,
                Run_flags = options?.RunFlags
            };
        }
    }

    public class CopilotContext
    {
        public string SessionId { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }

        public static Context MapTo(CopilotContext context)
        {
            return new Context()
            {
                User_name = context?.UserName,
                Session_id = context?.SessionId,
                User_id = context.UserId,
                User_email = context.UserEmail
            };
        }
    }

    public class CopilotChatRequest
    {
        public string UserInput { get; set; } 
        public CopilotContext Context { get; set; }
        public CopilotContextOptions Options { get; set; }

        public static ChatRequest MapTo(CopilotChatRequest request)
        {
            return new ChatRequest()
            {
                Context = CopilotContext.MapTo(request.Context),
                Options = CopilotContextOptions.MapTo(request.Options),
                User_input = request.UserInput
            };
        }
    }

    public class CopilotChatResponseCitation
    {
        public string Name { get; set; }

        public List<string> Pages { get; set; }

        public static CopilotChatResponseCitation MapFrom(ChatResponseCitation citation)
        {
            return new CopilotChatResponseCitation()
            {
                Name = citation?.Name,
                Pages = citation?.Pages?.ToList()
            };
        }

        public static List<CopilotChatResponseCitation> MapFrom(ICollection<ChatResponseCitation> citations)
        {
            return citations?.Select(MapFrom).ToList();
        }
    }

    public class CopilotChatResponse
    {
        public string ResponseText { get; set; }
        public string ResponseFormat { get; set; }
        public List<CopilotChatResponseCitation> Citations { get; set; }
        public Debug_info DebugInfo { get; set; }

        public static CopilotChatResponse MapFrom(ChatResponse response)
        {
            return new CopilotChatResponse()
            {
                ResponseText = response.ResponseText,
                ResponseFormat = response.Response_format,
                Citations = CopilotChatResponseCitation.MapFrom(response.Citations),
                DebugInfo = response.Debug_info
            };
        }
    }

    public class CopilotDocInfoRequest
    {
        public List<string> Filenames { get; set; }

        public static GetIndexDocumentInfoRequest MapTo(CopilotDocInfoRequest request)
        {
            return new GetIndexDocumentInfoRequest()
            {
                Blob_files = request.Filenames
            };
        }
    }

    public class CopilotDocInfoResponse
    {
        //
        // Summary:
        //     The storage location of the file
        public string Uri { get; set; }

        //
        // Summary:
        //     The name of the file
        public string Filename { get; set; }

        //
        // Summary:
        //     LLM-generated summary for document, if any
        public string Summary { get; set; }

        //
        // Summary:
        //     ISO time string when document was indexed
        public DateTimeOffset? IndexedTime { get; set; }

        //
        // Summary:
        //     ISO time string when summary was created/updated
        public DateTimeOffset? UpdatedTime { get; set; }

        //
        // Summary:
        //     The size in bytes of the source document
        public double? DocumentSize { get; set; }

        public static CopilotDocInfoResponse MapTo(GetIndexDocumentInfoDocInfo response)
        {
            return new CopilotDocInfoResponse()
            {
                DocumentSize = response.Document_size,
                Filename = response.File,
                IndexedTime = response.Indexed_time,
                Summary = response.Summary,
                UpdatedTime = response.Summary_updated_time,
                Uri = response.Uri
            };
        }

        public static List<CopilotDocInfoResponse> MapTo(IEnumerable<GetIndexDocumentInfoDocInfo> response)
        {
            return response?.Select(MapTo).ToList();
        }
    }
}
