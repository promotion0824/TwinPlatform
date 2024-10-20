namespace Willow.SubscriptionUpload;

using Azure.Data.Tables;
using Azure.Identity;
using CsvHelper;

internal class SubscriptionUpload
{
    private const string TableName = "TelemetryStreamingSubscriptions";

    private static readonly string[] RequiredHeaders = [
        "ConnectorId",
        "ExternalId",
    ];

    internal static async Task Start(string file, string subscriptionId, string? connectionString, Uri? endpoint)
    {
        using TextReader textReader = new StreamReader(file);

        using CsvReader csvReader = new(textReader, System.Globalization.CultureInfo.InvariantCulture);

        csvReader.Read();
        csvReader.ReadHeader();

        string[] headers = csvReader.HeaderRecord;

        if (!RequiredHeaders.All(headers.Contains))
        {
            throw new InvalidOperationException("Missing required headers");
        }

        var records = csvReader.GetRecords<dynamic>();

        TableClient tableClient = endpoint is not null ? new(endpoint, TableName, new DefaultAzureCredential()) : new(connectionString, TableName);

        foreach (var record in records)
        {
            var dictionary = (IDictionary<string, object>)record!;

            string rowKey = $"{subscriptionId}|{dictionary["ConnectorId"]}|{dictionary["ExternalId"]}";

            string partitionKey = subscriptionId;

            var tableRow = new Azure.Data.Tables.TableEntity(partitionKey, rowKey);

            foreach (var header in headers)
            {
                var value = dictionary[header];

                tableRow.Add(header, value);

                Console.Write(tableRow.GetString(header) + "\t");
            }
            Console.WriteLine();

            await tableClient.UpsertEntityAsync(tableRow);


        }
    }
}
