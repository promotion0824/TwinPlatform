using Microsoft.EntityFrameworkCore;
using NotificationCore.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotificationCore.Entities;

[Table("NotificationsUsers")]
[PrimaryKey(nameof(UserId), nameof(NotificationId))]
public class NotificationUserEntity
{
    public Guid UserId { get; set; }
    public Guid NotificationId { get; set; }
    [ForeignKey(nameof(NotificationId))]
    public NotificationEntity Notification { get; set; }
    public NotificationUserState State { get; set; }
    public DateTime? ClearedDateTime { get; set; }
}

