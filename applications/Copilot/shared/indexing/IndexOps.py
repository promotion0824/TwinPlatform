
from datetime import datetime, timedelta, timezone
import json
from typing import Literal, Optional, Dict, Any, List, Tuple

import os
import numpy as np
import pydantic
from tenacity import retry, wait_exponential, stop_after_attempt, before_sleep, before_sleep_log
import logging

from azure.search.documents.indexes.models import SearchIndex

from CopilotPythonHost.libs.models.GetIndexDocumentInfo import GetIndexDocumentInfoDocInfo, GetIndexDocumentInfoResponse
from shared.indexing.BlobOps import BlobOps
from shared.Utils import get_num_tokens, get_search_filter, try_parse_isodate, get_index_timestr
from shared.indexing.SearchIndexConfig import (
    FieldName_ContentTokenCount,
    FieldName_DocLastUpdateTime,
    FieldName_DocUnstructuredMetadata,
    FieldName_FilterTags,
    FieldName_Title,
    FieldName_TotalDocumentLength,
    FieldName_TotalDocumentNumChunks,
    FieldName_Uri,
    IndexFields,
    ItemType_DocumentSummary, 
    get_vector_search_config,
    ItemType_DocumentChunk,
    FieldName_Id, FieldName_IndexUpdateTime,
    FieldName_ItemType, FieldName_Content,
    FieldName_ParsePath, FieldName_PageNumber,
    FieldName_CopilotEnabled, FieldName_GroupId,
    FieldName_Vector
)

from shared.ServicesWrapper import ServicesWrapper
from shared.Utils import timed
from shared.OpenTelemetry import (
    log_exception, log_info, log_debug, log_warning, logger,
    MetricDeleteDocument, MetricGetDocumentInfo
)

# We can get errors for creating vector embeddings for large documents
#   and a single vector represeting an entire document has diminshing returns -
#   the fist part of the document is likely to contain most of the info we need
MaxWholeDocTextSizeForVectorEmbeddings = 8*1000

# TODO: could use python TypedDict instead of pydantic BaseModel (no runtime checking)
class IndexDocumentInfo(pydantic.BaseModel):
    model_config = pydantic.ConfigDict(extra='forbid')

    Title: str
    Uri: str
    RequestedChunkSize: int
    TotalDocumentNumChunks: int
    TotalDocumentLength: int
    DocLastUpdateTime: datetime
    IndexerSource: Optional[str] = None
    ProcessingParameters: Optional[str] = None
    DocUnstructuredMetadata: Optional[str] = None
    CopilotEnabled: Optional[bool] = True
    GroupId: Optional[str] = None
    IndexUpdateTime: Optional[str] = None
    FilterTags: Optional[List[str]] = None


    def to_doc_dict(self, chunk_group_id: str) -> Dict[str, str]:
        d = self.model_dump()
        d[FieldName_GroupId] = chunk_group_id
        d[FieldName_IndexUpdateTime] = get_index_timestr(datetime.now(timezone.utc))
        d[FieldName_DocLastUpdateTime] = get_index_timestr(self.DocLastUpdateTime)
        return d

    def get_chunk_group_id(self, item_type = ItemType_DocumentChunk) -> str:
        #timestamp = f"{datetime.now(timezone.utc).strftime('%Y-%m-%dT%H-%M-%S-%f')}"
        # Keys can only contain letters, digits, underscore (_), dash (-), or equal sign (=)
        # This should be the whole URI or the hash if we want to support 
        #  either multiple blob containers, multiple versions of the same doc
        clean_title = BlobOps.get_nameOrUri_filename(self.Uri).replace(' ', '_').replace('.', '_')
        clean_title = ''.join(filter( lambda c: 
                str.isalpha(c) or str.isdigit(c) or c=='-' or c=='_', clean_title)) 
        gid = f"{item_type}-{self.RequestedChunkSize}-{clean_title}" 
        # Don't include these so we can upsert docWhole or docSummary docs w/o deleting existing first
        # f"-{self.TotalDocumentLength}-{self.TotalDocumentNumChunks}" 
        return gid


