# Run once for non-docker local development

setx WillowContext__CustomerInstanceConfiguration__CustomerInstanceName "local"
setx COPILOT_LOG_LEVEL "DEBUG"
setx COPILOT_DEBUG_ENABLE_LOCAL_APPINSIGHTS "False"

setx Image "copilot:0.0000"
setx DeploymentTime "2024-06-17T20:11:34Z"

# Secrets
setx AZURE_OPENAI_API_KEY "SECRET"

# Only needed for local testing
setx VECTOR_STORE_APIKEY "SECRET"

rem setx LANGCHAIN_API_KEY "SECRET"
setx COPILOT_DEBUG "True"
setx COPILOT_SERVER_PORT "8080"
setx COPILOT_INDEXING_NUMTHREADS "1"

setx FORCE_REINDEX_LASTUPDATETIME "2024-06-11T08:58:14.781066"
setx FORCE_SUMMARY_LASTUPDATETIME "2024-06-11T08:58:14.781066"
setx SUMMARY_GENERATION_ENABLED "true"
setx INDEX_ON_STARTUP "false"

setx AZURESEARCH_FIELDS_ID "Id"
setx AZURESEARCH_FIELDS_CONTENT "Content"
setx AZURESEARCH_FIELDS_CONTENT_VECTOR "ContentVector"

rem Config for EUS shared
setx AZURE_OPENAI_ENDPOINT "https://oai-dev-eus-01-shared.openai.azure.com/"
setx CHAT_DEPLOYMENT_NAME "test-dse-gpt-35-turbo-16k"
setx VECTOR_EMBEDDINGS_DEPLOYMENT_NAME "test-dse-ada-002"

rem for canada shared
setx AZURE_OPENAI_ENDPOINT "https://oai-dev-cne-01-shared.openai.azure.com/"
setx CHAT_DEPLOYMENT_NAME_SMALL "copilot-gpt-35-turbo-16k"
setx CHAT_DEPLOYMENT_NAME_LARGE "test-copilot-gpt-4-32k"
setx VECTOR_EMBEDDINGS_DEPLOYMENT_NAME "copilot-text-embedding-ada-002"

rem for DEV
setx VECTOR_STORE_ADDRESS "https://search-doc-poc.search.windows.net"
setx VECTOR_INDEX_NAME "documents-dev"

rem for CI:
setx VECTOR_STORE_ADDRESS "https://srch-dev-eus-01-wil-in1.search.windows.net"
setx VECTOR_STORE_NAME "srch-dev-eus-01-wil-in1" 
setx VECTOR_INDEX_NAME "documents-wil-dev"
setx ApplicationInsights__AuditConnectionString "InstrumentationKey=4182639d-fe1b-4388-b944-90669055bd5d;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/"
setx ApplicationInsights__ConnectionString "InstrumentationKey=e8569e75-a08e-4a73-b8a0-ed6191a9be7f;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/;ApplicationId=8b7abf71-0522-45d5-9361-a976879507dc"

setx BlobStorage__AccountName "stodeveus01wili3c46beea"
setx BlobStorage__ContainerName "twindocuments"

setx COPILOT_RUNFLAGS "LLM-Small,CitationsMode:off"