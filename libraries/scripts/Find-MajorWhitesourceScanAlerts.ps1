[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $UserKey,

    [Parameter()]
    [string]
    $ProjectToken,

    [Parameter()]
    [string]
    $ExcludedArtifactsCsv,

    [Parameter()]
    [string]
    $ProjectName,

    [Parameter(Mandatory = $false)]
    [string]
    $SystemAccessToken,

    [Parameter(Mandatory = $false)]
    [int]
    $CheckIntervalSeconds = 15,

    [Parameter(Mandatory = $false)]
    [int]
    $CheckTimes = 40
)

. $PSScriptRoot/AzDoHelpers.ps1

function Get-ApiResponse {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]
        $UserKey,

        [Parameter(Mandatory = $false)]
        [string]
        $ProjectToken,

        [string]
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [ValidateSet("getProjectAlerts", "getProjectState", "getProjectInventory")]
        $Command
    )

    $url = "https://saas.whitesourcesoftware.com/api/v1.3"
    $body = @{
        requestType = $Command
        userKey = $UserKey
        projectToken = $ProjectToken
    }
    $jsonBody = ConvertTo-Json $body
    $response = Invoke-WebRequest $url -Method Post -Body $jsonBody -ContentType "application/json" -UseBasicParsing
    $data = $response.Content | ConvertFrom-Json
    return $data
}
$SecurityVulnerabilityType = "SECURITY_VULNERABILITY"
$PolicyViolationType = "REJECTED_BY_POLICY_RESOURCE"

# In the last update was more than the check interval period in the past then wait for any scan to update in whitesource first
$inventory = Get-ApiResponse -UserKey $UserKey -ProjectToken $ProjectToken -Command "getProjectInventory"
$lastUpdate = [DateTime]::Parse($inventory.projectVitals.lastUpdatedDate)
Write-Verbose "Whitesource update was last done at $lastUpdate"
if ($($lastUpdate - [DateTime]::UtcNow).TotalSeconds -lt -$CheckIntervalSeconds){
    Write-Verbose "Waiting $CheckIntervalSeconds for any new updates"
    Start-Sleep -s $CheckIntervalSeconds
    $inventory = Get-ApiResponse -UserKey $UserKey -ProjectToken $ProjectToken -Command "getProjectInventory"
    $lastUpdate = [DateTime]::Parse($inventory.projectVitals.lastUpdatedDate)
    Write-Verbose "Whitesource update was last done at $lastUpdate"
}

# Check if the update is still in progress with alerts before proceeding
$state = Get-ApiResponse -UserKey $UserKey -ProjectToken $ProjectToken -Command "getProjectState"
Write-Verbose $(ConvertTo-Json $state)
$checks = 1
while ($state.projectState.inProgress -and $checks -le $CheckTimes) {
    Write-Verbose "Whitesource scan was still in progress sleeping for $CheckIntervalSeconds for  $checks of $CheckTimes times"
    Start-Sleep -s $CheckIntervalSeconds
    $state = Get-ApiResponse -UserKey $UserKey -ProjectToken $ProjectToken -Command "getProjectState"
    $checks ++
}

$allAlerts = Get-ApiResponse -UserKey $UserKey -ProjectToken $ProjectToken -Command "getProjectAlerts" | Select-Object -expand alerts

$high = $allAlerts | Where-Object {($_.type -eq $SecurityVulnerabilityType -and $_.vulnerability.severity -eq "high") -or $_.type -eq $PolicyViolationType }

$exclusions = $ExcludedArtifactsCsv.split(",") | Where-Object { $_ -ne "" }

$newAlerts = $high | Where-Object { $exclusions -notcontains $_.library.artifactId}

foreach ($alert in $newAlerts) {
    if ($alert.type -eq $SecurityVulnerabilityType)
    {
        Write-Verbose "$($alert.library.name) (artifactId: $($alert.library.artifactId)) has a high security issue to fix $($alert.vulnerability.fixResolutionText)"
    }
    if ($alert.type -eq $PolicyViolationType)
    {
        Write-Verbose "$($alert.library.name) (artifactId: $($alert.library.artifactId)) has been rejected by policy $($alert.description)"
    }
}

$highMsgs = $newAlerts | Where-Object {$_.type -eq $SecurityVulnerabilityType} | ForEach-Object { "* **$($_.library.name)** ($($_.library.version)) Requires $($_.vulnerability.fixResolutionText) `r`n   * [$($_.vulnerability.description)]($($($_.vulnerability.url))) - $($_.vulnerability.publishDate)" }
$policyMsgs = $newAlerts | Where-Object {$_.type -eq $PolicyViolationType} | ForEach-Object { "* **$($_.library.name)** ($($_.library.version)) violates policy $($_.description) `r`n"}

$Title = ":unlock: Whitesource Status for project ($ProjectName) :fire_engine:"

$markdownComment = @"
$Title
See more details at [Whitesource](https://saas.whitesourcesoftware.com/Wss/WSS.html)
$(if ($highMsgs.Count -gt 0){"## Please fix these Security Vulnerabilities as they are high severity"} else {""})

$($highMsgs -join "`r`n")

$(if ($policyMsgs.Count -gt 0){"## Please remove these libraries as they are rejected by policy"} else {""})

$($policyMsgs -join "`r`n")

## All Issues detected by Whitesource
| Issue Type | Count |
|------------------|----|
$($($allAlerts.Type | Where-Object {$_ -ne "MULTIPLE_LICENSES"} | Group-Object | ForEach-Object {"|$($_.Name) | $($_.Count)|"}) -join "`r`n")

$(if ($exclusions.Count -gt 0){ "## These Vulnerabilities have previously been excluded remove the exclusion if not required"} else {""})
$($($exclusions | ForEach-Object { "* $_"}) -join "`r`n")
"@

Write-Verbose "Posting PR Comment via AzureDevOps REST API"

$body = @{
    "comments" = @(
        @{"parentCommentId" = 0;
            "commentType"   = 1;
            "content"       = $markdownComment
        }
    );
    "status"   = if ($newAlerts.Count -gt 0){"active"} else {"closed"}
} | ConvertTo-Json

# post to the PR
Remove-PRComment -TextToMatchOn $Title -SystemAccessToken $SystemAccessToken
Add-PRComment -Body $body -SystemAccessToken $SystemAccessToken


# Write an error out to fail the build
if ($newAlerts.Count -gt 0){
    Write-Error "Failed because of having $($newAlerts.Count) high security vulnerability or policy violation in WhiteSource" -ErrorAction Stop
}
