# pylint: disable=broad-exception-caught.
from typing import ( List, Optional )

import os
import re
from dataclasses import dataclass
from urllib import response

from attr import field
from azure.identity import DefaultAzureCredential
from azure.identity import ChainedTokenCredential, ManagedIdentityCredential, AzureCliCredential
from langchain_core.vectorstores import VectorStoreRetriever

from .OpenTelemetry import log_exception, log_info, log_debug, log_warning, log_error
from .HealthCheck import CopilotHealthChecks
from .Utils import timed, DebugMode

# from langchain.embeddings.azure_openai import AzureOpenAIEmbeddings
# from langchain.chat_models.azure_openai import AzureChatOpenAI
from langchain_openai import AzureChatOpenAI, AzureOpenAIEmbeddings
from langchain_community.vectorstores.azuresearch import AzureSearch, AzureSearchVectorStoreRetriever
from langchain_community.retrievers import AzureCognitiveSearchRetriever
from langchain.schema import HumanMessage
from azure.search.documents import SearchClient

from azure.search.documents.indexes import SearchIndexClient
from shared.indexing.SearchIndexConfig import IndexFields

DefaultTemperatureModelSmall = 0.1 # default is 1.0 for GPT3.5
DefaultTemperatureModelLarge = 0.2 # default is 0.7 for GPT4

# Note: For langchain integration use azure.search.docuents==11.4.0b8
# don't update to latest until missing Vector fixed:
#   https://github.com/langchain-ai/langchain/discussions/13245
#   https://stackoverflow.com/questions/77462188/langchain-and-azure-cognitive-search-importerror-cannot-import-name-vector
#   https://github.com/langchain-ai/langchain/issues/7813

@dataclass
class TokenLimits:
    llm_max_tokens: int
    llm_max_usable_tokens: int
    memory_max_tokens: int
    context_max_tokens: int
    prompt_max_tokens: int
    request_max_tokens: int
    response_max_tokens: int


