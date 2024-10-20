using System;

namespace WorkflowCore.Services.MappedIntegration.Models;

public class MappedReporter
{
    public Guid? ReporterId { get; set; }
    /// <summary>
    /// Reporter full name
    /// </summary>
    public string ReporterName { get; set; }

    /// <summary>
    /// Reporter phone number
    /// </summary>
    public string ReporterPhone { get; set; } = string.Empty;
    /// <summary>
    /// Reporter email
    /// </summary>
    public string ReporterEmail { get; set; }

    /// <summary>
    /// Reporter company name
    /// </summary>
    public string ReporterCompany { get; set; } = string.Empty;
}

