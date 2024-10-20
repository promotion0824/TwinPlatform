from typing import Optional, List, Any
import re
import os

#from dataclasses import dataclass
from dataclasses_json import dataclass_json, LetterCase, Undefined, config
from marshmallow_dataclass import dataclass

from shared.Citations import get_citations
from shared.OpenTelemetry import log_debug

# Warning: Do not set any default values for dataclass properties 
#  other than dataclasses.field - this will override the field created
#  by the metaclass and break things.

UndefinedBehavior = Undefined.RAISE

@dataclass_json(letter_case = LetterCase.CAMEL, undefined=UndefinedBehavior)
@dataclass
class ChatRequestContext:

    dataclass_json_config = config(letter_case=LetterCase.CAMEL, undefined=UndefinedBehavior)

    session_id: str
    user_name: Optional[str] = None
    user_email: Optional[str] = None
    user_id: Optional[str] = None  # TODO: Will become non-optional

@dataclass_json(letter_case = LetterCase.CAMEL, undefined=UndefinedBehavior)
@dataclass
class ChatRequestOptions:

    # TDDO: Unify standalone and k/v bool flags

    ValidRunFlags = [ 
                    ########## Standalone runflags (missing = False)
                    "DebugInline",              # return debug info inline with response text
                    "DebugReturn",              # return debug info as separate payload
                    "DebugDocContents",         # return document chunk text contents (doc titles/chunk# always returned)
                    "DebugPrompt",              # return the LLM prompt used 
                    "DebugResponse",            # add LLM response (before any post-processing)
                    "DebugJson",                # Return inline debug info as JSON rather than YAML
                    "TraceLangsmith",           # if LANGCHAIN_KEY is set, trace using langsmith SAS tool
                    "LLM-Large", "LLM-Small",   # if LLM-Large is set, use GPT4, else use GPT3.5
                    "AllDocs",                  # Use all documents, even if not copilot-enabled
                    "Clear",                    # Clear all sticky/inline runflags attched to sessions memory
                    "AddCits",                  # Append citations to response text
                    ########### boolean k/v runflags
                    "AddChunkMetadata",         # Add metadata to each document chunk [True] (citation info always included)
                    ########### numeric k/v runflags
                    "KDocs",                    # number of documents to retrieve from vector search
                    "LLMTemp",                  # temperature for LLM (default is 1.0 for GPT3, 0.7 for GPT4)
                    "HistoryPercent",           # Percentage of tokens alloted to history to use (0 = no memory set)
                    "ExpandChunksPercent",      # percentge of content token limit to use up expanding neighboring chunks after search
                    ########### str k/v runflags
                    "SearchType",               # similarity | hybrid | keyword (only fist works w/current chain)
                    "IndexName",                # name of the index to use for vector search (for a/b testing during dev)
                    "CitationsMode",            # 'inline' | 'refs' | 'refs-pages' | 'off'
    ]

    ValidPromptHints = []
    # Note: Using runflags for models now - remove model hints if this seems appropriate
    ValidModelHints = ["GPT3", "GPT4"]

    dataclass_json_config = config(letter_case=LetterCase.CAMEL, undefined=UndefinedBehavior)

    model_hint: Optional[str] = None
    prompt_hints: Optional[List[str]] = None
    run_flags: Optional[List[str]] = None

    # Not passed in request - created from run_flags - should not be added to swagger
    run_flags_dict: Optional[dict] = None 

    def __post_init__(self):
        self.check_flags()

    def check_flags(self):

        if self.model_hint and self.model_hint not in self.ValidModelHints:
            raise ValueError(f"Invalid model_hint: {self.model_hint}")

        self.prompt_hints = self.prompt_hints or []
        for hint in self.prompt_hints:
            if hint not in self.ValidPromptHints:
                raise ValueError(f"Invalid prompt_hint: {hint}")

        self.run_flags = self.run_flags or []
        self.run_flags_dict = {}

        log_debug(f"Processing run_flags: {self.run_flags}")

        for flag in self.run_flags:
            if not flag:
                continue
            if ':' in flag:
                [k,v] = flag.split(':')
                if k not in self.ValidRunFlags:
                    raise ValueError(f"Invalid key-value run_flag: {k}")
                self.run_flags_dict[k] = v
            else:
                if flag not in self.ValidRunFlags:
                    raise ValueError(f"Invalid run_flag: {flag}")
                self.run_flags_dict[flag] = True
            

    def get_run_flag_value(self, flag:str, default:Any = False) -> Any:
        """ Return value of run flag with value like "KDocs:8"
        """
        if flag not in self.ValidRunFlags:
            raise ValueError(f"Invalid key-value run_flag: {flag}")
        return self.run_flags_dict.get(flag, default)

    def has_request_run_flags(self) -> bool:    
        return len(self.run_flags) > 0 if self.run_flags else False

    def has_run_flags(self) -> bool:    
        return len(self.run_flags_dict) > 0 

    def set_request_run_flags(self, flags:List[str]):
        self.run_flags = flags
        self.check_flags()
    def get_request_run_flags(self) -> List[str]:
        return self.run_flags

    def has_run_flag(self, flag:str) -> bool:
        if flag not in self.ValidRunFlags:
            raise ValueError(f"Invalid run_flag: {flag}")
        # Return False if flag missing, or explicitly set to False
        return self.run_flags_dict.get(flag, False)

    def has_prompt_hint(self, hint:str) -> bool:
        if hint not in self.ValidPromptHints: 
            raise ValueError(f"Invalid prompt_hint: {hint}")
        return hint in (self.prompt_hints or [])

    def get_bool_flag(self, flag:str, default:bool = False) -> bool:
        val = self.get_run_flag_value(flag, default)
        if isinstance(val, bool):
            return val
        val = str(val).lower()
        if val not in ["true", "false"]:
            raise ValueError(f"Invalid boolean run_flag value: {val}")
        return val == "true"
    
    def get_model_hint(self) -> Optional[str]:
        return self.model_hint or None # don't return ""

    def is_debug_inline(self) -> bool:
        return self.has_run_flag("DebugInline")

    def add_chunk_metadata(self) -> bool:
        return self.get_bool_flag("AddChunkMetadata", True)

    def is_debug_return(self) -> bool:
        return self.has_run_flag("DebugReturn")

    def is_debug_response(self) -> bool:
        return self.has_run_flag("DebugResponse")

    def is_debug_doc_contents(self) -> bool:
        return self.has_run_flag("DebugDocContents")

    def is_debug_prompt(self) -> bool:
        return self.has_run_flag("DebugPrompt")

    def is_debug_json(self) -> bool:
        return self.has_run_flag("DebugJson")

    def is_debug(self) -> bool:
        return self.is_debug_inline() or self.is_debug_return()

    def is_llm_large(self) -> bool:
        return self.has_run_flag("LLM-Large")

    def use_all_docs(self) -> bool:
        return self.has_run_flag("AllDocs")

    def has_add_citations(self) -> bool:
        return self.has_run_flag("AddCits")

    def get_citations_mode(self, default:str = "off") -> str:
        return self.get_run_flag_value("CitationsMode", default)

    def has_clear_flag(self) -> bool:
        return self.has_run_flag("Clear")
    def remove_clear_flag(self):
        if self.has_clear_flag():
            self.run_flags.remove("Clear")

    def is_langsmith_enabled(self) -> bool:
        return self.has_run_flag("TraceLangsmith")

    def get_history_percent(self, default:int) -> int:
        return int(self.get_run_flag_value("HistoryPercent", default))

    def get_k_docs(self, default:int = 6) -> int|None:
        kdocs = self.get_run_flag_value("KDocs", default)
        return int(kdocs) if kdocs else None

    def get_expand_chunks_percent(self, default:int = 50) -> int:
        return int(self.get_run_flag_value("ExpandChunksPercent", default))

    def get_llm_temp(self, default:int = None) -> float|None:
        temp = self.get_run_flag_value("LLMTemp", default)
        return float(temp) if temp else None

    def get_search_type(self, default:str = None) -> str:
        st = self.get_run_flag_value("SearchType", default)
        if st is not None and (st not in ["hybrid", "similarity"]):
            raise ValueError(f"Invalid search_type: {st}")
        return st

    def get_index_name(self, default:str = None) -> str:
        return self.get_run_flag_value("IndexName", default)


