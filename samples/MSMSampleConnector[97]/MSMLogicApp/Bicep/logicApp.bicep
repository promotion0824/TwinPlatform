param name string
param location string

// MSM Team will deploy this template with the customer's TenantId
param tenantId string

resource logicApp 'Microsoft.Logic/workflows@2019-05-01' = {
  name: name
  location: location
  properties: {
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json'
      triggers: {
        http_request: {
          type: 'request'
          kind: 'http'
          inputs: {
            schema: {
              '$schema': 'http://json-schema.org/draft-04/schema#'
              type: 'object'
              properties: {
                // Yours to play with - Customer Preconfigured Input - If they're not needed they can be removed
                instanceName: {
                  type: 'string' // msm-connector-api
                }
                clientId: {
                  type: 'string' // hidden by the trigger's runtimeConfiguration.secureData settings
                }
                clientSecret: {
                  type: 'string' // hidden by the trigger's runtimeConfiguration.secureData settings
                }
                // Yours to play with - Partner Preconfigured Input - If they're not needed they can be removed
                config: {
                  // Currently empty but partner can preconfigure static variables consistent across customers
                  // for a given region or other multi-tenant consistent configurations. This config object
                  // stored with the MSM team will be passed in with each trigger.
                  type: 'object'
                }
                //Given to you - Relevant
                correlationId: {
                  type: 'string' // LogId - Add to headers for troubleshooting
                }
                lastRefreshTime: {
                  type: 'string' // Last time the PurchasedEnergy sync was run successfully
                }
                //Given to you - Irrelevant to partner today
                refreshId: {
                  type: 'string' // RunId
                }
              }
              required: [
                'lastRefreshTime'
                'instanceName'
                'clientId'
                'clientSecret'
                'tenantId'
              ]
            }
          }
          operationOptions: 'EnableSchemaValidation'
          runtimeConfiguration: {
            concurrency: {
              runs: 1
            }
            secureData: {
              properties: [
                'inputs'
                'outputs'
              ]
            }
          }
        }
      }
      actions: {
        InitializeContinuationToken: {
          type: 'InitializeVariable'
          inputs: {
            variables: [
              {
                name: 'ContinuationToken'
                type: 'string'
                value: '@{null}'
              }
            ]
          }
        }
        try: {
          runAfter: {
            InitializeContinuationToken: [
              'Succeeded'
            ]
          }
          type: 'Scope'
          actions: {
            // Get Token with Customer provided credentials to communicate with Partner
            GetToken: {
              type: 'Http'
              inputs: {
                method: 'POST'
                uri: '${environment().authentication.loginEndpoint}/${tenantId}/oauth2/token'
                headers: {
                  'Content-Type': 'application/x-www-form-urlencoded'
                }
                body: 'client_id=@{triggerBody()?.clientId}&client_secret=@{triggerBody()?.clientSecret}&grant_type=client_credentials&response_type=token'
              }
              runtimeConfiguration: {
                secureData: {
                  properties: [
                    'inputs'
                    'outputs'
                  ]
                }
              }
            }
            // Work your way through the paginated results
            UntilTheContinuationTokenIsNull: {
              runAfter: {
                GetToken: [
                  'Succeeded'
                ]
              }
              actions: {
                // Call your endpoint which knows how to access a customer's PurchasedEnergy storage
                CallPartnerEndpoint: {
                  type: 'Http'
                  inputs: {
                    method: 'POST'
                    uri: 'https://@{triggerBody()?.instanceName}.azurewebsites.net/api/purchasedEnergy?api-version=2022-12-04'
                    headers: {
                      Authorization: '@{concat(\'Bearer \', body(\'GetToken\')?.access_token)}' // hidden by the GetToken's runtimeConfiguration.secureData settings
                      'Content-Type': 'application/json'
                      'correlation-id': '@{triggerBody()?.correlationId}' // Optional to add for tracability purposes so MSM Team and Partner Team can trace a transaction
                    }
                    body: {
                      StartDate: '@{triggerBody()?.lastRefreshTime}'
                      ContinuationToken: '@{variables(\'ContinuationToken\')}'
                      MaxNumberOfItems: 100 //TODO: Microsoft Sustainability Connector Team to confirm volume of data they can support per batch.
                    }
                  }
                }
                DetermineIfMoreDataIsAvailable: {
                  runAfter: {
                    CallPartnerEndpoint: [
                      'Succeeded'
                    ]
                  }
                  type: 'SetVariable'
                  inputs: {
                    name: 'ContinuationToken'
                    value: '@{body(\'CallPartnerEndpoint\').continuationToken}'
                  }
                }
                // If needed transform partner endpoint response into MSM PurchasedEnergy Schema
                AlignToMSMConnectorSchema: {
                  runAfter: {
                    DetermineIfMoreDataIsAvailable: [
                      'Succeeded'
                    ]
                  }
                  type: 'Select'
                  inputs: {
                    from: '@array(body(\'CallPartnerEndpoint\')?.data)'
                    select: {
                      msdyn_consumptionstartdate: '@{item()?.consumptionStartDate}'
                      msdyn_consumptionenddate: '@{item()?.consumptionEndDate}'
                      msdyn_cost: '@{item()?.cost}'
                      msdyn_costunit: '@{item()?.costUnit}'
                      msdyn_dataqualitytype: '@{item()?.dataQualityType}'
                      msdyn_energyprovidername: '@{item()?.energyProviderName}'
                      msdyn_energytype: '@{item()?.energyType}'
                      msdyn_facilityid: '@{item()?.facility}'
                      msdyn_isrenewable: '@{item()?.isRenewable}'
                      msdyn_name: '@{item()?.name}'
                      msdyn_organizationalunitid: '@{item()?.organizationalUnit}'
                      msdyn_quantity: '@{item()?.quantity}'
                      msdyn_quantityunit: '@{item()?.quantityUnit}'
                      msdyn_description: '@{item()?.description}'
                      msdyn_evidence: '@{item()?.evidence}'
                      msdyn_contractualinstrumenttypeid: '@{item()?.contractualInstrumentType}'
                      msdyn_origincorrelationid: '@{item()?.originCorrelationId}'
                      msdyn_transactiondate: '@{item()?.transactionDate}'
                      msdyn_meternumber: '@{item()?.meterNumber}'
                    }
                  }
                }
                // TODO: Microsoft Sustainability Connector Team to fill out - Forwards data you've properly formatted onto their internal Api.
              }
              expression: '@equals(body(\'CallPartnerEndpoint\')?.continuationToken, null)'
              limit: {
                // Try to accomplish paging through dataset within an hour and 1000 pages
                count: 1000
                timeout: 'PT1H'
              }
              type: 'Until'
            }
          }
        }
        catch: {
          runAfter: {
            try: [
              'TimedOut'
              'Skipped'
              'Failed'
            ]
          }
          type: 'Scope'
          actions: {
            // TODO: Microsoft Sustainability Connector team to fill out - Handles transaction details to retry or alert OnCall.
          }
        }
      }
    }
  }
}

output logicApp object = logicApp
