# pylint: disable=broad-exception-caught.

import json
from typing import Optional, List
from openai import AzureOpenAI
import yaml
import os
from dataclasses import asdict, dataclass

from langchain.prompts import PromptTemplate
from langchain.globals import set_verbose, set_debug
from langchain.callbacks import get_openai_callback
from langchain.chains.conversational_retrieval.base import ConversationalRetrievalChain, LLMChain
from langchain.chains.base import Chain
from langchain_core.runnables.config import RunnableConfig

from libs.openai.Memory import MemorySessionCacheSingleton
from shared.Prompts import CondenseQuestionPrompt, MainPrompt, DocumentChunkPrompt
from libs.openai.CustomDocRetriever import CustomHybridRetriever, CustomVectorStoreRetriever

from libs.models.ChatRequest import ChatRequest
from libs.models.ChatResponse import ChatResponse, ChatResponseDebugInfo, ChatResponseDebugInfoDocument, ChatResponseCitation

# Add access to shared packages from Copilot project root
#sys.path.append(os.path.dirname(os.path.abspath(__file__ + '/../../../')))
from shared.HealthCheck import CopilotHealthChecks
from shared.ServicesWrapper import ServicesWrapper, TokenLimits
from shared.Utils import timed, DebugMode
from shared.Citations import get_citations
from shared.indexing.IndexOps import AzureAiIndexOps
from shared.indexing.SearchIndexConfig import FieldName_ItemType, FieldName_CopilotEnabled
from shared.OpenTelemetry import (
    log_exception, log_info, log_debug, log_warning, 
    MetricChat
)

DefaultKDocs = 8
DefaultSearchType = "hybrid" # "similarity" | "hybrid" 
ChainType = "CRC" # CRC | RQA (not working)

# TODO: Create cached factory for RAG or SW based on any state/params such as index_name 
#   or remove all state - calls should be thread-safe
# Should save about 0.5s per request
# TODO: get new params object from chatReqest rather than taking as dependency (same w/resp)

