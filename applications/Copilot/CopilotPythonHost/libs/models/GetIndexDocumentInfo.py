
from typing import Optional, List
from datetime import datetime

#from dataclasses import dataclass
from dataclasses_json import dataclass_json, LetterCase, Undefined, config
from marshmallow_dataclass import dataclass

UndefinedBehavior = Undefined.RAISE

@dataclass_json(undefined=UndefinedBehavior)
@dataclass
class GetIndexDocumentInfoRequest:
    blob_files: List[str]
    index_name: Optional[str] = None

@dataclass_json(undefined=UndefinedBehavior)
@dataclass
class GetIndexDocumentInfoDocInfo:
    uri: str
    file: str
    indexed_time: str
    document_size: int
    num_chunk_docs: int
    copilot_enabled: bool
    metadata_json: Optional[str]
    summary: Optional[str]
    summary_updated_time: Optional[str]

class GetIndexDocumentInfoResponse:
    doc_infos: List[GetIndexDocumentInfoDocInfo]