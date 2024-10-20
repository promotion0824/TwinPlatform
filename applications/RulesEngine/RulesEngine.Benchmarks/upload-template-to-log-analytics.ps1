# Adapted from: https://docs.microsoft.com/en-us/azure/azure-monitor/logs/data-collector-api
# More notes here: https://willow.atlassian.net/wiki/spaces/RulesEngine/pages/2187690184/Automated+Performance+Testing+in+Azure+Pipelines+Benchmarkdotnet+Baselines

# Get variables as input from the job in the pipeline, which makes use of pipeline variable groups to store values
param ($CustomerId, $SharedKey, $SasKey)

###Replace with your Workspace ID
###$CustomerId = "afbe66ef-d11a-4875-ac45-b1ef26b87441"  

###Replace with your Primary Key
###$SharedKey = "0ZX7U9Rw6Du1yAPDP40egkXLs/YdUO8bqHC7zg24E+INm1+DlKu+p+YulGPKbtmbz7FOm3HVcVJXFJzkBdmBwg=="

# Specify the name of the record type that you'll be creating - this is the table we read from in the Grafana dashboard in the particular log analytics workspace
$LogType = "DotNetBenchmarks1"

# Optional name of a field that includes the timestamp for the data. If the time field is not specified, Azure Monitor assumes the time is the message ingestion time
$TimeStampField = ""

# The next step reads from a SAS container in Azure blob storage.
# The hidden key ($SasKey) is stored in the pipeline and set in the azure portal on the container.
# The container currently being used: https://portal.azure.com/#@willowinc.com/resource/subscriptions/249312a0-4c83-4d73-b164-18c5e72bf219/resourceGroups/performance-testing-rsg/providers/Microsoft.Storage/storageAccounts/dotnetbenchmarks/overview

$json = Invoke-WebRequest -Uri "https://dotnetbenchmarks.blob.core.windows.net/benchmark-results/RulesEngine.Benchmarks.TemplateBenchmarks-report.json$SasKey"

#$json = @"
#[
#    {
#    }
#]
#"@

# Create the function to create the authorization signature
Function Build-Signature ($customerId, $sharedKey, $date, $contentLength, $method, $contentType, $resource)
{
    $xHeaders = "x-ms-date:" + $date
    $stringToHash = $method + "`n" + $contentLength + "`n" + $contentType + "`n" + $xHeaders + "`n" + $resource

    $bytesToHash = [Text.Encoding]::UTF8.GetBytes($stringToHash)
    $keyBytes = [Convert]::FromBase64String($sharedKey)

    $sha256 = New-Object System.Security.Cryptography.HMACSHA256
    $sha256.Key = $keyBytes
    $calculatedHash = $sha256.ComputeHash($bytesToHash)
    $encodedHash = [Convert]::ToBase64String($calculatedHash)
    $authorization = 'SharedKey {0}:{1}' -f $customerId,$encodedHash
    return $authorization
}

# Create the function to create and post the request
Function Post-LogAnalyticsData($customerId, $sharedKey, $body, $logType)
{
    $method = "POST"
    $contentType = "application/json"
    $resource = "/api/logs"
    $rfc1123date = [DateTime]::UtcNow.ToString("r")
    $contentLength = $body.Length
    $signature = Build-Signature `
        -customerId $customerId `
        -sharedKey $sharedKey `
        -date $rfc1123date `
        -contentLength $contentLength `
        -method $method `
        -contentType $contentType `
        -resource $resource
    $uri = "https://" + $customerId + ".ods.opinsights.azure.com" + $resource + "?api-version=2016-04-01"

    $headers = @{
        "Authorization" = $signature;
        "Log-Type" = $logType;
        "x-ms-date" = $rfc1123date;
        "time-generated-field" = $TimeStampField;
    }

    $response = Invoke-WebRequest -Uri $uri -Method $method -ContentType $contentType -Headers $headers -Body $body -UseBasicParsing
    return $response.StatusCode

}

# Submit the data to the API endpoint
Post-LogAnalyticsData -customerId $customerId -sharedKey $sharedKey -body ([System.Text.Encoding]::UTF8.GetBytes($json)) -logType $logType