class ServicesWrapper:
    """ Convenience wrapper for all Azure AI services
    """

    def __init__(
                self, 
                throw_exceptions: bool = True, 
                in_startup: bool = False,
                index_name: str = None
                ):

        self._is_ready = False
        self._throw_exceptions = throw_exceptions
        self._in_startup = in_startup

        self._chat_deployment_name = os.environ.get("CHAT_DEPLOYMENT_NAME")
        self._chat_deployment_name_small = \
            os.environ.get("CHAT_DEPLOYMENT_NAME_SMALL") or self._chat_deployment_name
        self._chat_deployment_name_large = \
            os.environ.get("CHAT_DEPLOYMENT_NAME_LARGE") or self._chat_deployment_name
        if not self._chat_deployment_name_small:
            raise ValueError("CHAT_DEPLOYMENT_NAME(_SMALL) not set")
        if not self._chat_deployment_name_large:
            raise ValueError("CHAT_DEPLOYMENT_NAME(_LARGE) not set")

        self._embeddings_deployment_name = os.environ["VECTOR_EMBEDDINGS_DEPLOYMENT_NAME"]

        self._vector_index_name = index_name or ServicesWrapper.get_default_read_index_name()
        self._vector_store_address = os.environ["VECTOR_STORE_ADDRESS"]
        self._vector_store_name = os.environ.get("VECTOR_STORE_NAME") \
                or re.search( r"https://(.*?)\.search\.windows\.net", \
                    self._vector_store_address).group(1)

        # Below comment valid once MI is working in deployed scenario:
        # Used only for debugging locally as MI won't work in local container -
        #   VECTOR_STORE_APIKEY should be omitted when deployed
        self._vector_store_api_key = os.environ.get("VECTOR_STORE_APIKEY", None)

        self._llm_small = None
        self._llm_large = None
        self._embeddings = None
        self._vector_store = None
        self._document_retriever = None
        self._search_client = None
        self._credential = None
        self._search_token = None

        self._default_top_k_docs = 8
        self._default_search_type = "hybrid"

        # https://learn.microsoft.com/en-us/azure/search/search-howto-aad?tabs=config-svc-portal%2Caad-dotnet
        # https://pypi.org/project/azure-search-documents/
        # os.environ["OPENAI_API_TYPE"] = "azure_ad"
        # https://python.langchain.com/docs/integrations/llms/azure_openai#azure-active-directory-authentication-
        # Must use API secret key for OpenAI at the moment
        self._credential = ChainedTokenCredential(
            ManagedIdentityCredential(),
            DefaultAzureCredential(),
            AzureCliCredential())

        # The class is designed to get these resources lazily, but we'll 
        #  get them all upfront to catch errors early on startup
        if in_startup:
            self.init_all_services()

    @staticmethod
    def get_default_create_index_name() -> str:
        # return "py-" + os.environ["VECTOR_INDEX_NAME"]
        # At this point assume that the appsettings are correct
        return os.environ["VECTOR_INDEX_NAME"]

    @staticmethod
    def get_default_read_index_name() -> str:
        return os.environ["VECTOR_INDEX_NAME"]

    # TODO: Enforcing ContextPercent and MemoryPercent only at the moment (most important)
    # TODO: pass in history and/or context value/percent and adjust other values accordingly
    def get_token_limits(self, 
                         is_large: bool,
                         memory_percent_percent:int = 100) -> TokenLimits:

        RequestTokenPercent = 10
        ResponseTokenPercent = 10
        MemoryPercent = 20
        ContextPercent = 50
        PromptPercent = 5
        OverheadPercent = 5        # Fudge-factor to make sure we never go over

        memoryPercent = memory_percent_percent * MemoryPercent // 100
        # If we don't use all of what we've alloted for memory, give it to context
        contextPercent = ContextPercent + (100 - memory_percent_percent) * MemoryPercent // 100

        llm_max_tokens = self.get_llm_max_tokens(is_large)
        max_usable_tokens = (100 - OverheadPercent) * llm_max_tokens // 100

        def percent_tokens(p:int) -> int: return max_usable_tokens * p // 100

        return TokenLimits(
            llm_max_tokens = llm_max_tokens,
            llm_max_usable_tokens = max_usable_tokens,
            memory_max_tokens = percent_tokens(memoryPercent),
            context_max_tokens = percent_tokens(contextPercent),
            prompt_max_tokens = percent_tokens(PromptPercent),
            request_max_tokens = percent_tokens(RequestTokenPercent),
            response_max_tokens = percent_tokens(ResponseTokenPercent)
        )

    @timed()
    def init_all_services(self):
        try:
            self._is_ready = \
                self.get_llm() \
                and self.get_embeddings_service() \
                and self.get_vector_store() \
                and True
        finally:
            if self._is_ready:
                CopilotHealthChecks.healthcheck_copilot.set_healthy("Initalized")
            else:
                CopilotHealthChecks.healthcheck_copilot.set_failing("Failed Init")

    def is_ready(self) -> bool:
        return self._is_ready


    # TODO: not used at the moment
    def get_search_token(self) -> str | None:

        if (self._search_token is not None): 
            return self._search_token
        try:
            log_info("Retrieving token for Azure Search")
            self._search_token = self._credential.get_token("https://search.azure.com/.default").token
            return self._search_token
        except Exception as ex:
            # May be running locally in container w/no access to service principal
            log_error(f"Error retrieving token for Azure Search: {ex}")
        return None

    def _handle_exception(self, ex: Exception):

        if self._throw_exceptions:
            log_exception(ex)
            raise ex
        else:
            log_info(f"ignoring exception: {ex}")
            return None

    def get_llm_small(self, temperature:int = None):
        if not self._llm_small:
            self._llm_small = self._get_llm( self._chat_deployment_name_small) 
        # Set temp afterward instead of c'tor in case we've cached the LLM already
        self._llm_small.temperature = temperature or DefaultTemperatureModelSmall
        return self._llm_small

    def get_llm_large(self, temperature:int = None):
        if not self._llm_large:
            self._llm_large = self._get_llm( self._chat_deployment_name_large)
        self._llm_large.temperature = temperature or DefaultTemperatureModelLarge
        return self._llm_large

    def get_llm_max_tokens(self, is_large: bool) -> int:
        # There's no way to query the model for this... - if we're consistent with 
        #  deployment names (test-dse-gpt-35-turbo-16k) we could parse it from there
        # https://community.openai.com/t/request-query-for-a-models-max-tokens/161891
        return 32_000 if is_large else 16_000

    def get_llm(self):
        return self.get_llm_small()

    def _get_llm(self, deployment_name: str) -> AzureChatOpenAI | None:

        try:
            log_info(f"Creating AzureChatOpenAI: {deployment_name}) ")
            llm = AzureChatOpenAI( 
                deployment_name = deployment_name,
                # batchSize = 10 # got an unexpected keyword argument 'batchSize'
            )
            CopilotHealthChecks.healthcheck_openai.set_healthy( "Initalized")

            if self._in_startup:
                # Make test call to LLM on init for health check
                _result = llm( [HumanMessage( content="hello")])

            return llm

        except Exception as ex:
            CopilotHealthChecks.healthcheck_openai.set_failing( "llm failed during init:")
            return self._handle_exception(ex)


    def get_embeddings_service(self) -> AzureOpenAIEmbeddings:

        if self._embeddings is None:
            try:
                log_info("Creating AzureOpenAIEmbeddings")
                self._embeddings: AzureOpenAIEmbeddings = AzureOpenAIEmbeddings(
                    deployment = self._embeddings_deployment_name,
                    chunk_size = 100,
                    # azure_ad_token = 
                )

                if self._in_startup:
                    # Make test call to generate vector embedding on init for health check
                    _vec = self._embeddings.embed_query("test")

            except Exception as ex:

                CopilotHealthChecks.healthcheck_azure_ai_search.set_failing( "embeddings failed during init")
                return self._handle_exception(ex)

        return self._embeddings


    def get_vector_store(self, index_name: str = None) -> AzureSearch:

        if self._vector_store is not None:
            #log_debug("Using pre-initialized vector store.")    
            return self._vector_store

        # TODO: temporary code to test MI in deployed scenario and fallback to api key
        throw_exceptions = self._throw_exceptions
        try:
            self._throw_exceptions = False
            if not DebugMode or self._vector_store_api_key is None:
                # If key is None langchain internally creates with MI using DefaultAzureCredentials
                self._get_vector_store(None, index_name) 

            if self._vector_store_api_key is not None:
                # Use otherwise use API key for local docker testing 
                self._get_vector_store(self._vector_store_api_key, index_name)

        finally:
            self._throw_exceptions = throw_exceptions

        if not self._vector_store:
            log_error("Failed to create AzureSearch")
        return self._vector_store


    @timed(enter_msg=False)
    def _get_vector_store(self, key = None, index_name: str = None) -> AzureSearch:

        index_name = index_name or self._vector_index_name

        try:
            log_info(f"Creating AzureSearch on index '{index_name}' with key:{key[0:4] if key else 'ManagedIdentity'}...")
            # This class uses the Azure SearchClient internally
            # Note: This langchain class will create the index if it doesn't exist --
            #  if we don't want to do this here and we still want to check for connectivity 
            #  at startup, we could use the Search(Index)Client directly
            # NOTE: The above doesn't quite work anyway as LC insists on certain fieldnames
            #  like a generic "metadata" field. 
            self._vector_store = AzureSearch(
                azure_search_endpoint = self._vector_store_address,
                azure_search_key = key,
                fields = IndexFields, # must pass in or bare/default index will be cerated
                index_name = index_name,
                embedding_function = self.get_embeddings_service().embed_query,
            )
            if self._in_startup:
                log_debug("Calling vector store search on startup")
                # On startup on, quick call to find documents for health check
                _docs = self._vector_store.similarity_search(
                    query = "test", k = 1, search_type = "similarity"
                    # filter = {...}
                )

            CopilotHealthChecks.healthcheck_azure_ai_search.set_healthy( "Initalized")
            return self._vector_store

        except Exception as ex:
            CopilotHealthChecks.healthcheck_azure_ai_search.set_failing( "vector store failed during init")
            return self._handle_exception(ex)

    # Note: We don't use search_client directly at the moment as we call the vector_store thruough 
    #   langchain - using this to test managed idetity in deployed scenario

    def get_search_client(self, index_name: str = None) -> SearchClient:
        vs = self.get_vector_store(index_name)
        if not vs:
            raise Exception("Failed to get vector store")
        return vs.client

    @timed()
    def _XXX_get_search_client(self) -> SearchClient:
        
        try:
            log_info("Creating SearchClient")
            self.search_client = SearchClient(
                self._vector_store_address, 
                self._vector_index_name, 
                self._credential)
            log_info(f"Document count: {self.search_client.get_document_count()}")
            return self.search_client
        except Exception as ex:
            return self._handle_exception(ex)

    @timed()
    def get_search_index_client(self) -> SearchIndexClient:
        try:
            log_info("Creating SearchIndexClient")
            return SearchIndexClient(
                endpoint = self._vector_store_address, 
                credential = self._credential)
        except Exception as ex:
            return self._handle_exception(ex)


    """
    Retreiver should be generating a query like the one below.
    This requires that we've setup an index Vectorizer to generate the embeddings for the query -
      otherwise we could generate it ourselves and pass it in "vectors".
    "search" is only present for hybrid searches.
    Note that the actual hybrid search request allows different text for
      the index pre-search and the vector search - not sure if exposed through the langchain wrapper.
    Pre-search would favor exact matches in searchable meta-data fields without explicit filtering.

        {
        "search": "repair compressor",
        "filters: "CopilotEnabled eq true",
        "vectorQueries": [
            {
            "kind": "text",
            "text": "repair compressor",
            "k": 5,
            "fields": "ContentVector"
            }
        ] }

    Note the absolute score values of hybrid searches is not meaningful - even with scores
      approaching 1.0 for search and vector scores individually, the hybrid scores
      are low, in the 0.03 range. Relative rankings seem to be as expected.
    """

    @timed()
    # Takes ~0.5s - all in get_vector_store_document_retreiver
    def get_vector_store_document_retreiver(self, 
                                            top_k_docs: int = None, 
                                            search_type: str = None,
                                            filters: Optional[str] = None) -> AzureSearchVectorStoreRetriever:
        log_info("Creating AzureSearch Retriever")
        vector_store = self.get_vector_store()
        if vector_store is None: return None

        search_kwargs = {
            "k": top_k_docs or self._default_top_k_docs,
            # Valid values here are similarity or hybrid or semantic_hybrid.
            "search_type": search_type or self._default_search_type,
            "filters": filters
        }

        retriever = vector_store.as_retriever(
            # At this level we always want 'similarity', and use kwargs below to 
            #   pass in either 'similarity; or 'hybrid'. Note that 'keyword' isn't supported,
            #   so if we wanted to bypass the vector ranking all togther and do a 
            #   keyword/semantic search, we should use the SearchClient or AzureCognitiveSearchRetriever 
            # Valid values here are: ('similarity', 'similarity_score_threshold', 'mmr')
            #TODO: querytype=Semantic reranking - need to use custom?
            # see: https://learn.microsoft.com/en-us/azure/search/hybrid-search-overview
            search_type = "similarity",
            search_kwargs = search_kwargs
        )

        # NOTE HACK: This is to workaround an apparent recent LC bug where the search_kwargs are not 
        #  being saved in the retriever we store them in the vectorstore and then copy them 
        #  to the retriever during _get_related_documents in CustomVectorStoreRetriever
        vector_store.search_kwargs = search_kwargs

        return retriever

    def get_acs_document_retreiver(self, key = None):
        # We can use AzureCognitiveSearchRetriever, or we can pass in the VectorStore as the retriever
        # Don't know how to use MI with the former (only API key) so using the latter
        # Retreiver options here: https://python.langchain.com/docs/modules/data_connection/retrievers/
        # Works with key=None outside of container. Works with search_token locally (TBD deployed)

        if self._document_retriever is None:
            try:
                log_info(f"Creating AzureCognitiveSearchRetriever for {self._vector_store_name}")
                self._document_retriever = AzureCognitiveSearchRetriever(
                    # Where is ACSR getting the vector field name? - must be assuming only one and looking for byte[]
                    content_key = os.environ["AZURESEARCH_FIELDS_CONTENT"],
                    service_name = self._vector_store_name,
                    api_key = key,
                    index_name = self._vector_index_name,
                    top_k = self._default_top_k_docs)
                log_info("Created AzureCognitiveSearchRetriever")

            except Exception as ex:
                CopilotHealthChecks.healthcheck_openai.set_failing( "acs_doc_ret failed during init")
                return self._handle_exception(ex)

        return self._document_retriever