@dataclass_json(letter_case = LetterCase.CAMEL, undefined=UndefinedBehavior)
@dataclass
class ChatRequest:
    dataclass_json_config = config(letter_case=LetterCase.CAMEL, undefined=UndefinedBehavior)

    user_input: str
    context: ChatRequestContext
    options: Optional[ChatRequestOptions] = None

    # TODO: can remove this now given the post-init
    def get_options(self) -> ChatRequestOptions:
        return self.options or ChatRequestOptions(None, None, None)

    def __post_init__(self):
        if not self.options:
            self.options = ChatRequestOptions(None, None, None)

        request_run_flags = self.options.run_flags or []
        self.options.run_flags = []

        # Add runflags in reverse order of precedence (env, request, inline) so that they override as expected
        self.add_env_runflags()
        self.options.run_flags.extend(request_run_flags)
        self.add_inline_runflags()

        self.options.check_flags()
        log_debug(f"Final run_flags: {self.options.run_flags_dict}")

    def add_env_runflags(self):
        """Add runflags from the environment
        """
        env_run_flags = os.environ.get("COPILOT_RUNFLAGS")
        if not env_run_flags:
            return
        for flag in env_run_flags.split(','):
            self.options.run_flags.append(flag)

    def add_inline_runflags(self):
        """Allow the user to specificy runflags for the specific request.
        """
        regex = r"<<<CRF (.*?)>>>"
        match = re.search(regex, self.user_input)
        if match:
            flag_text = match.group(1)
            added_run_flags = flag_text.split() 
            self.user_input = self.user_input.replace(match.group(0), "").strip()
            for flag in added_run_flags:
                self.options.run_flags.append(flag)


        
