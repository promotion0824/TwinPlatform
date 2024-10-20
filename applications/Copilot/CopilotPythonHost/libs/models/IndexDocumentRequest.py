
from typing import Optional, List

#from dataclasses import dataclass
from dataclasses_json import dataclass_json, LetterCase, Undefined, config
from marshmallow_dataclass import dataclass

UndefinedBehavior = Undefined.RAISE

@dataclass_json(undefined=UndefinedBehavior)
@dataclass
class IndexDocumentRequest:
    blob_file: str
    generate_summaries_mode: Optional[str] = "ifNewer"
    generate_index_mode: Optional[str] = "ifNewer"
    index_name: Optional[str] = None
    run_in_background: Optional[bool] = True
    chunk_size: Optional[int] = None
    chunk_overlap: Optional[int] = None
    include_chunkdocs_for_copilot_disabled_files: Optional[bool] = False
    indexing_strategy: Optional[str] = None
