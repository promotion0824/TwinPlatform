using System;
using Willow.Infrastructure.Exceptions;

namespace DigitalTwinCore.Exceptions
{
    [Serializable]
    public class DigitalTwinCoreException : BadRequestException
    {
        public DigitalTwinCoreException(Guid? siteId, string message, Exception innerException = null) 
            : base(message, innerException)
        {
            SiteId = siteId;
        }

        public Guid? SiteId { get; private set; }
    }
}