class RAG:
    """
    Class to build and run the langchain chain based 
      on parameters from the chat request.

    - Generate main prompt
    - Get/Create memory based on user session
    - Create conversation chain (using docment chunk prompt)
    - Run chain using optional debug modes
    """

    def __init__(self,
                 chat_request: ChatRequest,
                 index_name: str = None,
                ):

        #self._service_wrapper = service_wrapper
        self._chat_request = chat_request
        self._chain: Chain = None
        # self._is_ready = False
        self.session_memory = None
        self._service_wrapper = None
        self._index_name = index_name

    def get_services(self) -> ServicesWrapper:
        # Set this here after init so can use memory-injected IndexName runflag
        self._index_name = self._index_name or self._chat_request.get_options().get_index_name()
        if not self._service_wrapper:
            self._service_wrapper = ServicesWrapper(index_name = self._index_name)
        return self._service_wrapper

    def get_chat_response(self) -> ChatResponse: 
        """Main entry point to answer user's question.
        Load and run the langchain chain."""

        #TODO: memory could probably always use llm_small?
        self.session_memory = MemorySessionCacheSingleton.get_memory_for(
                        self._chat_request.context.session_id)

        if self._chat_request.get_options().has_clear_flag():
            log_debug("Clearing sticky run flags")
            self.session_memory.set_sticky_runflags([])
            self._chat_request.get_options().remove_clear_flag()

        if self._chat_request.get_options().has_request_run_flags():
            log_debug(f"Setting sticky run flags: {self._chat_request.get_options().get_request_run_flags()}")
            self.session_memory.set_sticky_runflags(self._chat_request.get_options().get_request_run_flags())
        else:
            log_debug(f"Using sticky run flags from memory: {self.session_memory.get_sticky_runflags()}")
            self._chat_request.get_options().set_request_run_flags(self.session_memory.get_sticky_runflags())

        if not self._chat_request.user_input:
            return ChatResponse(responseText="<<<Flags set>>>", response_format="text/plain")

        self.load_chain()
        #if not self._is_ready: return None # TODO: needed anymore? should throw or be ok

        return self.run_chain(self._chat_request.user_input)

    @timed()
    def load_chain(self):

        if self._chain:
            return 

        services = self.get_services()

        iops = AzureAiIndexOps(services = services)
        if not iops.index_exists():
            # We allow the caller to pass in the index name for some calls.
            # Check for existence, otherise the langchain retriever will create a 
            # (bare default schema) index as a side-effect 
            raise ValueError(f"Index '{self._index_name}' does not exist")
            
        # if not self._service_wrapper.is_ready():
        #    log_warning("RAG: ServiceWrapper not ready")
        #    return False

        set_debug(DebugMode)
        set_verbose(DebugMode)

        #TODO: memory could probably always use llm_small?
        #TODO: This is always just clipping memory w/o realloting to context
        history_percent = self._chat_request.get_options().get_history_percent(100)

        token_limits = services.get_token_limits(
            self._chat_request.get_options().is_llm_large(),
            history_percent)

        # Note that the LLM used for the memory (if used by mem type) and the 
        #  conversational chain (if using ConversationRetrievalChain)
        #  do not necessarily have to be the same.
        use_large_llm = self._chat_request.get_options().is_llm_large()
        temperature = self._chat_request.get_options().get_llm_temp()
        llm = services.get_llm_large(temperature) \
                if use_large_llm \
                else services.get_llm_small(temperature)

        self.session_memory.init_memory(llm, token_limits.memory_max_tokens)

        # TODO: Other retriever types:
        # https://python.langchain.com/docs/modules/data_connection/retrievers/

        k_docs = self._chat_request.get_options().get_k_docs(DefaultKDocs)
        search_type = self._chat_request.get_options().get_search_type(DefaultSearchType) # "similarity" | "hybrid" | "keyword" (keyword throws error)
        #TODO: use this filter builder when we move of the old index (don't need "or null' check)
        #search_filters = [ (FieldName_ItemType, "docChunk"), ]
        #if True: search_filters.append((FieldName_CopilotEnabled, True))
        search_filters = "((ItemType eq 'docChunk') or (ItemType eq null))"
        if not self._chat_request.get_options().use_all_docs():
            # note "ne false" matches null or true (null for old index)
            search_filters += f" and ({FieldName_CopilotEnabled} ne false)"

        try:
            base_retriever = services.get_vector_store_document_retreiver(
                top_k_docs = k_docs, 
                search_type = search_type,
                filters = search_filters
            )

            retriever = CustomVectorStoreRetriever(
                base_retriever = base_retriever,
                max_tokens = token_limits.context_max_tokens,
                expand_chunks_percent_tokens = self._chat_request.get_options().get_expand_chunks_percent(),
                add_chunk_metadata = self._chat_request.get_options().add_chunk_metadata(),
                memory = self.session_memory.get_langchain_memory())
            #retriever = CustomHybridRetriever(self._service_wrapper)

            prompt_template = MainPrompt().get_prompt_template(self._chat_request.get_options().prompt_hints)

            # chain_type: 
            #   stuff: Add document chunks as-is
            #   | map_reduce: summarize each chunk (in parallel) 
            #   | refine: progressive summarize using LLM by adding each chunk in turn to previous summary (serial)
            #   | map_rerank: for specific Q&A call LLM with task-specific prompt to re-score each chunk (experiemental)
            if ChainType == "CRC":

                self._chain = ConversationalRetrievalChain.from_llm(
                    llm = llm,
                    memory = self.session_memory.get_langchain_memory(),
                    chain_type = "stuff",
                    retriever = retriever,
                    combine_docs_chain_kwargs = {
                        "prompt": prompt_template,
                        "document_prompt": DocumentChunkPrompt().get_prompt_template()
                    },
                    condense_question_prompt = CondenseQuestionPrompt().get_prompt_template(),
                    # condense_question_llm = # TODO: should prob always be small llm
                    return_source_documents = True,
                    return_generated_question = True,
                    #rephrase_question = False, - doesn't do anything - see NoOpLLMChain below
                    # verbose = True, debug = True
                )
                # TODO: also doesn't work
                # self._chain.question_generator = NoOpLLMChain()
                # self._chain.rephrase_question = False

            # NOTE: Does not work
            elif ChainType == "RQA":

                self._chain = RetrievalQA.from_chain_type(
                    llm = llm,
                    memory = memory,
                    chain_type = "stuff",
                    retriever = retriever,
                    # combine_docs_chain_kwargs = {"prompt": prompt},
                    chain_type_kwargs={"prompt": prompt_template},
                    return_source_documents=True,
                    #verbose = True, debug = True
                )

            else:
                raise ValueError(f"Unknown chain type: {ChainType}")   

            # If we don't fail here, keep at Starting until we get our first call - or add new Initalized status
        except Exception as ex:
            log_exception(ex)
            CopilotHealthChecks.healthcheck_openai.set_failing("Failed during chain init")
            raise

        # self._is_ready = True
        # return self._is_ready
        return


    @timed(duration_all_metric_fn = MetricChat)
    def run_chain(self, user_input: str) -> ChatResponse:
        """
        Runs the ConversationalRetrievalChain with the user input.
        """
        try:
            self.load_chain()

            instance_name = os.environ.get("WillowContext__CustomerInstanceConfiguration__CustomerInstanceName", "local")
            run_name = f"Copilot"
            # This will show up in LangSmith metadata section for the trace
            config: RunnableConfig = { 
                    "run_name": run_name,
                    "metadata": {
                        "request_options": self._chat_request.get_options().to_dict(),
                        "request_context": self._chat_request.context.to_dict(),
                        "instance_name": instance_name
                    }
            }

            if ChainType == "CRC":
                input = {
                    "question": user_input,
                }
            elif ChainType == "RQA":
                input = {
                    "query": user_input,
                    #"chat_history": []
                }
            else:
                raise ValueError(f"Unknown chain type: {ChainType}")

            with get_openai_callback() as cb:
                # Call the constructed chain with the user input
                result = self._chain.invoke(input, config)
                # TODO: Telemetry metric?
                log_info(f"Tokens used: {cb.total_tokens}")

            CopilotHealthChecks.healthcheck_copilot.set_healthy()
            CopilotHealthChecks.healthcheck_openai.set_healthy()
            CopilotHealthChecks.healthcheck_azure_ai_search.set_healthy()

            response = self.get_response(result, cb)

        except Exception as ex:
            log_exception(ex)
            CopilotHealthChecks.healthcheck_copilot.set_failing("Failed during chain run")
            raise

        return response

    def get_response(self, result, callback_result):
        """Create the ChatReponse object from the chain result,
        adding citations and any debug return info requested."""

        answer = result["answer"] # "result" for RQA?
        source_docs = result["source_documents"]
        log_debug(f"Source docs: {list(set( [doc.metadata['Title'] for doc in source_docs] ))}")
        chat_request_options = self._chat_request.get_options()

        documents: List[ChatResponseDebugInfoDocument] = []
        #if chat_request_options.is_debug():
        if True:
            for doc in source_docs:
                #try: chunk_index = int(doc.metadata['Id'].split('_')[3]) except: chunk_index = -1
                documents.append(
                    ChatResponseDebugInfoDocument(
                        name = doc.metadata['Title'],
                        #chunk_index= chunk_index,
                        page = int(doc.metadata.get('PageNumber') or 0),
                        type = doc.metadata.get('ItemType') or "?",
                        content = doc.page_content 
                            if chat_request_options.is_debug_doc_contents() 
                            else None,
                        score = round( doc.metadata.get('@search.score', 0), 4)
                    )
                )
        log_debug(f"Documents: {documents}")

        hist_turns, hist_max_tokens, hist_cur_tokens, hist_summary = \
                        self.session_memory.get_history_stats()

        citations = None
        answer_response = answer
        # TODO: adjust prompt based on citation mode? Handle inline vs. end-citations from the model explicitly?
        new_answer, citations = get_citations(
            answer, 
            append_citations_text = chat_request_options.has_add_citations(),
            citations_mode = chat_request_options.get_citations_mode("off")
        )
        answer_response = new_answer

        citations_response = [
            ChatResponseCitation(name = c[0], pages = c[1]) for c in citations
        ] if citations else None

        debug_info = None

        if chat_request_options.is_debug():
            debug_info = ChatResponseDebugInfo(
                num_tokens_used = callback_result.total_tokens,
                history_turns = hist_turns,
                history_tokens_used = hist_cur_tokens,
                history_max_tokens = hist_max_tokens,
                history_summary_buffer = hist_summary,
                index_name = self._service_wrapper._vector_index_name,
                citations = citations_response,
                documents = documents if chat_request_options.is_debug() else None,
                prompt = MainPrompt().get_prompt_template(chat_request_options.prompt_hints).template \
                    if chat_request_options.is_debug_prompt() else None,
                #version_info = VersionInfo,
                user_question = self._chat_request.user_input,
                generated_question = result.get("generated_question"),
                request_options = self._chat_request.get_options(),
                llm_response = answer if chat_request_options.is_debug_response() else None
            )

            if self._chat_request.get_options().is_debug_inline():
                if chat_request_options.is_debug_json():
                    dump = json.dumps(asdict(debug_info), indent = 4, sort_keys = False)
                    format = "json"
                else:
                    dump = yaml.dump(asdict(debug_info), indent = 4, sort_keys = False)
                    format = "yaml"
                # include response and then debug info in a markdown code block
                answer_response = f"{answer_response}\n```{format}\n======== DEBUG INFO ===========\n\n{dump}\n```"

        return ChatResponse(
            responseText = answer_response,
            response_format = "text/plain",
            citations = citations_response,
            debug_info = debug_info if chat_request_options.is_debug_return() else None,
        )

# https://github.com/langchain-ai/langchain/issues/2303#issuecomment-1677280257
# https://github.com/langchain-ai/langchain/issues/6879
# TODO: doesn't work
class NoOpLLMChain(LLMChain):
    """No-op LLM chain."""

    def __init__(self) -> None:
        super().__init__(llm=AzureOpenAI(), prompt=PromptTemplate(template="", input_variables=[]))
        #super().__init__(llm=None, prompt=PromptTemplate(template="", input_variables=[]))
        pass

    async def arun(self, question: str, *args, **kwargs) -> str:
        return question

    def stream(self, **kwargs):
        return kwargs
    def invoke(self, **kwargs):
        return kwargs
    def batch(self, **kwargs):
        return kwargs
    async def astream(self, **kwargs):
        return kwargs
    async def ainvoke(self, **kwargs):
        return kwargs
    async def abatch(self, **kwargs):
        return kwargs