namespace Willow.Msm.Connector.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Willow.Msm.Connector.Models;

    /// <summary>
    /// GetPurchasedEnergyQuantityForFacility.
    /// </summary>
    internal partial class WillowClient : IWillowClient
    {
        public async Task<List<PurchasedEnergyTwinDetail>> GetPurchasedEnergyQuantityForFacility(string siteId)
        {
            log!.LogInformation($"Getting Purchased Energy Quantity for Facility: {siteId}");

            // Iterate the purchasedEnergyTwinDetails for the site (facilty) and get the Quantity for each Twin
            var facility = this.msmOrganization.MsmFacilities!.Find(f => f.SiteId == siteId);

            foreach (var purchasedEnergyTwin in facility!.PurchasedEnergyTwinDetails)
            {
                // dont get the quantity if msdyn_energyprovidername is not set
                if (string.IsNullOrEmpty(purchasedEnergyTwin.Msdyn_energyprovidername))
                {
                    continue;
                }

                string currentDateFormatted = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                var watermarkDateFormatted = carbonActivityRequestMessage!.WatermarkDate.ToString("yyyy-MM-dd HH:mm");
                var quantityUrl = $"https://{this.carbonActivityRequestMessage.OrganizationShortName}.app.willowinc.com/publicapi/v2/sites/{siteId}/points/{purchasedEnergyTwin.TrendId}/trendlog?startDate={watermarkDateFormatted}&endDate={currentDateFormatted}&PageSize=100&PageStart=0";

                using var client = httpClientFactory.CreateClient();
                using var quantityRequest = new HttpRequestMessage(HttpMethod.Get, quantityUrl);
                quantityRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.willowToken.Token);

                using var quantityResponseMessage = await client.SendAsync(quantityRequest);
                var quantityResponseBody = quantityResponseMessage.Content.ReadAsStringAsync().Result;

                var quantities = GetQuantity(carbonActivityRequestMessage.AggregationWindow, quantityResponseBody);

                if (quantities.Count > 0)
                {
                    purchasedEnergyTwin.Quantities = quantities;
                }
            }

            return facility.PurchasedEnergyTwinDetails;
        }

        private static List<TimestampedDataPoint> GetQuantity(string aggregationWindow, string pointDataJson)
        {
            var pointData = JsonConvert.DeserializeObject<dynamic>(pointDataJson);
            var data = ((JArray)pointData!.data).ToObject<List<DataPoint>>();

            List<TimestampedDataPoint> result = new List<TimestampedDataPoint>();

            switch (aggregationWindow)
            {
                case "None":
                    for (int i = 0; i < data!.Count; i++)
                    {
                        var currentData = data[i];
                        DateTime startDate = i > 0 ? data[i - 1].Timestamp : currentData.Timestamp;
                        DateTime endDate = currentData.Timestamp;
                        if (i > 0)
                        {
                            result.Add(new TimestampedDataPoint { StartDate = startDate, EndDate = endDate, Value = currentData.Value });
                        }
                    }

                    break;
                case "Day":
                    result = data!.GroupBy(x => x.Timestamp.Date)
                                 .Select(g => new TimestampedDataPoint
                                 {
                                     StartDate = g.Key,
                                     EndDate = g.Key.AddHours(23).AddMinutes(59),
                                     Value = g.Sum(x => x.Value),
                                 })
                                 .ToList();
                    break;
                case "Week":
                    CultureInfo ci = CultureInfo.CurrentCulture;
                    result = data!.GroupBy(x => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(x.Timestamp, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek))
                                 .Select(g =>
                                 {
                                     var weekYear = g.First().Timestamp.Year;

                                     // Finding the first day of the week for the earliest timestamp in the group
                                     var firstTimestamp = g.Min(x => x.Timestamp);
                                     var diff = (7 + (firstTimestamp.DayOfWeek - CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)) % 7;
                                     var firstDayOfWeek = firstTimestamp.AddDays(-diff).Date;

                                     // Adjust for year transition issue, ensuring the week belongs to the correct year
                                     if (firstDayOfWeek.Year < weekYear)
                                     {
                                         firstDayOfWeek = firstDayOfWeek.AddDays(7);
                                     }

                                     var lastDayOfWeek = firstDayOfWeek.AddDays(6).Date.AddHours(23).AddMinutes(59);
                                     return new TimestampedDataPoint { StartDate = firstDayOfWeek, EndDate = lastDayOfWeek, Value = g.Sum(x => x.Value) };
                                 })
                                 .ToList();

                    break;
                case "Month":
                    result = data!.GroupBy(x => new { x.Timestamp.Year, x.Timestamp.Month })
                                 .Select(g =>
                                 {
                                     var firstDayOfMonth = new DateTime(g.Key.Year, g.Key.Month, 1);
                                     var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);
                                     return new TimestampedDataPoint { StartDate = firstDayOfMonth, EndDate = lastDayOfMonth, Value = g.Sum(x => x.Value) };
                                 })
                                 .ToList();
                    break;
                default:
                    throw new ArgumentException("Invalid aggregation window");
            }

            return result;
        }
    }
}
