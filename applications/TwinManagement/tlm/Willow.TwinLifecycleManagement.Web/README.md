# Twin Lifecycle Management
Twin Lifecycle Management is a client environment extension providing a way to manage twins and models in a particular ADT instance, as well as exposing various maintenance features. This extension aims to supersede the DigitalTwinImporter console application.

## Current Development Status
v1.

## Architecture
Twin Lifecycle Management consists of two components: a front-end providing user interface and a back-end managing the actual process of managing data in the Azure Digital Twins instance. The application is deployed in a particular client environment and has access to the resources of that environment only.

The application depends on the Azure Active Directory for authentication and Willow Platform (Willow.AzureDigitalTwins.Api instance specifically) which is used to perform the actual data management jobs over a customer's Azure Digital Twins instance.

## Running & Deploying
For local startup, see the [Building and running the TLM extension locally](https://willow.atlassian.net/wiki/spaces/WCP/pages/2110194009/Building+and+running+the+TLM+extension+locally) article.

For deployments, see the [Deploying the TLM extension to the dev/prod environments](https://willow.atlassian.net/wiki/spaces/WCP/pages/2110488926/Deploying+the+TLM+extension+to+dev+prod+environments) article.
