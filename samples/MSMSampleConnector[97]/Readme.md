# Microsoft Sustainability Manager First Party Logic App Connector

## Overview

This sample project is to help Microsoft Partners get started developing their first party connector [Logic App](https://learn.microsoft.com/en-us/azure/logic-apps/logic-apps-overview) for [Microsoft Sustainability Manager (MSM)](https://learn.microsoft.com/en-us/industry/sustainability/sustainability-manager-overview). This connector will allow customers of both the Partner and MSM to approach MSM, discover that the Partner has a first party connection path established to MSM and with limited inputs begin to sync data from the Partner into their MSM instance.

The first party logic app connector is developed with, and then ultimately hosted by, the MSM team. Onboarding this connector will start with you, the Partner, developing a LogicApp which will communicate back to your customer facing endpoints and transform the data to match MSM's schema if not already in that form. If you already have similar information surfaced to customers then the LogicApp workflow may be become more complex. After you have established the starting code for the LogicApp workflow you will need to work with the MSM team to share the code and establish livesite and disaster recovery protocols.

## Prerequisites

- An Azure Subscription

## Samples

### MSMConnectorApi

This sample is a potential framework that can be used to surface PurchasedEnergy which has been aggregated and persisted. This API demonstrates some best practices to support the connector such as pagination and versioning as well as decoupling data model contracts. To aid development there is also an endpoint for generating some sample data to a backend data model contract if the data aggregation process is being worked on in parallel.

The architecture in this sample uses a Azure Storage Account Table Store to persist the data. This is not a requirement, only one of the many options Azure provides. You may use whatever storage you deem appropriate for your larger architecture story.

#### Deploy

In the Azure Portal create:

- A StorageAccount with an `msmconnector` table
- A KeyVault
- [Optional] An ApplicationInsights

Via VisualStudio:

- Update the `.\MSMConnectorApi\src\appsettings.json` to point to your generated resources.
- Publish the API to an AppService by right clicking on the project and selecting publish.

> [!NOTE]
> The sample code does not use Azure Key Vault at this time, but it would be a good location for extending the sample TLS certificates.

### MSMLogicApp

This sample is a starting point for the first party connector. The inputs for this connector are sent via Http trigger from the Customer's MSM instance. This Http trigger contains details which the customer has provided during their setup of the connector (custom endpoint, identity, etc.) as well as any multi-tenant configuration the Partner has provided to MSM before the existence of any customers (for example: all customers in RegionA talk to this rootEndpoint or that token provider).

To manage the volume of data this logic app demonstrates, in tandem with the MSMConnectorApi sample, paginating through results and mapping from the customer data model contract to MSM's data model contract for their internal APIs. It also takes care to protect any secrets or tokens from leaking by setting the appropriate policies.

#### Deploy

Run `.\MSMLogicApp\deploy.ps1 -subscription <subscriptionName or Id> -location <region> -connectorName <name of the logicApp> -tenantId <AAD Tenant for where the logic app is deployed> [-customerTenantId <AAD Tenant for permissions to access MSMConnectorApi>]`

#### Running

1. Go to Azure Portal
1. Go to your successfully deployed LogicApp
1. Click `Logic app designer`
1. Click `Run Trigger`
1. Click `Run with payload`
1. Populate the `Body` text field with the contents of `.\MSMLogicApp\Bicep\sampleBody.json`
1. Click `Run`
1. Click `View monitoring view`

From here you will see a collapsed version of your logicApp connector. You can click on any of the actions to expand and inspect their respective input and output values as pictured below. To run again hit the `X` in the upper right corner and start over with `Step 4`. If you have deployed new LogicApp code you must navigate way from `Logic app designer` and return to `Step 3`.

![A picture showing a successful run of the LogicApp](Assets/SuccessfullyRunLogicApp.png)
