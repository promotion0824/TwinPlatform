namespace Willow.Msm.Connector.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Willow.Msm.Connector.Models;

#pragma warning disable SA1649 // File name should match first type name
    /// <summary>
    /// The Willow Client Interface.
    /// </summary>
    public interface IWillowClient
#pragma warning restore SA1649 // File name should match first type name
    {
        /// <summary>
        /// Initaialises the Willow Client.
        /// </summary>
        /// <param name="carbonActivityRequestMessage">The Carbon Activity Message.</param>
        /// <param name="log">Log.</param>
        void Initialise(CarbonActivityRequestMessage carbonActivityRequestMessage, ILogger log);

        /// <summary>
        /// Gets a Token to call Willlow Public API.
        /// </summary>
        /// <returns>Willow Token object.</returns>
        Task<WillowToken> GetToken();

        /// <summary>
        /// Gets Purchased Electricity.
        /// </summary>
        /// <returns>A list of Purchased Energy objects.</returns>
        Task<List<MsmPurchasedEnergy>> GetPurchasedElectricity();

        /// <summary>
        /// Get all the Facilities (sites) for the Organization.
        /// </summary>
        /// <returns>A List of MsmFacilities.</returns>
        Task<List<MsmFacility>> GetAllFacilities();

        /// <summary>
        /// Get all the Purchased Energy Twins for the Site, including their associated isCapabilityOf Utility Account names.
        /// </summary>
        /// <param name="siteId">The siteId.</param>
        /// <returns>A List of Purchased EnergyTwinDetails for the Facility (Site).</returns>
        Task<List<PurchasedEnergyTwinDetail>> GetMsmRelatedTwinDetailsForFacility(string siteId);
    }

    /// <summary>
    /// The Willow Client.
    /// </summary>
    internal partial class WillowClient : IWillowClient
    {
        private MsmOrganization msmOrganization;
        private CarbonActivityRequestMessage? carbonActivityRequestMessage;
        private ILogger? log;
        private readonly MsmFunctionOptions msmFunctionOptions;
        private readonly WillowToken willowToken;
        private readonly IHttpClientFactory httpClientFactory;

        public WillowClient(IOptions<MsmFunctionOptions> msmFunctionOptions, IHttpClientFactory httpClientFactory)
        {
            this.msmFunctionOptions = msmFunctionOptions.Value;
            this.msmOrganization = new MsmOrganization();
            this.willowToken = new WillowToken();
            this.httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Return all the Required Properties of an object that are not set.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>A comma seperated list of the required properties that have not been set.</returns>
        private static string GetUnsetRequiredProperties(object obj)
        {
            var unsetProperties = new List<string>();

            var properties = obj.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(JsonPropertyAttribute)));

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<JsonPropertyAttribute>();
                if (attribute != null && attribute.Required == Required.Always)
                {
                    var value = property.GetValue(obj);
                    if (value == null || (property.PropertyType.IsValueType && Equals(value, Activator.CreateInstance(property.PropertyType))))
                    {
                        unsetProperties.Add(property.Name);
                    }
                }
            }

            return string.Join(", ", unsetProperties);
        }
    }
}
