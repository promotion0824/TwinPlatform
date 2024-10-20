import datetime
import io
import time
import os
import base64
from typing import List, Any, Tuple, Literal, Callable
import json
from concurrent.futures import ThreadPoolExecutor
import threading
import multiprocessing
import deepdiff
from time import sleep
from pypdf import PdfReader

from shared.indexing.IndexOps import IndexDocumentChunk, IndexDocumentInfo, AzureAiIndexOps
from shared.ServicesWrapper import ServicesWrapper
from shared.indexing.TextChunker import TextChunk, TextChunker
from shared.indexing.BlobOps import BlobOps
from shared.indexing.SearchIndexConfig import FieldName_CopilotEnabled, FieldName_FilterTags, FieldName_Id, FieldName_IndexUpdateTime, FieldName_Title, FieldName_Vector, ItemType_DocumentSummary, ItemType_DocumentWhole, FieldName_DocUnstructuredMetadata, ItemType_DocumentChunk, FieldName_ItemType
from shared.indexing.DocumentSummarizer import DocumentSummarizer, SummaryError, summary_is_error
from shared.indexing.SearchIndexConfig import FieldName_DocLastUpdateTime, FieldName_TotalDocumentLength, FieldName_ContentLength
from shared.Metadata import DocumentMetadata, Metadata
from shared.Utils import (
    timed, get_num_tokens, default, try_parse_isodate, 
    gc_collect, DebugMode, elapsed_ms
)
from shared.OpenTelemetry import (
    log_info, log_debug, log_error, log_exception, log_warning,
    MetricIndexDocument, MetricIndexRebuildRequest, MetricIndexRebuildComplete,
    MetricCreateSummary
)

from azure.storage.blob._models import BlobProperties

DefaultChunkSize = 4000 # about 1k-1.3k tokens
DefaultChunkOverlap = 200

# TODO: separate class for processing various format files and adding to index (txt at least)
# TODO: cutoff size to not chunk in less than n tokens?

PyPdfExtractionMode: Literal['plain', 'layout'] = 'plain'

GenerateSummaryModeStrEnum = Literal['off', 'ifNewer', 'force'] 
GenerateIndexModeStrEnum = Literal['off', 'ifNewer', 'force'] 

IndexingStrategy_SimpleTextOverlap = "overlap"
IndexingStrategy_PageWithOverlap = "page-overlap"
ErrorSummary = "N/A"

RebuildInProgressStartTime:datetime.datetime | None = None


