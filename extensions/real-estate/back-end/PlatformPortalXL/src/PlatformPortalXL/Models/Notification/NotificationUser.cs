using System;

namespace PlatformPortalXL.Models.Notification
{
    public enum NotificationSource
    {
        Insight = 1
    }

    public class NotificationUser
    {
        public Guid Id { get; set; }
        public NotificationSource Source { get; set; }
        public string Title { get; set; }
        public string PropertyBagJson { get; set; }
        public Guid UserId { get; set; }
        public NotificationUserState State { get; set; }
        public DateTime? ClearedDateTime { get; set; }
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// SourceId  is the id of the entity that this notification is associated with
        /// this id will be stored as a string, but it can represent any type of entity id
        /// and the type of entity will be determined by the Source property.
        /// </summary>
        public string SourceId { get; set; }
    }
}
