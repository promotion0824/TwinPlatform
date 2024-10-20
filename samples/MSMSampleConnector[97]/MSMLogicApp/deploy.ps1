param (
    [Parameter(Mandatory=$true)] [String]$subscription,
    [Parameter(Mandatory=$true)] [String]$location,
    [Parameter(Mandatory=$true)] [String]$connectorName,
    [Parameter(Mandatory=$true)] [String]$tenantId,
    [Parameter(Mandatory=$false)] [String]$customerTenantId = $tenantId,

    [Parameter(Mandatory=$false)] [Switch]$skipCommonInfra = $false
)
Set-StrictMode -Version latest
$ErrorActionPreference = 'Stop'
$root = Split-Path $(Split-Path $PSScriptRoot -Parent) -Parent

$resourceGroupName = "msm-connector"

Write-Output "Make sure we are logged in to Azure"
try {
    $userPrincipalId = az ad signed-in-user show --query id --output tsv --only-show-errors

    if (!$userPrincipalId) {
        Write-Output "User not logged in. Logging in"
        az login --tenant $tenantId --only-show-errors
        $userPrincipalId = az ad signed-in-user show --query id --output tsv --only-show-errors
    }
    else {
        Write-Output "User already logged in."
    }
}
catch {
    Write-Output "Error getting user id. Trying to log in again."
    az login --tenant $tenantId --only-show-errors
    $userPrincipalId = az ad signed-in-user show --query id --output tsv --only-show-errors
}

Write-Output "Switching to Subscription $subscription"
az account set --subscription $subscription

Write-Output "Creating the Resource Groups"
$rg = (az group create --location $location --name $resourceGroupName) | ConvertFrom-Json
Write-Output "-> $($rg.name)"

if($skipCommonInfra) {
    Write-Output "Getting LogicApp resources"
    $common = (az deployment group show --resource-group $resourceGroupName --name common) | ConvertFrom-Json
} 
else {
    Write-Output "Deploying LogicApp resources"
    $common = (az deployment group create --template-file "$root\products\MsmLogicApp\Bicep\logicApp.bicep" -g $resourceGroupName `
                            --parameters name=$connectorName `
                                         location=$location `
                                         tenantId=$customerTenantId
                            ) | ConvertFrom-Json
}
Write-Output "-> $($common.properties.outputs.logicApp.value)"

write-host "Complete!"
write-host "<('_'<) <('_')> (>'_')>"
