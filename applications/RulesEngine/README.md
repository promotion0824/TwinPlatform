# Architecture

The Rules Engine consists of two components: a web application (RulesEngine.Web) and a back end processor (RulesEngine.Processor). Each will be deployed on a per-customer environment basis. Each was developed as a multi-tenant application for testing purposes. The two components share several Azure resources, these have also been designed to run in shared mode or with a separate instance per customer environment. This allows for cost saving in any environment where data isolation is not required, e.g. a single service bus namespace for all test instances.

The two components build into container images using the Dockerfile provided in each project directory. They communicate with each
other over Service Bus. The RulesEngine.Processor is headless; it communicates solely over Service Bus. The RulesEngine.Web component
provides the UI that the Smart Buildings team uses to create and manage rules. It also provides a Swagger endpoint.

# Dependencies

* Azure Digital Twins (ADT)
* Azure Data Explorer (ADX)
* Azure Service Bus
* Azure Application Insights
* Azure ADB2C

# Dependency configuration

The Azure dependencies are configured using appsettings.json, customer-environments.json and any environment variable overrides. Environent variables are prefixed with `RULES_` so, for example:

````
    - "RULES_CUSTOMER__Id=wil-prd-lda-met"
    - "APPLICATIONINSIGHTS__INSTRUMENTATIONKEY=e38e9002-7b83-4a28-9eea-6e05a6412bfa"
````

Note tha application insights is not prefixed and that there is a single underscore double underscore on all the RULES_ settings.

The configuration assumes a single ADX instance per customer environment but can use multiple ADT instances.

Details on specific Azure resource requirements are listed below.

## Azure Service Bus

This currently has two topics: `rule-execution` and `rule-execution-response`. The first topic requires _exactly-once_ execution whereas the second topic is more of a broadcast of execution state. To allow for sharing of the service bus namespace between customer environments (e.g. to save costs in a test environment) the `rule-execution` topic has multiple subscribers, one per customer environment, named after the customer environment and each of these subscriptions filters messages to just the ones intended for that customer environment using a Correlation Filter with `willowEnvironmentId == {name of environment}`.

## Azure SQL Database

Like Service bus this can also be a single shared SQL Server (for cost saving) or one-database-per environment.

# Structure

_All of the structure is in a state of flux, classes may move between projects at any time and projects may be added or consolidated._

*RulesEngine Web* is the UI for the Smart Buildings team as a ReactJS application.

*RulesEngine Processor* is the rules engine itself that runs the rules.

*WillowExpressions* is a library from Ian that contains the Expression parser.

*WillowExpressions.Test* really needs some unit tests.

*WillowRules* contains the `RulesService` which creates `RuleInstances` from `Rules`.

# Authentication to Azure Resources

The plan is to use managed identities throughout. Running locally the docker-compose file maps the VSCode credential store into the container so that it can run under the user's AD credentials. Due to issues with managed identities and permissions, some of the resources are stll configured using SASS tokens. This will change.

# User authentication to RulesEngine Web

RulesEngine Web uses ADB2C to authenticate users. ADB2C flows are defined here: https://dev.azure.com/willowdev/AzurePlatform/_git/aad-b2c

# Authorization

Authorization rules are in C# code and use ASPNET policy-based AUTHORIZE attributes. Authorization code can check a user's email address and can use membership of ADB2C AD Groups to inform policy decisions.

# Data Model

[![](https://mermaid.ink/img/pako:eNp1k8tuwjAQRX9l5DX8QBaVUEFVpaJWhR3uwo0nxCK2Iz9UpYR_7-QJpGEVz_jM3GuPc2aplcgSdnSizGG_5uYzFgjL5VOdCw9amKqGJvVqfBAmxQ4YohG0BmtYpcG6XRDhEfWLzoJ1HU276piHQ_8FZVohDxtzVAa_uBl2bkSaam3dtfw9TaNz2Dq76rclqYge_Ug-8FQocyJoRTlP5W9NOLmF0iktXEXCEosaPrpw20Tc3EZjhepVRnj_o8zINkHn0CGZ9dMbnmKlcAFsVsOu8gH1SzOr6aBa-XaKNbRethjEHf_P6Nh2np_L3j2M8ZT-ztmhW0NXkNHAemUIdKDJWMv4XSifo4Rga3i2mhrLmZfR78w8ivn5TT0PNz3B2IJpdFooSf_AmRsAzkKOGjlLaCkxE7EInHFzITSWkppspKIGLMlE4XHBRAx2V5mUJcFFHKC1EjQM3VOXP1toQhE)](https://mermaid-js.github.io/mermaid-live-editor/edit/#pako:eNp1k8tuwjAQRX9l5DX8QBaVUEFVpaJWhR3uwo0nxCK2Iz9UpYR_7-QJpGEVz_jM3GuPc2aplcgSdnSizGG_5uYzFgjL5VOdCw9amKqGJvVqfBAmxQ4YohG0BmtYpcG6XRDhEfWLzoJ1HU276piHQ_8FZVohDxtzVAa_uBl2bkSaam3dtfw9TaNz2Dq76rclqYge_Ug-8FQocyJoRTlP5W9NOLmF0iktXEXCEosaPrpw20Tc3EZjhepVRnj_o8zINkHn0CGZ9dMbnmKlcAFsVsOu8gH1SzOr6aBa-XaKNbRethjEHf_P6Nh2np_L3j2M8ZT-ztmhW0NXkNHAemUIdKDJWMv4XSifo4Rga3i2mhrLmZfR78w8ivn5TT0PNz3B2IJpdFooSf_AmRsAzkKOGjlLaCkxE7EInHFzITSWkppspKIGLMlE4XHBRAx2V5mUJcFFHKC1EjQM3VOXP1toQhE)
