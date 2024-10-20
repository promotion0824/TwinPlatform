using System;
using System.Collections.Generic;

namespace Willow.IoTService.Monitoring.Options;

public class ThirdPartyConsumerOptions : IThirdPartyConsumerOptions
{
    public List<ThirdPartyConsumer> ThirdPartyConsumers { get; set; } = new();

    public ThirdPartyConsumer? GetThirdPartyConsumer(Guid siteID)
    {
        return this.ThirdPartyConsumers.Find(thirdPartyConsumer => thirdPartyConsumer.SiteID == siteID);
    }

    public List<ThirdPartyConsumer>? GetThirdPartyConsumers(Guid siteID)
    {
        return this.ThirdPartyConsumers.FindAll(thirdPartyConsumer => thirdPartyConsumer.SiteID == siteID);
    }
}

public class ThirdPartyConsumer
{
    public string External { get; set; } = string.Empty;

    public string Customer { get; set; } = string.Empty;

    public string Site { get; set; } = string.Empty;

    public Guid CustomerID { get; set; }

    public Guid SiteID { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        ThirdPartyConsumer customer = (ThirdPartyConsumer)obj;
        return (this.SiteID == customer.SiteID);
    }

    public override int GetHashCode()
    {
        return this.SiteID.GetHashCode();
    }

    public override string ToString()
    {
        return this.Customer + ", " + this.Site;
    }
}
