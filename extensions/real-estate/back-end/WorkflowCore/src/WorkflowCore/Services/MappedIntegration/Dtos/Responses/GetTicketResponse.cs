namespace WorkflowCore.Services.MappedIntegration.Dtos.Responses;

public class GetTicketResponse : BaseResponse
{
    /// <summary>
    /// Ticket Data
    /// </summary>
    public MappedTicketDto Data { get; set; }

    public static GetTicketResponse CreateSuccess(MappedTicketDto data)
    {
        return new GetTicketResponse
        {
            Data = data,
            IsSuccess = true
        };
    }
}

