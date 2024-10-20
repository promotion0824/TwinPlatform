namespace Willow.Msm.Connector.Services
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Willow.Msm.Connector.Models;

    /// <summary>
    /// Initialise Method for Willow Client.
    /// </summary>
    internal partial class WillowClient : IWillowClient
    {
        public void Initialise(CarbonActivityRequestMessage carbonActivityRequestMessage, ILogger log)
        {
            this.log = log;

            this.log.LogInformation("Initialising Willow Client");

            this.carbonActivityRequestMessage = carbonActivityRequestMessage;

            // TODO: Discuss getting msdyn_organizationalunitid from the 'Portfolio' collection in the Twin (dtmi:com:willowinc:Portfolio;1)
            this.msmOrganization.Msdyn_organizationalunitid = carbonActivityRequestMessage.OrganizationalUnitId;
        }
    }
}