class IndexDocumentChunk(pydantic.BaseModel):
    model_config = pydantic.ConfigDict(extra='forbid')

    Content: str
    PageNumber: int
    ContentTokenCount: int
    ContentLength: int
    ParsePath: Optional[str] = None
    Id: Optional[str] = None
    ContentVector: Optional[List[float]] = None
    ItemType: Optional[str] = ItemType_DocumentChunk

    def to_doc_dict(self, chunk_group_id: str, chunk_num: int) -> Dict[str, str|int]:
        d = self.model_dump()
        d[FieldName_ParsePath] = d[FieldName_ParsePath] or str( d[FieldName_PageNumber] or '')
        d[FieldName_Id] = d[FieldName_Id] or f"{chunk_group_id}-{chunk_num}"
        return d


class AzureAiIndexOps:

    def __init__(self, index_name: str = None, services: ServicesWrapper = None):
        if services:
            self.services = services
            self.index_name = services._vector_index_name
        else:
            self.index_name = index_name or ServicesWrapper.get_default_read_index_name()
            self.services = ServicesWrapper(index_name = self.index_name)
        log_info(f"AzureAiIndexOps: index_name: {self.index_name}")
        self._cached_index_names = []

    # TODO: Only create index for indexing operations - not for search
    #   otherwise we create random indexes with errrant runflags
    @timed()
    def create_or_update_index(self):

        if self.index_exists():
            log_info(f"Index '{self.index_name}' already exists - still calling create_or_update")

        index_client = self.services.get_search_index_client()
        index = SearchIndex(
            name = self.index_name, 
            fields = IndexFields,
            vector_search = get_vector_search_config(), 
            #semantic_search=semantic_search
        )
        result = index_client.create_or_update_index(index)
        log_info(f"Index '{result.name}' created or updated")

    @timed()
    def index_exists(self):
        """Check to see if index exits.
        We only need this because of langchains propensity to create indexes on-the-fly 
         during a query rather than throwing an error.
        This call is pretty slow to make every call, so cache positive responses.
        """
        if self.index_name in self._cached_index_names:
            return True
        index_client = self.services.get_search_index_client()
        index_names = [i for i in index_client.list_index_names()]
        found = self.index_name in index_names
        if found:
            self._cached_index_names.append(self.index_name)
        return found

    @timed()
    def delete_index(self):
        log_info(f"Deleting index '{self.index_name}'")
        index_client = self.services.get_search_index_client()
        index_client.delete_index(self.index_name)

    @timed()
    def _add_embeddings(self, 
                        doc_dicts: list[dict[str, Any]],
                        file: str|None = None
                        ):
        log_info(f"Getting embeddings for {len(doc_dicts)} documents")
        embed_service = self.services.get_embeddings_service()
        text_chunks = [ d[FieldName_Content] for d in doc_dicts]

        vector_embeddings = self._embed_documents(text_chunks)

        for i in range(len(doc_dicts)):
            # TODO: Do we really need numpy here?
            vec_list = np.array(vector_embeddings[i], dtype = np.float32).tolist()
            if not vec_list:
                log_warning(f"Empty vector embeddings vector {i} for file '{file}'")
            doc_dicts[i][FieldName_Vector] = vec_list

    @retry(
        wait=wait_exponential(multiplier=1, min=1, max=60), 
        stop=stop_after_attempt(8),
        before_sleep = before_sleep_log(logger, logging.DEBUG),
    )
    # Will throw a RetryError if all retries fail
    # "Error processing pdf file: Error code: 429 - {'error': {'code': '429', 'message': 'Requests to the Embeddings_Create Operation under Azure OpenAI API version 2023-05-15 have exceeded call rate limit of your current OpenAI S0 pricing tier. Please retry after 3 seconds. Please go here: https://aka.ms/oai/quotaincrease if you would like to further increase the default rate limit.'}}"
    def _embed_documents(self, text_chunks: List[str]) -> List[List[float]]:
        embed_service = self.services.get_embeddings_service()
        return embed_service.embed_documents(text_chunks)

    @timed()
    def add_doc_index_chunks_to_index(self, 
                        chunks: List[IndexDocumentChunk],
                        doc_info: IndexDocumentInfo,
                        file:str|None = None):
        """ Adds a Document to the index, with each chunk as a separate index document.
        doc_info contains the data common to all chunks, which is merged with the chunks.
        """
        log_info(f"Adding {len(chunks)} document chunks for {file} to index {self.index_name}")

        if len(chunks) == 0:
            log_warning("No document chunks to upload")
            return

        chunk_group_id = doc_info.get_chunk_group_id()
        # Merge the common doc info with each chunk
        info_dict = doc_info.to_doc_dict(chunk_group_id)
        merged_docs = [{**c.to_doc_dict(chunk_group_id, i), **info_dict} \
                        for (i,c) in enumerate(chunks)]

        # Call embedding service to get vectors for each chunk
        self._add_embeddings(merged_docs, file)
        self.upload_documents(merged_docs)


    @timed()
    def upload_documents(self, docs: List[Dict[str, Any]]):
        search_client = self.services.get_search_client()
        if not docs:
            log_warning("No index documents to upload")
            return 
        response = search_client.upload_documents(docs)
        if not all([r.succeeded for r in response]):
            raise Exception(response)

    @timed()
    def get_document(self, id)-> dict[str, Any]:
        search_client = self.services.get_search_client()
        return search_client.get_document(id)

    # Hack: Polymorphic return: return_total_count:true will return (count,docs) - all other callers don't need this and will return docs only
    # TODO: Refactor
    @timed()
    def find_index_docs(self, 
                        uri: str|None,
                        copilot_enabled_only: bool,
                        doc_type = None,
                        select_extra_fields: List[str]|None = None,
                        update_time: Optional[datetime] = None,
                        page_size:int|None = None,
                        page_number:int|None = None,
                        return_total_count: bool = False,
                        use_index_update_time: bool = True
                    ) -> List[Dict[str, Any]] | Tuple[int, List[Dict[str, Any]]]:
        """ Returns skinny documents (only doc Id) matching the filter.
        Select Id plus any additional fields - if s_a_f is None, omit select to include all fields.
        """
        filters = []

        if uri:
            uri = BlobOps.get_nameOrUri_uri(uri)
            filters.append( (FieldName_Uri, uri) )

        if copilot_enabled_only:
            #filters.append( (FieldName_CopilotEnabled, True) )
            filters.append( (FieldName_CopilotEnabled, False, "ne") )

        if doc_type:
            filters.append( (FieldName_ItemType, doc_type) )

        if update_time:
            filters.append((
                FieldName_IndexUpdateTime if use_index_update_time else FieldName_DocLastUpdateTime, 
                update_time, 
                "gt"))

        filter = get_search_filter(filters)

        search_client = self.services.get_search_client()
        fields = [FieldName_Id, *select_extra_fields] \
                    if select_extra_fields is not None else None

        skip = page_size * page_number if (page_number is not None and page_size) else None
        top = page_size or None

        try:
            search_results_response = search_client.search(
                search_text = "",
                filter = filter,
                select = fields,
                include_total_count = return_total_count,
                skip = skip, top = top
            )
            matching_docs = [s for s in search_results_response] # Note any exceptions are thrown here, not in .search
        except Exception as e:
            log_exception(f"Error searching for documents with filter '{filter}'", e)
            raise

        log_info(f"Returning {len(matching_docs)} matching documents for filter '{filter}'")

        if return_total_count:
            total_count = search_results_response.get_count()
            log_info(f"Returning page of {len(matching_docs)} matching documents for filter '{filter}' (of total: {total_count})")
            return total_count, matching_docs

        return matching_docs

    def find_skinny_docs_for_summary(self, uri: str) -> List[Dict[str, Any]]:
        """ Returns skinny documents (only doc Id) matching the uri and ItemType_DocumentSummary"""

        uri = BlobOps.get_nameOrUri_uri(uri)
        extra_fields = [FieldName_DocLastUpdateTime, FieldName_IndexUpdateTime]
        return self.find_index_docs(uri, 
                                    copilot_enabled_only = False, 
                                    doc_type = ItemType_DocumentSummary,
                                    select_extra_fields = extra_fields)

    @timed()
    def delete_documents(self, docs:List[dict[str,Any]]):
        search_client = self.services.get_search_client()
        response = search_client.delete_documents(docs)
        if not all([r.succeeded for r in response]):
            raise Exception(response)

    # TODO: Need to pass in chunk size/overlap to only detect/delete correctly sized chunks
    #  Will need to use groupId rather than Uri for search
    @timed()
    def delete_docs_for_blob_uri_if_needed(self, 
                                 uri: str, 
                                 blob_update_time: datetime|None = None,
                                 match_only:bool = False) -> int:
                                 
        """ Deletes all document chunks where d.Uri == uri
            If doc_update_time is passed, only deletes if the doc was updated after that time
        """
        matching_docs, n_docs = None, 0
        try:
            uri = BlobOps.get_nameOrUri_uri(uri) # in case only filename part was passed in
            search_client = self.services.get_search_client(self.index_name)
            matching_docs = self.find_index_docs(
                uri, 
                select_extra_fields = [FieldName_DocLastUpdateTime],
                copilot_enabled_only = False)
            n_docs = len(matching_docs)

            if n_docs > 0:

                if blob_update_time:
                    doc = matching_docs[0]
                    # TODO: removing the Z not needed after python v3.11
                    strtime = doc[FieldName_DocLastUpdateTime].replace('Z', '+00:00')
                    existing_time = datetime.fromisoformat(strtime)
                    if blob_update_time <= existing_time:
                        return -1 

                if match_only:
                    log_debug("Match_only is True, not deleting - will upsert new docChunks")
                    return len(matching_docs)

                log_info(f"Deleting existing docChunks for uri: {uri}")
                response = search_client.delete_documents(matching_docs)

                # deletion is idempotent and will succeed for any id so should never get 
                #  a doc-specific error unless we have a bug passing invalid ids
                if not all([r.succeeded for r in response]):
                    matching_docs = None
                    # TODO: Better exception
                    raise Exception(response)
        finally:
            MetricDeleteDocument(matching_docs is not None, n_docs)

        return n_docs


    def add_whole_index_document(self, 
                                   text: str, 
                                   file: str,
                                   doc_last_update_time: datetime,
                                   item_type: str,
                                   metadata: Optional[str] = None,
                                   summary_type: Optional[str] = None):
        
        uri = BlobOps.get_nameOrUri_uri(file)
        file = BlobOps.get_nameOrUri_filename(file)

        log_info(f"Adding whole document of type {item_type} to index: '{file}'")

        summary_doc_info = IndexDocumentInfo(
            Title = file,
            # Note the Uri is retuned url-encoded - need to be careful that we search with the same encoding
            #  or use BlobOps.get_uriOrName_filename(file) to get the unencoded filename
            Uri = uri,
            RequestedChunkSize = 0,
            TotalDocumentNumChunks = 0,
            TotalDocumentLength = len(text), # we could make this the len of the whole source doc
            DocLastUpdateTime = doc_last_update_time,
            IndexerSource = "py-indexer",
            CopilotEnabled = True,

            # dump as json string of dict - will be url encoded
            ProcessingParameters = json.dumps(None),  #TODO: add summary prompt hash, LLM info, etc?
            FilterTags = [],
            DocUnstructuredMetadata = metadata or json.dumps(f"summary for {file}"),
        )

        #TODO: whole/summary docs don't really need requstedChunkSize, but will always be default and won't hurt 
        group_id = summary_doc_info.get_chunk_group_id(item_type)
        id = f"{summary_type}-{group_id}" if summary_type else group_id
        
        summary_doc_chunk = IndexDocumentChunk(
            Id = id,
            ItemType = item_type,
            Content = text,
            PageNumber = 0,
            ContentTokenCount = get_num_tokens(text),
            ContentLength = len(text),
        )
        merged_doc = {**summary_doc_info.to_doc_dict(group_id), 
                       **summary_doc_chunk.to_doc_dict(group_id, 0)}

        # TODO: remove when Azure indexer updated or we move completely away from it
        del merged_doc[FieldName_FilterTags]

        search_client = self.services.get_search_client()
        # Limit the amount of text we create embeddings for, but still write entire doc to index
        if len(text) > MaxWholeDocTextSizeForVectorEmbeddings:
            log_info(f"Document text too large for vector embeddings - clipping: {len(text)} chars to {MaxWholeDocTextSizeForVectorEmbeddings} for '{file}'")
            merged_doc[FieldName_Content] = text[:MaxWholeDocTextSizeForVectorEmbeddings]
        self._add_embeddings([merged_doc], file = file)
        merged_doc[FieldName_Content] = text

        # This will upsert if already exists
        responses = search_client.upload_documents(documents = [merged_doc] )

        if not all([r.succeeded for r in responses]):
            raise Exception(responses)

    # Returning part of a response object is a leaky abstraction
    # This fn should be moved to a new api helper file, as it's only used for GetDocInfo 
    def get_document_info(self, file: str) -> GetIndexDocumentInfoDocInfo | None:
        try:
            info = self._get_document_info(file)
            MetricGetDocumentInfo(True, info is not None)
        except Exception as e:
            MetricGetDocumentInfo(False, False)
            log_exception(f"Error getting document info for '{file}'", e)
            raise

    def _get_document_info(self, file: str) -> GetIndexDocumentInfoDocInfo | None:
        docs = self.find_index_docs(file, copilot_enabled_only = False, 
                select_extra_fields = [
                    FieldName_IndexUpdateTime, 
                    FieldName_TotalDocumentNumChunks,
                    FieldName_TotalDocumentLength,
                    FieldName_ItemType,
                    FieldName_CopilotEnabled,
                    FieldName_DocUnstructuredMetadata,
                    FieldName_Uri,
                    FieldName_Title
                ])

        if not docs: 
            return None

        chunk_docs = [d for d in docs if d[FieldName_ItemType] == ItemType_DocumentChunk]
        chunk_doc = chunk_docs[0] if chunk_docs else None
        doc = chunk_doc or docs[0]
        summary_docs = [d for d in docs if d[FieldName_ItemType] == ItemType_DocumentSummary]
        summary_doc = summary_docs[0] if summary_docs else None
        # TODO: Could recover from cache here if not in index (though should be copied to index during any ifNewer rebuild)
        if summary_doc:
            search_client = self.services.get_search_client()
            summary_doc = search_client.get_document(summary_doc[FieldName_Id])
            #summary_doc = self.find_index_docs(file, copilot_enabled_only = False, select_extra_fields = [ FieldName_Content ])[0]
            
        info = GetIndexDocumentInfoDocInfo(
            uri = doc[FieldName_Uri],
            file = doc[FieldName_Title],
            indexed_time = doc[FieldName_IndexUpdateTime],
            # Return -1 if info not available - only if using index maintained by Azure integrated indexer 
            num_chunk_docs = int(doc[FieldName_TotalDocumentNumChunks] or "-1"),
            document_size = int(chunk_doc[FieldName_TotalDocumentLength]) if chunk_doc else -1,
            summary = summary_doc[FieldName_Content] if summary_doc else None,
            copilot_enabled = doc[FieldName_CopilotEnabled],
            metadata_json = chunk_doc[FieldName_DocUnstructuredMetadata] if chunk_doc else None,
            summary_updated_time = summary_doc[FieldName_IndexUpdateTime] if summary_doc else None
        )
        return info

