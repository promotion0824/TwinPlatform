
from typing import Optional, List

#from dataclasses import dataclass
from dataclasses_json import dataclass_json, LetterCase, Undefined, config
from marshmallow_dataclass import dataclass

UndefinedBehavior = Undefined.RAISE

@dataclass_json(undefined=UndefinedBehavior)
@dataclass
class DeleteDocumentRequest:
    blob_file: str
    index_name: Optional[str] = None
