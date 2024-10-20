namespace Willow.Msm.Connector.Models
{
    using System;
    using System.Numerics;
    using Newtonsoft.Json;

    /// <summary>
    /// Electric energy, measured in MWh, delivered by the utility to a customer in accordance with the signed agreement.
    /// </summary>
    public class MsmPurchasedEnergy
    {
        /// <summary>
        /// Gets or sets the Consumption end date (DateTime).
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public required string Msdyn_consumptionenddate { get; set; }

        /// <summary>
        /// Gets or sets the Consumption start date (DateTime).
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public required string Msdyn_consumptionstartdate { get; set; }

        /// <summary>
        /// Gets or sets the name of the custom entity.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public required string Msdyn_name { get; set; }

        /// <summary>
        /// Gets or sets the data quality type which can be actual, estimated, or other descriptors of the data.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public required string Msdyn_dataqualitytype { get; set; }

        /// <summary>
        /// Gets or sets the name of the energy provider.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public required string Msdyn_energyprovidername { get; set; }

        /// <summary>
        /// Gets or sets type of energy consumed, categorized within Scope 2 emissions.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public required string Msdyn_energytype { get; set; }

        /// <summary>
        /// Gets or sets the source of electric energy is renewable.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public required string Msdyn_isrenewable { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the facility associated with the purchased energy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public required string Msdyn_facilityid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the organizational unit associated with the purchased energy.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public required string Msdyn_organizationalunitid { get; set; }

        /// <summary>
        /// Gets or sets the quantity of energy consumed.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public required decimal Msdyn_quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure for the quantity of energy consumed.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public required string Msdyn_quantityunit { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the contractual instrument type associated with the purchased energy.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string? Msdyn_contractualinstrumenttypeid { get; set; }

        /// <summary>
        /// Gets or sets the date of the transaction associated with the energy consumption.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public DateTime? Msdyn_transactiondate { get; set; }

        /// <summary>
        /// Gets or sets the cost associated with the energy consumption.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public decimal? Msdyn_cost { get; set; }

        /// <summary>
        /// Gets or sets unit of measure for the cost of the energy consumed.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string? Msdyn_costunit { get; set; }

        /// <summary>
        /// Gets or sets additional detail about the energy consumption record.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string? Msdyn_description { get; set; }

        /// <summary>
        /// Gets or sets optional link to evidence or additional documentation associated with the energy consumption.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string? Msdyn_evidence { get; set; }

        /// <summary>
        /// Gets or sets an identifier for the usage of facilities in the context of energy consumption.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string? Msdyn_facilityusagedetailid { get; set; }

        /// <summary>
        /// Gets or sets the meter number associated with the energy consumption record.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string? Msdyn_meternumber { get; set; }

        /// <summary>
        /// Gets or sets the correlation ID originating from the source system for traceability.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string? Msdyn_origincorrelationid { get; set; }

        /// <summary>
        /// Gets or sets the Type ID categorizing the type of spending associated with the energy consumption.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string? Msdyn_spendtypeid { get; set; }

        /// <summary>
        /// Gets or sets the Partner ID within the value chain associated with the energy consumption.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string? Msdyn_valuechainpartnerid { get; set; }

        /// <summary>
        /// Gets or sets the Quantity of goods associated with the energy consumption.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public decimal? Msdyn_goodsquantity { get; set; }

        /// <summary>
        /// Gets or sets the Business unit owning the data related to the energy consumption.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string? OwningBusinessUnit { get; set; }

        /// <summary>
        /// Gets or sets the Unit of measure for the goods associated with the energy consumption.
        /// </summary>
        [JsonProperty(Required = Required.AllowNull)]
        public string? Msdyn_goodsunit { get; set; }
    }
}
