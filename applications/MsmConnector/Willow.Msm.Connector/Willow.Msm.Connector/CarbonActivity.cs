namespace Willow.Msm.Connector
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;
    using Microsoft.AspNetCore.Http.HttpResults;
    using Microsoft.AspNetCore.Mvc;
    using Willow.Msm.Connector.Models;
    using Willow.Msm.Connector.Services;

    /// <summary>
    /// Retrieves Carbon Activity data from the Willow App.
    /// </summary>
    /// <remarks>see: https://learn.microsoft.com/en-us/common-data-model/schema/core/industrycommon/sustainability/sustainabilitycarbon/overview.</remarks>
    public class CarbonActivity
    {
        /// <summary>
        /// Process a CarbonActivity request.
        /// </summary>
        /// <param name="carbonActivityRequestMessage">Carbon Activity Request Message.</param>
        /// <param name="willowClient">Instance of the Willow Client.</param>
        /// <param name="log">Instance of ILogger.</param>
        /// <returns>A list of Carbon Activities matching the request.</returns>
        public static async Task<Results<JsonHttpResult<List<MsmPurchasedEnergy>>, BadRequest<ErrorResponseMessage>>> ProcessCarbonActivityRequest([FromBody]CarbonActivityRequestMessage carbonActivityRequestMessage, IWillowClient willowClient, ILogger<CarbonActivity> log)
        {
            // Ensure the request has the required parameters.
            try
            {
                Validator.ValidateObject(carbonActivityRequestMessage, new ValidationContext(carbonActivityRequestMessage), true);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponseMessage
                {
                    Status = "Error",
                    Message = "Error parsing the CarbonActivity request.",
                    DetailedMessage = ex.Message,
                };

                return TypedResults.BadRequest(errorResponse);
            }

            log.LogInformation($"Received {carbonActivityRequestMessage.EnergyType} request.");

            // Return error if request is for an unsupported EnergyType
            if (carbonActivityRequestMessage.EnergyType != "Electricity")
            {
                var errorResponse = new ErrorResponseMessage
                {
                    Status = "Error",
                    Message = "Unsupported EnergyType. Only 'Electricity' is supported at this time.",
                };

                return TypedResults.BadRequest(errorResponse);
            }

            // Return error if request is for an unsupported Aggregation Window
            var supportedValues = new List<string> { "None", "Day", "Week", "Month" };
            if (!supportedValues.Contains(carbonActivityRequestMessage.AggregationWindow))
            {
                var errorResponse = new ErrorResponseMessage
                {
                    Status = "Error",
                    Message = "Unsupported AggregationWindow. Supported values are: None, Day, Week, Month.",
                };
                return TypedResults.BadRequest(errorResponse);
            }

            // Get the Carbon Activity Data
            try
            {
                willowClient.Initialise(carbonActivityRequestMessage, log);
                await willowClient.GetToken();
                await willowClient.GetAllFacilities();
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponseMessage
                {
                    Status = "Error",
                    Message = "Error occurred while getting Organization and Facility Information from the Willow Twin.",
                    DetailedMessage = ex.Message,
                };
                return TypedResults.BadRequest(errorResponse);
            }

            if (carbonActivityRequestMessage.EnergyType == "Electricity")
            {
                var purchasedEnergy = await willowClient.GetPurchasedElectricity();

                // Alternative with no formatting: return TypedResults.Ok(purchasedEnergy);
                return TypedResults.Json(purchasedEnergy, new System.Text.Json.JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                });
            }
            else
            {
                var errorResponse = new ErrorResponseMessage
                {
                    Status = "Error",
                    Message = "Unsupported EnergyType. Only 'Electricity' is supported at this time.",
                };
                return TypedResults.BadRequest(errorResponse);
            }
        }
    }
}
