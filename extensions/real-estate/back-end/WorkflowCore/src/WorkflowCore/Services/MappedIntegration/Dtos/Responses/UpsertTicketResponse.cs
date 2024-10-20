using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using WorkflowCore.Services.MappedIntegration.Dtos.Requests;

namespace WorkflowCore.Services.MappedIntegration.Dtos.Responses;

public class UpsertTicketResponse : BaseResponse
{
    /// <summary>
    /// Upserted ticket data
    /// </summary>
    public UpsertedTicketData Data { get; set; }
    public static UpsertTicketResponse CreateSuccess(UpsertedTicketData upsertedTicketData)
    {
        return new UpsertTicketResponse
        {
            Data = upsertedTicketData,
            IsSuccess = true
        };
    }

    public static UpsertTicketResponse CreateDBFailure(DbUpdateException ex, TicketData ticketData)
    {
        var errorList = new List<string>();
        // ticket created from Mapped should have unique ExternalId, this constraint is enforced in DB
        // which ensure that sourceId and sourceType=Mapped and externalId are unique
        // 2601 is the error number for unique index violation
        // 'UX_WF_Ticket_SourceId_ExternalId_SourceType' is the name of the unique index
        if (ex.InnerException is SqlException innerException
            && innerException.Number == 2601
            && innerException.Message.Contains("UX_WF_Ticket_SourceId_ExternalId_SourceType"))
        {
            errorList.Add($"Ticket with same ExternalId ({ticketData.ExternalId}) already exists");
        }
        return new UpsertTicketResponse
        {
            ErrorList = errorList,
            IsSuccess = false
        };
    }
}
public record UpsertedTicketData(Guid TicketId);

