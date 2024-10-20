using System.Collections.Generic;

namespace WorkflowCore.Services.MappedIntegration.Dtos.Responses;

public class GetTicketsResponse : BaseResponse
{
    /// <summary>
    /// Get list of tickets
    /// </summary>
    public List<MappedTicketDto> Data { get; set; }
    public static GetTicketsResponse CreateSuccess(List<MappedTicketDto> data)
    {
        return new GetTicketsResponse
        {
            Data = data,
            IsSuccess = true
        };
    }
}

