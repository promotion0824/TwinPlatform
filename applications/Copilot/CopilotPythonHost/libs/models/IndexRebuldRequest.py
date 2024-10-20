
from typing import Optional, List

#from dataclasses import dataclass
from dataclasses_json import dataclass_json, LetterCase, Undefined, config
from marshmallow_dataclass import dataclass

UndefinedBehavior = Undefined.RAISE

@dataclass_json(undefined=UndefinedBehavior)
@dataclass
class IndexRebuildRequest:
    delete_and_recreate_index: bool
    generate_summaries_mode: Optional[str]
    generate_index_mode: Optional[str]
    index_name: Optional[str] = None
    run_in_background: Optional[bool] = True
    num_threads: Optional[int] = None
    chunk_size: Optional[int] = None
    chunk_overlap: Optional[int] = None
    include_chunkdocs_for_copilot_disabled_files: Optional[bool] = True
    indexing_strategy: Optional[str] = None
