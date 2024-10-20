
import json
import os
from typing import List, Tuple
import re
from regex import D
import toolz
import pprint

from azure.search.documents import SearchClient

from langchain_core.vectorstores import VectorStoreRetriever, BaseRetriever
from langchain_core.documents import Document

from shared.OpenTelemetry import log_debug, log_info, log_warning
from shared.ServicesWrapper import ServicesWrapper
from shared.indexing.SearchIndexConfig import FieldName_ContentTokenCount, FieldName_DocUnstructuredMetadata, FieldName_GroupId, FieldName_Id, FieldName_PageNumber, FieldName_Title, FieldName_Content, FieldName_TotalDocumentNumChunks
from shared.Utils import pluck, timed, get_num_tokens, DebugMode
from shared.Prompts import DocumentChunkPrompt
from shared.Metadata import Metadata

from langchain.memory.chat_memory import BaseChatMemory

# TODO: detect incompatable documents based on metadata? alert user or re-rank?-
# TODO: rename Title to Filename in index (or also keep Title and get from metadata and/sep doctwin prop)

DefaultExpandChunksPercentTokens = 50

def add_normalized_doc_citation(doc: Document) -> None:
    """Create a single citation string from the document metadata and add it to the document.
    This combines title (and page if avail) into a single prop for the document Template to use
    when generating the context doc chunks.
    """
    title = doc.metadata["Title"]
    # Remove unique protion of blob name, if any (file.pdf$123123 -> file.pdf)
    title = title.split('$')[0]

    page = doc.metadata.get("PageNumber") 
    #citation = f"<<@citation title:'{title}', page:{page}>>" if page else f"<<@citation title:'{title}'>>"
    citation = f"{{title:'{title}', page:{page or 'unknown'}}}" 
    doc.metadata[DocumentChunkPrompt.VirtualFieldnameCitation] = citation


def add_extra_metadata(doc: Document, include:bool = True) -> None:
    """Add extra metadata to the document for use in the Template when generating the context doc chunks.
    For the moment, this is just the doctwin's description prop - we should use it to
    list the manufacturer, model, and other meaningful info about the asset in question.
    """
    if not include:
        doc.metadata[DocumentChunkPrompt.VirtualFieldnameMetadata] = DocumentChunkPrompt.MissingMetadataValue
        return

    metadata_str = doc.metadata[FieldName_DocUnstructuredMetadata]
    metadata = json.loads(metadata_str) if metadata_str else {}
    well_known_metadata = Metadata.parse_doctwin_metadata(metadata, doc.metadata[FieldName_Title])
    #TODO: Get from new custom properties assetInfo/extra_metadata
    doc.metadata[DocumentChunkPrompt.VirtualFieldnameMetadata] = well_known_metadata.doc_description or DocumentChunkPrompt.MissingMetadataValue

