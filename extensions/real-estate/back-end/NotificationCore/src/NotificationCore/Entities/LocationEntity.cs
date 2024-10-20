using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace NotificationCore.Entities;

[Table("Locations")]
[PrimaryKey(nameof(Id), nameof(NotificationTriggerId))]
public class LocationEntity
{
    /// <summary>
    /// Location Id is the twin id of the location
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [MaxLength(250)]
    public string Id { get; set; }
    [Required]
    public Guid NotificationTriggerId { get; set; }
    [ForeignKey(nameof(NotificationTriggerId))]
    public NotificationTriggerEntity NotificationTrigger { get; set; }

    internal static List<string> MapTo(IEnumerable<LocationEntity> locations)
    {
        return locations?.Select(c => c.Id).ToList();
    }

    public static List<LocationEntity> MapFrom(List<string> modelLocationIds)
    {
        return modelLocationIds?.Select(c => new LocationEntity
        {
            Id = c
        }).ToList();
    }
}
