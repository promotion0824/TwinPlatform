
import datetime
import dateutil
import json
from shared.OpenTelemetry import log_info, log_debug, log_error, log_exception, log_warning
from marshmallow_dataclass import dataclass
from dataclasses_json import dataclass_json

@dataclass_json()
@dataclass
class DocumentMetadata:
    is_copilot_enabled: bool        # used to filter out non-copilot enabled files
    doc_dtdl_model_type: str
    doc_description: str            # used in doc chunks to add meta-data (for now)
    doc_summary: str
    doc_code: str
    doc_custom_tags: list[str]      # used to set is_copilot_enabled
    doc_last_update_time: datetime.datetime
    doc_hash_willow_sha256: str|None
    doc_hash_blob_md5: str|None
    doc_custom_properties: dict[str, str]|None = None

CopilotEnabledTag = "copilot"

class Metadata:

    # TODO: Split into filter tags and add new filter tag property
    @classmethod
    def parse_doctwin_metadata(cls, metadata: dict, file: str) -> DocumentMetadata:
        """Parse the JSON that was synd'c from the the ADT Document twin.
        Note we also have optional docTwinRelatedMetadata that we are not making use of yet.
        """

        copilot_enabled = False
        doc_dtdl_document_type = ""
        doc_description = ""
        doc_summary = ""
        doc_code = ""
        doc_tags = []
        doc_custom_props = None 
        doctwin_last_update_time: datetime.datetime = None
        hash_sha256 = None

        doc_twin_metadata_str = metadata.get("documentTwinMetadata") if metadata else None
        log_debug(f"DocumentTwinMetadata for '{file}': {doc_twin_metadata_str}")
        # TODO: Also use docTwinRelatedMetadata if we decide it's useful

        if doc_twin_metadata_str:
            try: 
                dtm = json.loads(doc_twin_metadata_str)

                doc_dtdl_document_type = dtm.get("dtdlDocumentType") or ""
                doc_description = dtm.get("description") or ""
                doc_summary = dtm.get("summary") or ""
                last_updated_date_str = dtm.get("lastUpdatedDate") or ""
                if last_updated_date_str:
                    # TODO: python 3.11 can use datetime.fromisoformat
                    doctwin_last_update_time = dateutil.parser.parse(last_updated_date_str)
                doc_code = dtm.get("code")
                # customTags is a dict of tagname:true (defined before arrays avail in DTDL)
                doc_tags_dict = dtm.get("customTags") 
                doc_tags = list(doc_tags_dict.keys()) if doc_tags_dict else []
                hash_sha256 = dtm.get("Sha256Hash")
                # TODO: Properties from models derived from Document?

                if "customProperties" in dtm:
                    doc_custom_props = dtm["customProperties"].get("copilot") or {}
                    # For manufacturer, model, etc.
                    # TODO: dtm["customProperties"].get("extra_metadata")

            except Exception as e: 
                # Eating this exception
                log_error(f"Error parsing '{file}' JSON documentTwinMetadata: '{doc_twin_metadata_str}'")
        else:
            log_debug(f"No documentTwinMetadata found in blob metadata for '{file}'")

        # Mark as copilot enabled if we have a copiot custom props section, unless it's marked explicitly as disabled   
        copilot_enabled = doc_custom_props is not None and ('false' != doc_custom_props.get('enabled', 'true').lower())
        # TODO: remove below if we decide to use only customProps for copilot enabled
        copilot_enabled = copilot_enabled or (CopilotEnabledTag in doc_tags)

        dmd = DocumentMetadata(
            is_copilot_enabled = copilot_enabled,
            doc_dtdl_model_type = doc_dtdl_document_type,
            doc_description = doc_description,
            doc_summary = doc_summary,
            doc_code = doc_code,
            doc_last_update_time = doctwin_last_update_time,
            doc_custom_tags = doc_tags,
            doc_custom_properties = doc_custom_props,
            doc_hash_willow_sha256 = hash_sha256,
            doc_hash_blob_md5 = None # filled in by caller
        )
        log_info(f"DocumentMetadata for '{file}': {dmd}")
        return dmd