class CustomVectorStoreRetriever(BaseRetriever): 

    # Note we can't use an init method or pydantic will complain
    #  so init using CustomVectorStoreRetriever(base_retriever=...)

    base_retriever: VectorStoreRetriever
    max_tokens: int 
    expand_chunks_percent_tokens: int = 0 #TODO: temp to 50%?
    add_chunk_metadata: bool = False
    # It's a hack to pass the memory in here - would rather get from chain metadata via sessionId, but not in any args here 
    # TODO: Look at:  history: ChatMessageHistory = self.memory.chat_memory (HumanMessage/AIMessage list)
    #   Apply mmemory to partial of main template and count tokens?
    memory: BaseChatMemory

    chunk_id_regex = r"(docChunk-.+-)([0-9]+)$"

    def get_doc_chunk_index(self, doc: Document) -> int:
        #TODO: May want to add chunk_index as index field
        id = doc.metadata[FieldName_Id]
        chunk_index = -1 
        match = re.match(self.chunk_id_regex, id)
        if match:
            chunk_id = match.group(2)
            chunk_index = int(chunk_id)
        else:
            log_warning(f"CustomVectorStoreRetriever: doc id pattern doesn't match - assuming old indexer format")
        return chunk_index

    @timed()
    def fetch_extra_context_chunks(
                    self, 
                    docs: list[Document], 
                    total_tokens_used: int) -> tuple[int, list[Document]]:
        """Fetch any neighboring context chunks for documents to provide more context.
        """

        expand_percent = self.expand_chunks_percent_tokens \
                    if self.expand_chunks_percent_tokens != 0 else DefaultExpandChunksPercentTokens
        if expand_percent < 0 or expand_percent > 100:
            raise ValueError(f"expand_chunks_percent_tokens must be between 0 and 100: {expand_percent}")
        max_tokens = self.max_tokens * expand_percent // 100

        # Extend docs in a score-preferred manner - docs already sorted by score, highest first.
        # Docs extended: 1,  1, 2,  1, 2, 3 ...
        # Keep doing this until we've filled the alloted token context 
        for _phase in range(10):
            for i_doc in range(len(docs)):
                for j_doc in range(1+i_doc):
                    extend_doc = docs[j_doc]
                    tokens_added, new_docs = self.extend_chunk( extend_doc, docs, 1) 
                    total = tokens_added + total_tokens_used 
                    log_info(f"CustomVectorStoreRetriever: fetch_extra_context_chunks: added {tokens_added} for new total {total} (overflow: {total > self.max_tokens})")    
                    if total > max_tokens: 
                        return total, docs
                    total_tokens_used, docs = total, new_docs
                    if DebugMode:
                        print("\n===================\n")
                        d = toolz.reduceby(lambda d:d.metadata["Title"], 
                                           lambda x,y: [*x, self.get_doc_chunk_index(y)], 
                                           docs, [])
                        pprint.pprint(d)
                        #for d in docs: print((f"{d.metadata[FieldName_Title]}", self.get_doc_chunk_index(d)))

        # TODO: we could to the same loop with direction -1 to get the previous chunks

        return total_tokens_used, docs


    @timed()
    def extend_chunk(self, doc: Document, docs: list[Document], direction = 1) -> tuple[int, list[Document]]:
        docs = docs.copy()
        ci = self.get_doc_chunk_index(doc)
        new_ci = ci + direction
        #index = self.base_retriever.vectorstore.client._index_name
        #index_ops = self.get_index_ops(index)
        search_client:SearchClient = self.base_retriever.vectorstore.client
        num_doc_chunks = doc.metadata.get(FieldName_TotalDocumentNumChunks) or 1000

        log_info(f"CustomVectorStoreRetriever: extend_chunk {ci} for {doc.metadata[FieldName_Title]} (dir={direction})")

        i = 0
        while i < 1000 and new_ci >= 0 and new_ci < num_doc_chunks:
            new_id = f"{doc.metadata[FieldName_GroupId]}-{new_ci}"
            found_chunk = next(filter(lambda d: d.metadata[FieldName_Id] == new_id, docs), None)
            if not found_chunk:
                break
            new_ci += direction
        else:
            return 0, docs

        log_info(f"CustomVectorStoreRetriever: extend_chunk: retreiving chunk {new_ci} for {doc.metadata[FieldName_Title]} (dir={direction})")

        try:
            new_index_doc = search_client.get_document(new_id)
            content:str = new_index_doc[FieldName_Content]
            new_doc = Document(
                page_content = content, 
                metadata = new_index_doc)
            i = docs.index(doc)
            docs.insert(i+direction, new_doc)
            n_tokens = get_num_tokens(content)
            return n_tokens, docs
        except Exception as e:
            log_info(f"CustomVectorStoreRetriever: extend_chunk: failed to get chunk {new_ci} for {doc.metadata[FieldName_Title]}")
            return 0, docs

    # This is the main overide method for the retriever - note we don't use the non-underscored version
    def _get_relevant_documents( self, query: str, *, run_manager, **kwargs) -> List[Document]:

        k, search_type, filters = pluck(self.base_retriever.search_kwargs, 'k', 'search_type', 'filters')
        log_debug(f"CustomVectorStoreRetriever: (max_tokens:{self.max_tokens})  k: {k}, search_type: {search_type}, filters: '{filters}', query: '{query}'")

        # NOTE HACK: This is to workaround an apparent recent LC bug where the search_kwargs are not 
        #  being saved in the retriever we store them in the vectorstore and then copy them here.
        self.base_retriever.search_kwargs = self.base_retriever.vectorstore.search_kwargs

        # TODO: Exactly filling the context with going over is hard - would need to take
        #   chat history and all prompts into account 
        # We could just truncate at the end of the llm chain, assuming we move the context to the end
        # OTOH, if we have extra room, we can do another query to expand our the context
        #   by getting neighboring chunks, or get the whole doc checking TotalDocumentLength
        # Heuristics - if we have chunks for doc D and also whole doc and whole doc fits
        #   remove chunks and add whole doc? Only if no other docs are present in top k?

        # The actual query happens in line 493 of site-packages\langchain_community\vectorstores\azuresearch.py
        # Note that k docs and the query are set to be the same for the keyword search
        #   and the index search, though that doesn't need to be the case.
        # We could do a self-query and metadata analysis and provide distinct queries for each.
        # The langchain query also creates its own embeddings for the query and does
        #   not make use of the index vectorizer
        docs = self.base_retriever.get_relevant_documents(query, run_manager=run_manager)
        # Should already be the case
        # docs = sorted(docs, key = lambda x: float(x.metadata["@search.score"]), reverse=True)

        # TODO: Can add re-ranking step here

        total_tokens, total_doc_tokens, final_docs = 0, 0, []
        for i, d in enumerate(docs):

            #doc_tokens = int(d.metadata[FieldName_ContentTokenCount])
            doc_tokens = int(d.metadata[FieldName_ContentTokenCount])  \
                        if d.metadata[FieldName_ContentTokenCount] is not None \
                        else len(d.page_content) // 3
            doc_type = d.metadata.get("ItemType") or "docChunk"

            log_debug(f"{doc_type} {i} '{d.metadata[FieldName_Title]}':, doc_tokens: {doc_tokens}, total_tokens_used: {total_tokens}")

            if doc_type != "docChunk":
                # We're pre-filtering at the moment, so shouldn't see these
                log_debug(f"{doc_type} {i} OMITTED type {doc_type}")
                continue

            total_tokens += doc_tokens
            if total_tokens > self.max_tokens:
                log_debug(f"{doc_type} {i} OMITTED - too many tokens: {total_tokens} > {self.max_tokens}")
                # could break here, but want to log what we're missing
                continue

            final_docs.append(d)
            total_doc_tokens += doc_tokens

        total_doc_tokens, final_docs = \
            self.fetch_extra_context_chunks(final_docs, total_doc_tokens)

        for d in final_docs:
            add_normalized_doc_citation(d)
            add_extra_metadata(d, self.add_chunk_metadata)

        # TODO: Make these runflags
        # Experiment - highest scoring last to take advantage of recency bias
        if False:
            final_docs = sorted(final_docs, 
                            key = lambda x: float(x.metadata["@search.score"]), 
                            # reverse=False in the case is a reverse sort by score - lowest (worst) to best
                            reverse = False)

        # Now that we've kept the highest scoring docs we have room for, 
        #  put them back in natural order sorted by doc and page number.
        # Probably won't matter much unless we have consentive chunks.
        # (If we want to get really fancy, we could remove any repeated overlapping text 
        #   if we have adjacent chunks - but probably won't have much effect until we
        #   start intepreteting tables, assuming they aren't already semantically chunked.)
        if True:
            def sort_key(doc: Document) -> Tuple[str, int]:
                #order = doc.metadata.get(FieldName_PageNumber) or 0
                order = self.get_doc_chunk_index(doc)
                return (doc.metadata[FieldName_Title], order)
            final_docs = sorted(final_docs, key = sort_key)

        return final_docs

    # Combine all citation-related metadata into a single _Citation field.
    # Modify document metadata dict in place.


#TODO: in progress
class CustomHybridRetriever(BaseRetriever): 

    # Note we can't use an init method or pydantic will complain
    #  so init using CustomVectorStoreRetriever(base_retriever=...)
    base_retriever: VectorStoreRetriever 
    services_wrapper : ServicesWrapper 

    def _get_relevant_documents( self, query: str, *, run_manager) -> List[Document]:

        search_client = self.services_wrapper.get_search_client()
        vector_store = self.services_wrapper.get_vector_store()

        vec_docs = self._vector_store.similarity_search(
            query = query,
            k = 1, 
            search_type = "similarity"
            # filter = {...}
        )

        vector_store.search(
            search_text = query
        )

        results = search_client.search(
            search_text = query
        )

        docs = []

        for d in docs:
            add_normalized_doc_citation(d)

        return docs

    # Combine all citation-related metadata into a single _Citation field.
    # Modify document metadata dict in place.
    def normalize_doc_citation(self, doc: Document) -> None:

        title = doc.metadata["Title"]
        page = doc.metadata.get("PageNumber") or "unknown"

        citation = f"<<title:'{title}', page:{page}>>"  

        doc.metadata["_Citation"] = citation

