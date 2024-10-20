
from typing import Any
from datetime import datetime

from langchain.memory.chat_memory import BaseChatMemory
from langchain.memory import ConversationBufferMemory
from langchain.memory import ConversationSummaryBufferMemory
from langchain.memory.prompt import SUMMARY_PROMPT
from langchain_openai import AzureChatOpenAI

from shared.OpenTelemetry import log_info, log_debug, log_exception
from shared.Prompts import MemorySummarizePrompt


class ConversationMemory:
    """ Holds the memeory atttached to the user's session based on the supplied session ID
    The memory holds the underlying langchain memory object and associated resoruces
     as well as the last access time used for forgetting old memories.
    """

    DefaultMaxTokens = 4000

    def __init__(self, 
                 cache_key:str = "_Missing_", 
                 llm = None,
                 max_tokens = None):

        self._langchain_memory = None
        self._cache_key = cache_key # TODO: can remove if not needed for logging, etc.
        self._llm = llm
        self._max_tokens = max_tokens or self.DefaultMaxTokens
        self.sticky_run_flags = []

        if max_tokens: self.init_memory(llm, max_tokens)
        self.touch()

    def touch(self):
        self.last_acccess_time = datetime.now()

    def set_sticky_runflags(self, flags: list[str]):
        self.sticky_run_flags = flags
    def get_sticky_runflags(self) -> list[str]:
        return self.sticky_run_flags

    def init_memory(
            self, 
            llm,
            max_tokens:int,
            chain_type: str = "CRC", 
            memory_type = "CSBM",
        ):

        if self._langchain_memory:
            if self._max_tokens != max_tokens:
                # TODO: OK to change this after creation? - seems fine from looking at source
                # Assuming we don't change the memory type (hard-coded here)
                self._max_tokens = self._langchain_memory.max_token_limit = max_tokens
            self._llm = self._langchain_memory.llm = llm
            return

        self._llm = llm
        self._max_tokens = max_tokens

        input_key = "question" if chain_type == "CRC" else "query"
        output_key = "answer" # if chain_type == "CRC" else "result"
        memory_key = "chat_history"

        # This is what we'd like to do here (have no mem as opposed to a small one) - but not allowed
        # if self._max_tokens < 1: self._langchain_memory = None

        if memory_type == "CBM":
            self._langchain_memory = ConversationBufferMemory(
                memory_key = memory_key,
                input_key = input_key,
                output_key = output_key,
                return_messages = True,
                human_prefix = "User",
                ai_prefix = "Copilot",
            )

        # Error: get_num_tokens_from_messages() is not presently implemented for model gpt-35-turbo-16k. 
        elif memory_type == "CSBM":
            self._langchain_memory = ConversationSummaryBufferMemory(
                memory_key = memory_key,
                output_key = output_key,
                input_key = input_key,
                return_messages = True,
                prompt = MemorySummarizePrompt().get_prompt_template(),
                # num tokens to use before we summarize the memory buffer - defalut:2000 
                max_token_limit = max(3, self._max_tokens), # Bug if set very low due to some incorrect code in langchain
                llm = self._llm,
                human_prefix = "User",
                ai_prefix = "Copilot",
            )
        else:
            raise ValueError(f"Unknown memory_type: {memory_type}")

    def get_langchain_memory(self) -> BaseChatMemory | None:
        return self._langchain_memory

    def get_history_stats(self) -> tuple[int, int, int, str]:
        turns, tokens, summary_buffer = -1, -1, "" 
        try:
            # Works for CSBM, test for CBM if needed
            tokens = self._llm.get_num_tokens_from_messages(
                                self._langchain_memory.chat_memory.messages)
            turns = len(self._langchain_memory.chat_memory.messages) // 2
            summary_buffer = self._langchain_memory.moving_summary_buffer
        except Exception as ex:
            log_exception(ex, "Failed to get history tokens")
        return turns, self._max_tokens, tokens, summary_buffer

class MemorySessionCache:
    """Hold memory based on request-supplied session id"""

    MemoryTTLHours = 12

    def __init__(self):
        self._session_mem_cache: dict[str, ConversationMemory] = {}
    
    # Delete memories that are older than the TTL
    def forget_old_memories(self, cache: dict[str, ConversationMemory]):
        for key, memory in list(cache.items()):
            delta = (datetime.now() - memory.last_acccess_time).seconds
            if delta > self.MemoryTTLHours *60*60:
                log_debug(f"Forgetting old memory for: {key}")
                del cache[key]

    def get_llm_memory_for(self, 
                       key:str, 
                       llm: AzureChatOpenAI, 
                       max_tokens: int) -> BaseChatMemory | None:
        """Return the underlying langchain memory for the given session id, creating it if necessary"""

        memory = self.get_memory_for(key, llm, max_tokens)
        return memory.get_langchain_memory()


    def get_memory_for(self, 
                       key:str, 
                       llm: AzureChatOpenAI = None, 
                       max_tokens: int = None) -> ConversationMemory:
        """Return the memory for the given session id, creating it if necessary.
        Also scan for old memories and delete them if over time-to-live. """

        memory = self._session_mem_cache.get(key)

        if not memory:
            log_debug(f"Creating new memory for: {key}")
            # Is it ok to keep the LLM around? - should be as only wrapper for rest calls -
            #  otherwise could store a closure factory fn
            memory = self._session_mem_cache[key] = ConversationMemory(key, llm, max_tokens)
        else:
            log_debug(f"Found cached memory for: {key}")
            memory.touch()

        self.forget_old_memories(self._session_mem_cache)

        return memory

MemorySessionCacheSingleton = MemorySessionCache()