class PyPdfIndexProcessor:
    """Class to process PDF files - parse, chunk, and add to index."""

    def __init__(self, index_name_override = None, remove_extra_whitespace = True) -> None:
        self.pdf_reader = None
        self.index_ops = AzureAiIndexOps(index_name = index_name_override)
        self.remove_extra_whitespace = remove_extra_whitespace
        self.pages_text = []

    def get_pdf_reader(self, stream: io.BytesIO) -> PdfReader:

        if self.pdf_reader is not None:
            return self.pdf_reader

        self.pdf_reader = PdfReader(
            stream = stream,
            strict = False
        )

        form_fields = self.pdf_reader.get_form_text_fields()
        if form_fields:
            log_info(f"Pdf has form fields: {form_fields}") 

        # {"/CreationDate": "D:20190711102200-04'00'", 
        #  "/Creator": "Microsoft\u00ae Word 2016", 
        #  "/Producer": "Microsoft\u00ae Word 2016", 
        #  "/ModDate": "D:20190715150246-04'00'", 
        #   "/Title": "Local Law 97 of 2019"}
        doc_metadata = self.pdf_reader.metadata
        # TODO: Populate new EmbeddedDocumentMetadata field with PDF doc metadata if useful
        if doc_metadata:
            log_info(f"PDF embedded metadata: {json.dumps(doc_metadata)}")

        return self.pdf_reader

    def can_parse(self, stream) -> bool:
        try:
           self.get_pdf_reader(stream)
           return True
        except Exception as e:
            print(f"could not parse pdf stream: {e}")
            return False

    @timed()
    def get_text_from_pages(self, stream) -> list[str]:
        """Extract the text from each page in the PDF file and return a list of strings."""

        try:
            pdf_reader = self.get_pdf_reader(stream)
            log_info(f"Num pages: {len(pdf_reader.pages)}")

            self.pages_text = [''] * len(pdf_reader.pages)
            for i, page in enumerate(pdf_reader.pages):
                text = page.extract_text(
                    extraction_mode = PyPdfExtractionMode,
                    layout_mode_space_vertically = False,
                )
                self.pages_text[i] = text
                log_debug(f"\tParser: Page {i+1}: len {len(text)} ({get_num_tokens(text)} tokens)")
                # Each page extract takes several hundred ms to several mins worst case -
                #   yield CPU and GIL to any ready IO-bound threads such as incomming requests
                # There's some debate over whether sleep(0) works as intended on Windows
                sleep(0.0001) 
            return self.pages_text

        except Exception as e:
            log_exception(f"Error getting pdf page text: {e}")
            raise

    @timed()
    def get_chunks_from_pages_simple_text_overlap(
            self, 
            pages: list[str],
            chunk_size: int,
            chunk_overlap: int) -> list[TextChunk]:
        """
        Generate overlapping chunks from PDF pages with no regard to maintaining page boundaries.
        This creates full chunks of text that may span multiple pages.
        """
        log_info(f"get_chunks_from_pdf_stream: chunk_size:{chunk_size} overlap:{chunk_overlap}") 
        log_debug("Creating PdfReader from stream")

        all_text = ""

        try:
            log_info(f"Num pages: {len(self.pages_text)}")
            text_chunker = TextChunker(
                chunk_size, 
                chunk_overlap, 
                remove_extra_whitespace = self.remove_extra_whitespace
            )
            for i, text in enumerate(pages):
                all_text += text
                text_chunker.add_text(i+1, text)

            chunks = [c for c in text_chunker] # custom iterator calls get_next_chunk() until spans exhausted
            return chunks

        except Exception as e:
            log_exception(f"Error processing pdf file: {e}")
            raise

    @timed()
    def get_chunks_from_pages_page_with_overlap(
            self, 
            pages: list[str],
            chunk_size: int,
            chunk_overlap: int) -> list[TextChunk]:
        """
        Generate chunks that cover exactly one page, with overlap/2 context from 
         previous and next pages.
        Does not attempt to combine trivially small pages into larger chunks, but will 
         divide large pages into multiple chunks if needed - although with a default
         of 4000 chars this doesn't usually happen.
        This method will create more chunks than the simple_text_overlap method, but
         will be more accurate in terms of page citations.
        """
        log_info(f"get_chunks_from_pdf_stream: chunk_size:{chunk_size} overlap:{chunk_overlap}") 
        log_debug("Creating PdfReader from stream")

        chunks:list[TextChunk] = []

        try:
            log_info(f"Num pages: {len(self.pages_text)}")

            for i, cur_page_text in enumerate(pages):
                text_chunker = TextChunker(
                    chunk_size, 
                    chunk_overlap, 
                    remove_extra_whitespace = self.remove_extra_whitespace
                )
                if i > 0:
                    prev_page_text = pages[i-1][-chunk_overlap//2:] 
                    text_chunker.add_text(i-1, prev_page_text)

                text_chunker.add_text(i, cur_page_text)

                if i < len(pages)-1:
                    next_page_text = pages[i+1][:chunk_overlap//2]
                    text_chunker.add_text(i+1, next_page_text)

                page_chunks = [c for c in text_chunker] # custom iterator calls get_next_chunk() until spans exhausted
                chunks.extend(page_chunks)

            return chunks

        except Exception as e:
            log_exception(f"Error processing pdf file: {e}")
            raise

    def get_index_chunk_from_text_chunk(self, chunk: TextChunk) -> IndexDocumentChunk:
        token_count = get_num_tokens(chunk.Content)
        return IndexDocumentChunk(
            Content = chunk.Content, 
            PageNumber = chunk.Page, 
            ContentTokenCount = token_count,
            ContentLength = len(chunk.Content),
        )

    # Note that at the moment, we never update the actual document itself -
    #  once the blob is created, the document bits never change - only the metadata.
    # Updating the metadata will change the blob's last_modified time, 
    #  however adtapi metadata sync will only copy the metadata if it's changed.
    # Blob upload/creating and doc twin creation are separate operations but 
    #  should be within seconds of each other (depending on upload time), and doc twin 
    # last update time of blob update time due to metadata sync are on a 2 min sync schedule.
    # doc_last_update_time = blob_props.creation_time

    # TODO: Should we separate summary generation from the index processing 
    #    and make it background/eventual?

    def add_pdf_file_to_index(self, *args, **kwargs):
        try:
            return self._add_pdf_file_to_index(*args, **kwargs)
        except Exception as e:
            log_exception(f"Error adding pdf file to index: {e}")
            raise
        finally:
            gc_collect()

    # TODO: Switch to custom metric output with dimensions for actions taken

    @timed(duration_all_metric_fn = MetricIndexDocument)
    def _add_pdf_file_to_index(
                        self, 
                        file: str, 
                        generate_summaries_mode: GenerateSummaryModeStrEnum = "ifNewer",
                        generate_index_mode: GenerateIndexModeStrEnum = "ifNewer",
                        chunk_size: int|None = None, 
                        chunk_overlap: int|None = None,
                        include_chunkdocs_for_copilot_disabled_files = True,
                        indexing_strategy:str|None = None    
                        ) -> bool:
        """Add the PDF file to the index.
        Depending on options, only update the docChunk index docs if the file is 
        copilot enabled or needs to be reindexed based on update times.
        Also add wholeDoc index docs for the entire document, and use the LLM to
        create docSummary index docs.
        """

        chunk_size = chunk_size or DefaultChunkSize
        chunk_overlap = chunk_overlap or DefaultChunkOverlap

        bops = BlobOps(file)
        blob_props = bops.get_blob_properties()

        if not bops.file_looks_like("pdf"):
            log_info(f"File does not appear to be a PDF - ignoring: '{file}'")
            return False
                
        blob_last_update_time = blob_props.last_modified
        index_chunks, all_text = None, None
        reindex_chunks = generate_index_mode == "force"
        add_whole_doc = generate_index_mode == "force"
        update_metadata = False
        matching_docs, matching_chunk_docs = [], []
        force_delete = False
        need_existing_docs = force_delete or generate_index_mode != "off"

        if blob_props.deleted:
            log_info(f"Blob '{file}' is soft-deleted")
            force_delete = True

        # If we do ever modify the document, we can use the blobs own md5 hash
        #   or our own hash that we compute and store in the metadata and
        #   use the hash in the index and as part of the group/chunk ids.
        # blob_hash2 = doc_metadata.xxx # we now store SHA256 in docTwinMetadata 
        doc_metadata = Metadata.parse_doctwin_metadata(blob_props.metadata, bops.get_blob_name())
        doc_metadata.doc_hash_blob_md5 = base64.b64encode(blob_props.content_settings.content_md5).decode('utf-8') \
                                if blob_props.content_settings.content_md5 else None

        doc_copilot_enabled = doc_metadata.is_copilot_enabled
        log_info(f"Document '{file}' copilotEnabled:{doc_copilot_enabled}")

        if need_existing_docs:

            matching_docs = self.index_ops.find_index_docs(
                # Get skinny docs for file - include docWhole and docSummary as well as docChunks
                file, 
                doc_type = None, 
                select_extra_fields = [ 
                                       FieldName_DocLastUpdateTime, 
                                       FieldName_IndexUpdateTime,
                                       FieldName_TotalDocumentLength, 
                                       FieldName_ContentLength,
                                       FieldName_ItemType
                                    ],
                copilot_enabled_only = False)

            self.check_invariants_for_index_docs(file, matching_docs)

            matching_chunk_docs = [d for d in matching_docs if d[FieldName_ItemType] == ItemType_DocumentChunk]
            matching_whole_docs = [d for d in matching_docs if d[FieldName_ItemType] == ItemType_DocumentWhole]
            add_whole_doc = add_whole_doc or len(matching_whole_docs) == 0

        if generate_index_mode == "ifNewer":

            if len(matching_chunk_docs) == 0:
                # No current index docs - must reindex
                reindex_chunks = True
            else:
                # Get first chunk for doc - representitive of all chunks re:all non-content fields
                doc = matching_chunk_docs[0]

                source_doc_update_time = try_parse_isodate(doc[FieldName_DocLastUpdateTime])
                index_doc_update_time = try_parse_isodate(doc[FieldName_IndexUpdateTime])
                force_reindex_time_str = os.environ.get("FORCE_REINDEX_LASTUPDATETIME")
                force_reindex_time = try_parse_isodate(force_reindex_time_str) if force_reindex_time_str else None
                should_force_reindex = force_reindex_time and index_doc_update_time < force_reindex_time

                if should_force_reindex:
                    log_info(f"Force reindexing '{file}' due to FORCE_REINDEX_LASTUPDATETIME config of {force_reindex_time_str}")
                    reindex_chunks = True
                    add_whole_doc = True # TODO: Questionable - but will make part of contract for now
                elif source_doc_update_time >= blob_last_update_time:
                    content_len = doc[FieldName_ContentLength]
                    log_info(f"Document '{file}' is up to date in index {'(document empty)' if content_len == 0 else ''}")
                else:
                    # Blob file is newer than index - find out if the doc changed or just metadata
                    # Note that currently in TLM/ADTAPI this never happens - a new doc twin
                    #   is created on every upload.
                    # TODO: If we need to support this, better to use hash.
                    index_doc_len = int(doc[FieldName_TotalDocumentLength])
                    cur_doc_len = blob_props.size
                    if index_doc_len != blob_props.size:
                        log_info(f"Document size changed: '{file}': {index_doc_len} -> {cur_doc_len}")
                        log_warning(f"Detected document change - doesn't match current TLM/ADTAPI upload implementation of always creating new document on upload")
                        reindex_chunks = True
                        add_whole_doc = True
                    else:
                        log_info(f"Document '{file}' assuming only metadata changed")
                        update_metadata = True

        if matching_chunk_docs and update_metadata and not reindex_chunks:
            # Merge in new metadata into same content and upsert into existing docs.
            # We need to get all the indexdoc fields and merge because we can't upsert only specific fields
            #  - want to reuse current vector embeddings.
            #  (could be better to just to do this up top instead of getting skinny docs first?)
            matching_docs = self.index_ops.find_index_docs(
                file, 
                doc_type = None, # ItemType_DocumentChunk,
                select_extra_fields = None,
                copilot_enabled_only = False)

            if self.patch_document_metadata_if_needed(matching_docs, blob_props) > 0:
                log_info(f"Upserting metadata in-place for '{file}'")
                self.index_ops.upload_documents(matching_docs)

        elif force_delete:
            log_info(f"Force deleting all {len(matching_docs)} index docs for '{file}'")
            self.index_ops.delete_documents(matching_docs)
            return True
        elif matching_chunk_docs and (reindex_chunks or not doc_copilot_enabled):
            # Document contents have changed - delete old chunks
            log_debug(f"Deleting for reindex {len(matching_docs)} index chunks docs for '{file}'")
            self.index_ops.delete_documents(matching_chunk_docs)

        # Get document contents lazily 
        def get_doc_contents() -> Tuple[list[IndexDocumentChunk], str]:
            nonlocal all_text, index_chunks
            if all_text and index_chunks: 
                return index_chunks, all_text
            index_chunks, all_text = self.get_chunks_from_pdf_blob( bops, chunk_size, chunk_overlap)
            if len(index_chunks) == 0 or len(all_text) == 0:
                log_warning(f"No text found for '{file}'")
            return index_chunks, all_text

        # Note all_text below is as-is from extraction - extra whitespace has not been
        #   removed as we do for chunks. We could remove extra whitespace here as well
        #   - this is not the same as concatenating the chunks due to overlap.
        if reindex_chunks and (doc_copilot_enabled or include_chunkdocs_for_copilot_disabled_files):
            index_chunks, all_text = get_doc_contents()
            # TODO: Add Status field to index so we can mark as failed?
            if len(index_chunks) == 0 or len(all_text) == 0:
                log_warning(f"Creating single empty chunk for no-text file '{file}'")
                # Create empty chunk for no-text file so we don't try and re-index every time
                index_chunks = [
                    IndexDocumentChunk(
                        Content = "", PageNumber = 0, 
                        ContentTokenCount = 0, ContentLength = 0)
                    ]
            self._add_chunks_to_index(
                 bops, doc_metadata, 
                 chunk_size, chunk_overlap, index_chunks)

        # TODO: We don't update whoDoc if doc is updated - this never happens at the moment, but should handle
        # TODO: Refactor out is_index_doc_newer_than and treat equally for all doc types
        if add_whole_doc:
            index_chunks, all_text = get_doc_contents()
            log_debug(f"Adding whole document '{file}' to index")
            # Overwrite any existing docWhole -- even for copilot disabled files
            # NOTE: Allow no-text whole doc to be indexed if we can't parse it
            self.index_ops.add_whole_index_document(
                text = all_text, 
                file = bops.get_blob_name(), 
                doc_last_update_time = bops.get_blob_properties().last_modified, 
                item_type = ItemType_DocumentWhole, 
                metadata = json.dumps(blob_props.metadata),
                summary_type = None)

        # Overwrite any existing docSummary if needed -- even for copilot disabled files
        #TODO: We're now getting all doc types - we can pass in the summary docs now
        self.generate_summary_if_needed(
            file = file, 
            get_text_fn = get_doc_contents,
            metadata = json.dumps(blob_props.metadata),
            blob_last_update_time = blob_last_update_time,
            mode = generate_summaries_mode,
            update_metadata_only = update_metadata)

        log_info(f"Add to index processing complete for '{file}'")
        return reindex_chunks


    def check_invariants_for_index_docs(self, file:str, docs: List[dict]):

        n_summary_docs = len([d for d in docs if d[FieldName_ItemType] == ItemType_DocumentSummary])
        if n_summary_docs > 1:
            log_warning(f"Multiple summaries found for '{file}'")

        n_whole_docs = len([d for d in docs if d[FieldName_ItemType] == ItemType_DocumentWhole])
        if n_whole_docs > 1:
            log_warning(f"Multiple whole docs found for '{file}'")

    def generate_summary_if_needed(self, 
                                   file: str,
                                   get_text_fn: Callable[[], Tuple[list[IndexDocumentChunk], str]],
                                   blob_last_update_time: datetime.datetime,
                                   mode: GenerateSummaryModeStrEnum = None,
                                   metadata: str|None = None,
                                   update_metadata_only: bool = False,
                                   summary_type: str = "basic") -> bool:

        #TODO: We could also update the summary if missing
        #  doctwin.summary or doctwin.customProperties.copilot.llm_summary 
        index_doc_update_time = None
        existing_docs = []

        mode = mode or "ifNewer"

        # TODO: extract all configs to consts. prefix w/COPILOT?
        if os.environ.get("SUMMARY_GENERATION_ENABLED", "true").lower() != "true":
            log_info(f"SummaryExit: Summary generation is disabled in config - not generating summary for '{file}'")
            return False

        if mode == "off":
            log_info(f"SummaryExit: Summary generation mode is 'off' - not generating summary for '{file}'")
            return False

        force_reindex_time_str = os.environ.get("FORCE_SUMMARY_LASTUPDATETIME")
        force_reindex_time = try_parse_isodate(force_reindex_time_str) if force_reindex_time_str else None

        if mode == "ifNewer":

            if not blob_last_update_time:
                raise ValueError("doc_last_update_time must be provided when mode is 'ifNewer'")

            existing_docs = self.index_ops.find_skinny_docs_for_summary(file)
            log_debug(f"Found {len(existing_docs)} existing summary docs for '{file}'")

            have_summary_doc = len(existing_docs) > 0
            if have_summary_doc:

                if len(existing_docs) > 1:
                    log_warning(f"Found multiple summary docs for '{file}'")

                existing_doc = existing_docs[0]
                index_doc_update_time = try_parse_isodate(existing_doc[FieldName_DocLastUpdateTime])
                index_doc_update_time = try_parse_isodate(existing_doc[FieldName_IndexUpdateTime])
                if force_reindex_time and index_doc_update_time < force_reindex_time:
                    log_info(f"Force summarization of '{file}' due to FORCE_SUMMARY_LASTUPDATETIME config of {force_reindex_time_str}")
                    have_summary_doc = False
                # check if summary is newer than existing
                elif blob_last_update_time <= index_doc_update_time:
                    log_info(f"SummaryExit: Summary for '{file}' is up to date")
                    return False
                elif update_metadata_only:
                    log_info(f"SummaryExit: Blob file is newer than summary for '{file}' but only metadata changed - skipping summarization")
                    return False

                if have_summary_doc:
                    return False
            else:
                log_info(f"SummaryContinue: No existing summary docs for '{file}'")

        # TODO: SummarySkipped metric, or combine here?
        log_info(f"SummaryContinue: '{file}':" 
                 f"mode:{mode}, doc_last_update_time:{blob_last_update_time}, "
                 f"source_doc_update_time:{index_doc_update_time}, "
                 f"num_existing_docs:{len(existing_docs)}")   

        duration = 0
        cached_summary = None
        if mode == "ifNewer":
            try:
                # Check if we've saved the summary in the blob cache
                # TODO: We could use the hash of the LLM prompt text to keep multiple summaries for the same doc
                cached_name = f"_summary_{file}"
                summary_blob_container = os.environ.get("SUMMARY_CACHE_BLOB_CONTAINER") or "copilot"
                cache_bops = BlobOps(file = cached_name, container = summary_blob_container)
                exists = cache_bops.get_blob_client().exists()
                if exists:
                    if force_reindex_time and cache_bops.get_blob_properties().last_modified < force_reindex_time:
                        log_info(f"Summary found in blob-cache but older than FORCE_SUMMARY_LASTUPDATETIME - ignoring cached summary for '{file}'")
                    else:
                        duration, cached_summary = elapsed_ms(cache_bops.read_all) 
                        if not cached_summary:
                            log_warning(f"Cached summary found but empty for '{file}'")
                else:
                    log_debug(f"No cached summary found for '{file}'")
            except Exception as e:
                log_exception(f"Error attempting to read cached summary for '{file}'")
                # Continue processing, as this is just an optimization

        if cached_summary:
            log_info(f"Using blob-cached summary for '{file}'")
            summary = cached_summary
        else:
            _chunks, text = get_text_fn()
            summarizer = DocumentSummarizer(text=text, use_large_model = True, file = file)
            duration, summary = elapsed_ms( summarizer.summarize)

        MetricCreateSummary(
            duration, 
            ok = not summary_is_error(summary),
            summary_mode = mode, 
            from_cache = bool(cached_summary))

        # Note: Summary is now always returned, but may start with "SummaryNotAvailable:" if there was an error
        if summary:

            if not cached_summary:
                log_info(f"Writing summary to blob-cache for '{file}'")
                try:
                    cache_bops.write_all(summary)
                except Exception as e:
                    log_exception(f"Error writing summary to blob-cache for '{file}'")
                    # Continue processing, as this is just an optimization

            # Note we are caching SummaryError as well - so won't try to regenerate from same doc that caused problem
            self.index_ops.add_whole_index_document(
                        text = summary, 
                        file = file, 
                        doc_last_update_time = blob_last_update_time, 
                        item_type = ItemType_DocumentSummary, 
                        metadata = metadata,
                        summary_type = summary_type) 
            return True

        return False

    def get_chunks_from_pdf_blob(self, 
                                bops : BlobOps, 
                                chunk_size, 
                                overlap,
                                indexing_strategy:str|None = None
                                ) -> tuple[list[IndexDocumentChunk], str]:

        try:
            indexing_strategy = indexing_strategy or IndexingStrategy_SimpleTextOverlap
            file = bops.blob_name
            log_info(f"Processing pdf file: '{file}' for index '{self.index_ops.index_name} using strategy '{indexing_strategy} ")

            stream = bops.get_blob_stream()
            if not self.can_parse(stream):
                log_warning(f"Could not parse pdf file: {file}")
                # TODO: throw something better
                raise Exception(f"Could not parse pdf file: '{file}'")

            pages = self.get_text_from_pages(stream)
            all_text = ''.join(pages)

            if indexing_strategy == IndexingStrategy_SimpleTextOverlap:
                chunks = self.get_chunks_from_pages_simple_text_overlap(pages, chunk_size, overlap)
            elif indexing_strategy == IndexingStrategy_PageWithOverlap:
                chunks = self.get_chunks_from_pages_page_with_overlap(pages, chunk_size, overlap)
            else:
                raise ValueError(f"Invalid indexing strategy: '{indexing_strategy}'")

            log_info(f"Adding {len(chunks)} chunks to index for '{file}'")
            for (i, c) in enumerate(chunks):
                log_debug(f"File:'{file}', Chunk {i}, page:{c.Page}, len:{len(c.Content)} ({get_num_tokens(c.Content)})")

            index_chunks = [self.get_index_chunk_from_text_chunk(c) for c in chunks]

        finally:
            try:
                if stream: stream.close()
            except Exception as e:
                log_warning(f"Error closing pdf stream: {e}")

        return index_chunks, all_text

    @timed()
    def _add_chunks_to_index(
                        self, 
                        bops: BlobOps, 
                        known_metadata: DocumentMetadata,
                        chunk_size: int, overlap: int,
                        index_chunks: list[IndexDocumentChunk]) -> int:

        processing_parameters = {
            "doc-parser": "pypdf",
            "pypdf-extraction-mode": PyPdfExtractionMode,
            "remove-extra-whitespace": self.remove_extra_whitespace,
            "version": "1.0",
            "chunk-size": chunk_size,
            "chunk-overlap": overlap,
        }

        blob_props = bops.get_blob_properties()

        index_doc_info = IndexDocumentInfo(
            Title = bops.get_blob_name(),
            # We've already make the uri's name-safe when uploaded by twinsapi. If we decide to use 
            #  encoded form here (will put %20's in spaces,etc.) then we need to make
            #  sure that when we search for chunk/smmary docs that we encode in the filter as well
            Uri = bops.get_blob_uri_unencoded(),
            RequestedChunkSize = chunk_size,
            TotalDocumentNumChunks = len(index_chunks),
            TotalDocumentLength = blob_props.size,
            DocLastUpdateTime = blob_props.last_modified,
            IndexerSource = "py-indexer",
            CopilotEnabled = known_metadata.is_copilot_enabled,
            # TODO: should this come from customTag and customProperties? New field for latter?
            FilterTags = known_metadata.doc_custom_tags,

            # dump as json string of dict - will be url encoded
            ProcessingParameters = json.dumps(processing_parameters),
            DocUnstructuredMetadata = json.dumps(blob_props.metadata),
        )

        log_info(f"Adding {len(index_chunks)} chunks to index for '{bops.get_blob_name()}'")    

        self.index_ops.add_doc_index_chunks_to_index(index_chunks, index_doc_info, file = bops.get_blob_name())
        return len(index_chunks)

    def patch_document_metadata_if_needed(self, docs: List[dict], blob_props: BlobProperties) -> int:
        """ Update existing index documents with new metadata so it can be upserted.
        Only update if metadata has changed - return the number of docs updated.
        (Should be either 0 or len(docs))
        """
        # TODO: Unless we need to be more fine-grained, we could just compare metadata strings?
        # metadata_diff['values_changed']["root['documentTwinMetadata']"]['new_value']
        # If we want to see more fine-grained changes, we loads chagned docTwinMetadata and do the diff there
        #  rather than just at the top-level

        n_docs_updated = 0
        last_metadata_changed = {}
        new_metadata = blob_props.metadata
        new_metadata_json = None
        doc_metadata:DocumentMetadata = Metadata.parse_doctwin_metadata(
                                            blob_props.metadata, blob_props.name)

        if not docs or len(docs) == 0:
            log_warning(f"No docs to patch for '{blob_props.name}'")
            return 0

        if len(docs) > 0:
            doc = docs[0]
            vec = doc[FieldName_Vector]
            if not vec:
                log_warning(f"Document '{doc[FieldName_Title]}' of type {doc[FieldName_ItemType]} has no vector embedding in {FieldName_Vector}")

        for doc in docs:

            doc[FieldName_DocLastUpdateTime] = blob_props.last_modified
            old_metadata = json.loads(doc[FieldName_DocUnstructuredMetadata])
            #metadata_changed = new_metadata != old_metadata
            metadata_diff = deepdiff.DeepDiff(old_metadata, new_metadata)
            # Returns a dict ({} if no changes) with keys 'values_changed', 'dictionary_item_added', 'dictionary_item_removed'  
            if metadata_diff:
                if not new_metadata_json: new_metadata_json = json.dumps(new_metadata)
                # TODO: Should share code w/_add_doc_chunks_to_index
                doc[FieldName_DocUnstructuredMetadata] = new_metadata_json
                doc[FieldName_CopilotEnabled] = doc_metadata.is_copilot_enabled
                doc[FieldName_FilterTags] = doc_metadata.doc_custom_tags
                if last_metadata_changed != metadata_diff:
                    log_debug(f"Updating changed metadata for doc: {doc[FieldName_Title]}: {metadata_diff}")
                    last_metadata_changed = metadata_diff
                n_docs_updated += 1

        if n_docs_updated != 0 and n_docs_updated != len(docs):
            # All chunks should have the same metadata
            log_warning("nDocs with changed metadata should be 0 or all docs")

        return n_docs_updated

    def is_rebuild_in_progress(self) -> bool:
        return RebuildInProgressStartTime is not None

    @timed()
    def rebuild_index(
                self, 
                delete_and_recreate_index = True,
                generate_summaries_mode : GenerateSummaryModeStrEnum | None = "ifNewer",
                generate_index_mode : GenerateIndexModeStrEnum | None = "ifNewer",
                chunk_size: int|None = None,
                chunk_overlap: int|None = None,
                include_chunkdocs_for_copilot_disabled_files: bool | None = True,
                num_threads: int|None = None,
                indexing_strategy:str|None = None
                ) -> int:
        """Rebuild the index by processing each blob file in the container.
        Add doc chunks, whole docs, and LLM-generated summaries to the index,
          depending on force|ifNewer|off settings.
        """

        global RebuildInProgressStartTime
        if RebuildInProgressStartTime:
            log_warning(f"Index rebuild already in progress (started {RebuildInProgressStartTime.isoformat()}) - skipping this request")
            # TODO: Should we have a max_time here in case the previous one is hung or improperly reported?
            return 0

        try:
            RebuildInProgressStartTime = datetime.datetime.now()
            generate_summaries_mode = generate_summaries_mode or "ifNewer"
            generate_index_mode = generate_index_mode or "ifNewer"
            include_chunkdocs_for_copilot_disabled_files = default(include_chunkdocs_for_copilot_disabled_files, False)

            MetricIndexRebuildRequest(True,
                                delete_and_recreate = delete_and_recreate_index,
                                summary_mode = generate_summaries_mode,
                                index_mode = generate_index_mode)

            if delete_and_recreate_index:
                self.index_ops.delete_index()
            self.index_ops.create_or_update_index()

            if generate_index_mode == "off" and generate_summaries_mode == "off":
                log_debug("Rebuild index: No work to do")
                return 0

            n_cores = multiprocessing.cpu_count()
            n_workers = num_threads or min(4, int(os.environ.get("COPILOT_INDEXING_NUMTHREADS") or n_cores))
            log_debug(f"Rebuilding index with {n_workers} threads")
            n_docs_processed = 0

            # TODO: Use new twinsapi SQL based jobs api when avail?
            # TODO: Support cancellation?
            # TODO: Get nested OpenTelemetry logs working so we can scope in the threadid

            def work(job: tuple[int, BlobProperties]):
                i, blob = job
                tid = threading.current_thread().native_id
                log_info(f"Index all file worker (thread {tid}): {i:3}/{len(blob_list)}: {int(blob.size/1024):>8}K : {blob.name}")
                # TODO: Pass in file/blob to c'tor and instantiate per-file
                pp = PyPdfIndexProcessor(self.index_ops.index_name)
                try:
                    # add_pdf_file_to_index also emits separate metric for each document
                    pp.add_pdf_file_to_index(
                        blob.name,
                        generate_summaries_mode = generate_summaries_mode,
                        generate_index_mode = generate_index_mode,
                        chunk_size = chunk_size,
                        chunk_overlap = chunk_overlap,
                        include_chunkdocs_for_copilot_disabled_files = include_chunkdocs_for_copilot_disabled_files,
                        indexing_strategy = indexing_strategy
                    )
                except Exception as e:
                    log_exception(f"Aborting thread: IndexAll (thread:{tid}): Error processing '{blob.name}'")
                    raise # No effect as no more work to do and won't affect main thread

                log_info(f"Index all file worker complete: IndexRebuild->IndexDocument (index:{i}, thread:{tid}) '{blob.name}'")
                nonlocal n_docs_processed
                n_docs_processed += 1

            bops = BlobOps()
            blob_list = bops.get_blob_list()
            log_info(f"rebuild_index: found {len(blob_list)} documents to process")
            # Process short docs first, for no good reason than faster inital processing of files when debugging
            blob_list = sorted(blob_list, key = lambda b: b.size)
            # TODO: Is this threadpool shared w/Flask's for incomming requests? 

            start = time.time()
            # Index all files in threadpool 
            with ThreadPoolExecutor(max_workers = n_workers) as executor:
                worklist = enumerate(blob_list)
                executor.map(work, worklist)
            duration_ms = int((time.time() - start) * 1000)

            MetricIndexRebuildComplete(duration=duration_ms, ok=True,
                                        delete_and_recreate = delete_and_recreate_index,
                                        summary_mode = generate_summaries_mode,
                                        index_mode = generate_index_mode)

        finally:
            RebuildInProgressStartTime = None

        log_info(f"Index rebuild complete: {n_docs_processed} documents processed")
        return n_docs_processed


if __name__ == '__main__':  

    #sys.path.append( os.getcwd())
    #sys.path.append(os.path.dirname(os.path.abspath(__file__ + '/../')))
    #sys.path.append(os.path.dirname(os.path.abspath(__file__ + '/../../')))

    #index_name = "index_from_python_test"
    index_name = "pyn-documents-wil-dev"
    pp = PyPdfIndexProcessor(index_name)

    if True:
        #file = "Krack Coil Selection Guide for Evaporators.pdf"
        #file = "ll97of2019.pdf" # smaller at 600K
        #file = "https://stodeveus01wili3c46beea.blob.core.windows.net/twindocuments/ll97of2019.pdf"
        #file = 'troubleshooting-guide.pdf'
        #file = 'Krack Refrigeration Load Estimating Manual.pdf'
        file = "3086855_MPCETNM6S_SC_IO_EN Hussman ref case installation and operations manual.pdf"

        if True:
            pp.add_pdf_file_to_index(
                file,
                generate_summaries_mode = "off",
                generate_index_mode = "force",
            )

        if False:
            pp.rebuild_index(
                delete_and_recreate_index = False, 
                generate_summaries_mode = 'off',
                generate_index_mode = 'force',
                num_threads = 1
            )
            
            
