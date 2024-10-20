
from typing import Optional, List
from datetime import date, datetime

from dataclasses import field
from dataclasses_json import dataclass_json, LetterCase, Undefined, config
from marshmallow import fields
from marshmallow_dataclass import dataclass

UndefinedBehavior = Undefined.RAISE

@dataclass_json(undefined=UndefinedBehavior)
@dataclass
class FindIndexDocumentsRequest:
    # This still doesn't work for 'Z'ulu TZ or when missing - see fix_request_time handling in api
    # last_updated_time: Optional[datetime] = field(
    #     metadata=config(
    #         encoder=datetime.isoformat,
    #         decoder=datetime.fromisoformat,
    #         mm_field=fields.DateTime(format='iso')
    #     )
    # )
    # This expects unix epoch timestamp- see fix_request_time_timestamp handling in api
    last_updated_time: Optional[datetime] = None
    document_type: Optional[str] = None
    index_name: Optional[str] = None
    page_size: Optional[int] = None
    page_number: Optional[int] = None
    include_metadata: Optional[bool] = False
    include_content: Optional[bool] = True

# TODO: Allow query by uri, groupId, or Id?
# TODO: Return common metadata (should be same for all chunks) in main response? - main use case at the moment is for single docs like summary
@dataclass_json(undefined=UndefinedBehavior)
@dataclass
class FindIndexDocumentsDocument:
    uri: str
    id: str
    content: str|None
    doc_type: str
    indexed_time: datetime
    document_size: int
    metadata_json: str|None

@dataclass_json(undefined=UndefinedBehavior)
@dataclass
class FindIndexDocumentsResponse:
    total_count: int
    index_documents: List[FindIndexDocumentsDocument]