Param
(
    [string]
    $filePath,

    [string]
    $customerId,

    [string]
    $sharedKey

)
$ErrorActionPreference = "Stop"

# Adapted from https://gist.github.com/taddison/d49bd8c6f7fc1d45aa8e7b0906c180ae
# Adapted from https://docs.microsoft.com/en-us/azure/log-analytics/log-analytics-data-collector-api
Function Get-LogAnalyticsSignature {
    [cmdletbinding()]
    Param (
        $customerId,
        $sharedKey,
        $date,
        $contentLength,
        $method,
        $contentType,
        $resource
    )
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

Function Export-LogAnalytics {
    [cmdletbinding()]
    Param(
        $customerId,
        $sharedKey,
        $object,
        $logType,
        $TimeStampField
    )
    $bodyAsJson = ConvertTo-Json $object
    $body = [System.Text.Encoding]::UTF8.GetBytes($bodyAsJson)

    $method = "POST"
    $contentType = "application/json"
    $resource = "/api/logs"
    $rfc1123date = [DateTime]::UtcNow.ToString("r")
    $contentLength = $body.Length

    $signatureArguments = @{
        CustomerId = $customerId
        SharedKey = $sharedKey
        Date = $rfc1123date
        ContentLength = $contentLength
        Method = $method
        ContentType = $contentType
        Resource = $resource
    }

    $signature = Get-LogAnalyticsSignature @signatureArguments
    
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


$data = Get-Content -path $filePath | ConvertFrom-Csv

$BuildDefinition = "$($env:SYSTEM_DEFINITIONNAME)/$($env:SYSTEM_DEFINITIONID)/$($env:SYSTEM_PHASEDISPLAYNAME)"
$Run = "$($env:BUILD_BUILDNUMBER)/$($env:SYSTEM_PHASEATTEMPT)"


$data | ForEach-Object {
    $TimeGenerated=(Get-Date 01.01.1970)+([System.TimeSpan]::fromseconds($_.timestamp))
    Add-Member -InputObject $_ -NotePropertyName BuildDefinition -NotePropertyValue $BuildDefinition
    Add-Member -InputObject $_ -NotePropertyName Run -NotePropertyValue $Run
    Add-Member -InputObject $_ -NotePropertyName TimeGenerated -NotePropertyValue $TimeGenerated

}

##https://stackoverflow.com/questions/13888253/powershell-break-a-long-array-into-a-array-of-array-with-length-of-n-in-one-line
Function Split-Every($list, $count=4) {
    $aggregateList = @()

    $blocks = [Math]::Floor($list.Count / $count)
    $leftOver = $list.Count % $count
    for($i=0; $i -lt $blocks; $i++) {
        $end = $count * ($i + 1) - 1

        $aggregateList += @(,$list[$start..$end])
        $start = $end + 1
    }    
    if($leftOver -gt 0) {
        $aggregateList += @(,$list[$start..($end+$leftOver)])
    }

    $aggregateList    
}

$chunks = Split-Every $data 500


$logAnalyticsParams = @{
    CustomerId = $customerId
    SharedKey = $keys.primarySharedKey
    TimeStampField = "TimeGenerated"
    LogType = "K6Test"
}
foreach ($chunk in $chunks) {
    Export-LogAnalytics @logAnalyticsParams $chunk    
}


