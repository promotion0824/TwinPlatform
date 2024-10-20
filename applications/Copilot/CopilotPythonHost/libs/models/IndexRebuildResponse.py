

#from dataclasses import dataclass
from dataclasses_json import dataclass_json, LetterCase, Undefined, config
from marshmallow_dataclass import dataclass
from typing import Optional

UndefinedBehavior = Undefined.RAISE

@dataclass_json(undefined=UndefinedBehavior)
@dataclass
class IndexRebuildResponse:
    status: str
    job_id: Optional[str] = None