if __name__ == '__main__':  

    # sys.path.append(os.path.dirname(os.path.abspath(__file__ + '/../../')))
    #index_name = "index_from_python_test"
    index_name = "py-documents-wil-dev"
    ops = AzureAiIndexOps(index_name = index_name)

    if False:
        ops.add_summary_index_document("This is a test summary", "summarized-doc.pdf")
        pass

    if False:
        ops.create_or_update_index()

    if True:
        #file = 'https://stodeveus01wili3c46beea.blob.core.windows.net/twindocuments/Krack%20Refrigeration%20Load%20Estimating%20Manual.pdf'
        #file = "https://stodeveus01wili3c46beea.blob.core.windows.net/twindocuments/ll97of2019.pdf"
        file = "https://stodeveus01wili3c46beea.blob.core.windows.net/twindocuments/troubleshooting-guide.pdf"
        #ops.find_skinny_docs_for_file(file)
        #ops.delete_docs_for_blob_uri(file)
        time = datetime.now(timezone.utc) - timedelta(hours=23)
        docs = ops.find_index_docs(uri = None, 
                                   doc_type="docSummary", 
                                   update_time=time,
                                   copilot_enabled_only = False)
        pass

    if False:
        title = "test-document.pdf"
        file = f"http://blobs/{title}"
        doc_info = IndexDocumentInfo(
            Title = title,
            Uri = file,
            CopilotEnabled = True,
        )

        chunks = [
            IndexDocumentChunk(
                Content = "This is a test",
                PageNumber = 1,
            ),
            IndexDocumentChunk(
                Content = " of the document",
                PageNumber = 2,
            ),
        ]
        ops.add_doc_to_index(chunks, doc_info)

    if False:
        title = "another-doc.pdf"
        file = f"http://blobs/{title}"
        doc_info = IndexDocumentInfo(
            Title = title,
            Uri = file,
            CopilotEnabled = True,
        )

        chunks = [
            IndexDocumentChunk(
                Content = "different stuff",
                PageNumber = 1,
            ),
            IndexDocumentChunk(
                Content = " of the document",
                PageNumber = 2,
            ),
        ]
        ops.add_doc_to_index(chunks, doc_info)

    pass