using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkflowCore.Entities;

/// <summary>
/// The table contains the data of external users who are not registered in Willow.
/// </summary>
[Table("WF_ExternalProfiles")]
public class ExternalProfileEntity
{
    /// <summary>
    /// Id of the record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the external user.
    /// </summary>
    [Required]
    [MaxLength(250)]
    public string Name { get; set; }

    /// <summary>
    /// Email of the external user.
    /// this Email should be unique
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Email { get; set; }
    /// <summary>
    /// Phone of the external user.
    /// </summary>
    [MaxLength(32)]
    public string Phone { get; set; }
    /// <summary>
    /// Company name of the external user.
    /// </summary>
    [MaxLength(250)]
    public string Company { get; set; }
}

