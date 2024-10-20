
from typing import Optional, List

#from dataclasses import dataclass
from dataclasses_json import dataclass_json, LetterCase, Undefined, config
from marshmallow_dataclass import dataclass

from libs.models.ChatRequest import ChatRequestOptions
from shared import Citations

UndefinedBehavior = Undefined.RAISE

@dataclass_json(undefined=UndefinedBehavior)
@dataclass
class ChatResponseDebugInfoDocument:
    name: str
    page: int
    type: str
    content: Optional[str] = None
    score: Optional[float] = None

@dataclass_json(undefined=UndefinedBehavior)
@dataclass
class ChatResponseCitation:
    name: str
    pages: List[str]

@dataclass_json(undefined=UndefinedBehavior)
@dataclass
class ChatResponseDebugInfo:
    index_name: str
    num_tokens_used: int
    history_tokens_used: int
    history_max_tokens: int
    history_turns: int
    user_question: str
    generated_question: str
    history_summary_buffer: str
    prompt: Optional[str] 
    #version_info: str
    request_options: ChatRequestOptions
    citations: Optional[List[ChatResponseCitation]]
    documents: Optional[List[ChatResponseDebugInfoDocument]] 
    llm_response: Optional[str] = None


@dataclass_json(undefined=UndefinedBehavior)
@dataclass
class ChatResponse:
    #TODO: change to response_text when update nuget
    responseText: str
    response_format: str
    citations: Optional[List[ChatResponseCitation]] = None
    debug_info: Optional[ChatResponseDebugInfo] = None 