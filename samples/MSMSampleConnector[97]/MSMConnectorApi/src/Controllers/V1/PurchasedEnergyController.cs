//-----------------------------------------------------------------------
// <copyright file="PurchasedEnergyController.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.MSMConnectorApi.Controllers.V1
{
    using System.Text.Json;
    using Azure.Data.Tables;
    using Azure.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.SmartPlaces.Facilities.MSMConnectorApi.DataModels.V1;
    using Microsoft.SmartPlaces.Facilities.MSMConnectorApi.Extensions.V1;

    [ApiController]
    [ApiVersion("2022-12-04")]
    //TODO: Finish setup and enable Authorization based on Partner solution
    //[Authorize]
    [Route("api/[Controller]")]
    public class PurchasedEnergyController : ControllerBase
    {
        private static readonly string[] OrganizationalUnit = new[]
        {
            "Issaquah", "Redmond", "Bellevue"
        };

        private static readonly string[] Buildings = new[]
        {
            "Samm-C", "Mil-F", "RTC-4", "Studio A", "109", "Lincoln Square", "Mil-E", "109", "109", "Mil-B", "43", "40", "40", "40", "40", "40", "17", "17", "17"
        };

        private static readonly string[] Names = new[]
        {
            "Vikas", "Sahil", "DanG", "Nico", "Rick", "Vidya", "Praba", "Praveen", "Greg", "DanE", "Supradha", "Bob", "Saad", "Stephanie"
        };

        private readonly ILogger<PurchasedEnergyController> logger;
        private readonly IConfiguration config;
        private readonly TableClient storage;

        public PurchasedEnergyController(ILogger<PurchasedEnergyController> logger, IConfiguration config)
        {
            this.logger = logger;
            this.config = config;
            storage = new TableClient(new Uri(this.config["storageAccountEndpoint"]), "msmconnector", new DefaultAzureCredential());
        }

        /// <summary>
        /// Generate some sample data to fill a storage account table with data
        /// that aligns to the schema of <see cref="PartnerPurchasedEnergyRecord"/>.
        /// This is to allow for separation of workflows while the generation of the
        /// persistent data is being worked on if it doesn't already exist.
        /// </summary>
        /// <param name="daysToCreate">How many days back would you like your data to simulate</param>
        /// <param name="recordsPerDay">How many different records would you like per day</param>
        /// <param name="cancellationToken">A way to stop things.</param>
        /// <returns>Created records</returns>
        [HttpPost("CreateSampleData")]
        public async Task<ActionResult<IEnumerable<PartnerPurchasedEnergyRecord>>> CreateSampleDataAsync(int daysToCreate = 7, int recordsPerDay = 1, CancellationToken cancellationToken = default)
        {
            try
            {
                var records = new List<PartnerPurchasedEnergyRecord>();
                for (int i = 0; i < daysToCreate; i++)
                {
                    var startDate = DateTime.UtcNow.Date.AddDays(-1 * i);
                    var endDate = DateTime.UtcNow.Date.AddDays(-1 * i + 1).AddMilliseconds(-1);
                    for (int j = 0; j < recordsPerDay; j++)
                    {
                        records.Add(new PartnerPurchasedEnergyRecord()
                        {
                            PartitionKey = startDate.ToString("yyyy-MM-dd"),
                            RowKey = j.ToString(),
                            ConsumptionStartDate = startDate,
                            ConsumptionEndDate = endDate,
                            EnergyProviderName = "Not quite sure",
                            Facility = Buildings[Random.Shared.Next(Buildings.Length)],
                            IsRenewable = Random.Shared.NextDouble() > 0.5,
                            Name = Names[Random.Shared.Next(Names.Length)],
                            OrganizationalUnit = OrganizationalUnit[Random.Shared.Next(OrganizationalUnit.Length)],
                            Quantity = Random.Shared.Next(20, 40),
                            QuantityUnit = "kWh",
                            DataQualityType = DataQualityType.Metered.ToString(),
                            EnergyType = EnergyType.Electricity.ToString(),
                        });
                    }
                }

                foreach (var record in records)
                {
                    logger.LogInformation("Record: {record}", JsonSerializer.Serialize(record));
                    await storage.AddEntityAsync(record, cancellationToken);
                }

                return new ObjectResult(records) { StatusCode = StatusCodes.Status201Created };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Something went wrong...");
                return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        /// <summary>
        /// A way to retrieve <see cref="CustomerPurchasedEnergyRecord"/> data records from the Partner
        /// </summary>
        /// <param name="request">A collection of query parameters</param>
        /// <param name="cancellationToken">A way to stop things.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<PurchasedEnergyQueryResponse>> QueryPurchasedElectricityAsync([FromBody] PurchasedEnergyQueryRequest request,
                                                                                                    CancellationToken cancellationToken = default)
        {
            if (request.StartDate >= request.EndDate)
            {
                return BadRequest(new ArgumentException($"StartDate '{request.StartDate}' must be less than EndDate '{request.EndDate}'"));
            }

            // Generate queries for the the persistent data storage
            var pages = storage.QueryAsync<PartnerPurchasedEnergyRecord>(filter =>
                                                                            filter.ConsumptionStartDate >= request.StartDate
                                                                            && filter.ConsumptionEndDate <= request.EndDate,
                                                                            cancellationToken: cancellationToken)
                                                                        .AsPages(request.ContinuationToken, 1);

            var response = new PurchasedEnergyQueryResponse();

            // Retrieve and translate records from persistent storage to the response body
            await foreach (var page in pages)
            {
                foreach (var record in page.Values)
                {
                    response.Data.Add(record.ToPurchasedEnergy());
                }

                // Respect the customer supplied MaxNumberOfItems request
                if (response.Data.Count >= request.MaxNumberOfItems)
                {
                    // If there is more data to be queried then provide the customer with a way
                    // to continue where they left off.
                    response.ContinuationToken = page.ContinuationToken;
                    break;
                }
            }

            logger?.LogInformation("Query Page Size: {numberOfRecordsReturned}", response.Data.Count);

            return Ok(response);
        }
    }
}
