using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PlatformPortalXL.Services.ContactUs;
using Willow.DataValidation;

namespace PlatformPortalXL.Features.ContactUs;

public class CreateSupportTicketRequest
{
    [HtmlContent]
    [Required]
    public string Comment { get; set; }
    public Guid? SiteId { get; set; }
    public ContactUsCategory Category { get; set; }
    [HtmlContent]
    [Required]
    public string RequestorsName { get; set; }
    [Required]  
    [Email]
    public string RequestorsEmail{ get; set; }
    [HtmlContent]
    public string Subject { get; set;}
    public List<Guid> InsightIds { get; set; }
    [Required]
    [Url]
    public string Url { get; set; }
    [JsonIgnore]
    public string CommentBody => $"Requestors name: {RequestorsName}\n\rRequestors email address: {RequestorsEmail}\n\rSubmitted url: {Url}\n\rSubmitted date: {DateTime.UtcNow.ToString("dd MMM yyyy hh:mm tt")}\n\rDetail:\n\r {Comment}\n\r";
}
