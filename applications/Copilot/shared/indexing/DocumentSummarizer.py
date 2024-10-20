
from types import MethodDescriptorType
from typing import List
import sys
import itertools

from shared.Utils import timed, get_num_tokens
from shared.indexing.BlobOps import BlobOps
from shared.indexing.TextChunker import TextChunk, TextChunker

from langchain.chains.summarize import load_summarize_chain
from langchain_core.documents import Document

from shared.ServicesWrapper import ServicesWrapper
from shared.OpenTelemetry import log_info, log_debug, log_error, log_exception, log_warning
from shared.Prompts import SummarizeDocumentPrompt

from langchain import hub
from langchain.chains.llm import LLMChain
from langchain.chains import StuffDocumentsChain, MapReduceDocumentsChain, ReduceDocumentsChain
from langchain.prompts import PromptTemplate
from langchain_text_splitters import RecursiveCharacterTextSplitter, CharacterTextSplitter

SummaryErrorPrefix = "SummaryNotAvailable:"
SummaryGeneratedEmpty = SummaryErrorPrefix + " Summary was empty"
SummaryNoContent = SummaryErrorPrefix + " No content found in document"
SummaryError = SummaryErrorPrefix + " Error generating summary"

def summary_is_error(summary:str) -> bool:
    return not summary or summary.startswith(SummaryErrorPrefix)


class DocumentSummarizer:

    def __init__(self, 
                 text: str = None, 
                 chunks: List[TextChunk] = None,
                 use_large_model = True,
                 file:str = None):

        self.services = ServicesWrapper()
        self.stuff_chain = None
        self.llm = self.services.get_llm_large() if use_large_model else self.services.get_llm_small()
        self.llm_max_tokens = self.services.get_llm_max_tokens(is_large = use_large_model)
        self.text = text
        self.chunks = chunks
        self.file = file

        self.llm_max_tokens = self.services.get_llm_max_tokens(is_large = False)

        if self.text is None and self.chunks is None:
            raise ValueError(f"DocumentSummarizer: text or chunks must be provided for '{self.file}") 
        if self.text and self.chunks:
            raise ValueError(f"DocumentSummarizer: text and chunks cannot both be provided for '{self.file}") 

    # Default summarization method for now is to just use the first 
    # 8K (convervatively 32K tokens) of the document, as that's where the most important information is likely to be. 
    # We should re-chunk into 32k token (-prompt) lengths, and then MapReduce will perform beter as well
    def summarize(self, method = "stuff") -> str | None:

        summary: str = None
        if self.text is None:
            raise ValueError(f"DocumentSummarizer: initialize with text ({self.file})")
        if self.text == "":
            log_warning(f"summarize: Document length is zero for '{self.file}' - nothing to do")
            return SummaryNoContent

        log_info(f"Attempting summarize '{self.file} with '{method}' chain")
        
        try:
            if method == "stuff":
                summary = self.summarize_stuff(chars_max = self.llm_max_tokens * 2)
            elif method == "map_reduce":
                # TODO: mapreduce needs testing
                summary = self.summarize_map_reduce()
            else:
                raise ValueError(f"DocumentSummarizer: unknown summarization method: {method}")
        except Exception as e:
            log_error(f"summarize_stuff failed for '{self.file}'  {e}")
            summary = SummaryError

        return summary

    def rechunk_text(self, text:str, chunk_size:int, max_chunks:int) -> List[Document]:

        tc = TextChunker(
            chunk_size = chunk_size, 
            overlap = 50)
        tc.add_text(1, text)
        return [
            Document(page_content = c.Content) for c in itertools.islice(tc, max_chunks)
        ]

    def get_documents(self, chunk_size = None, max_chunks = None) -> list[Document]:

        docs = None
        max_chunks = max_chunks or sys.maxsize

        metadata = {}
        if self.chunks:
            # TODO: re-code this path
            if False: # use original chunks
                docs = [
                    Document(
                        #page_content = f"Page {chunk.Page}: {chunk.Content}\n\n", 
                        page_content = f"{chunk.Content}\n", 
                        metadata = metadata
                    ) for chunk in self.chunks]   
            else: # rechunk
                # Could also include Page if we want to summary to reference them
                text = "".join( [chunk.Content for chunk in self.chunks] )
                docs = self.rechunk_text(text, chunk_size, max_chunks)
        else:
            docs = self.rechunk_text(self.text, chunk_size, max_chunks)

        for i, doc in enumerate(docs):
            log_debug(f"Summary document chunk {i} tokens: {get_num_tokens(doc.page_content)}")

        return docs
                
        """ Note these make chunks from \n\n if any and then \n - chunks are too small for mapreduce 
        #text_splitter = CharacterTextSplitter.from_tiktoken_encoder( chunk_size = 100, chunk_overlap = 10)
        text_splitter = RecursiveCharacterTextSplitter(
            chunk_size=100,
            chunk_overlap=20,
            length_function=len,
            is_separator_regex=False,
        )
        split_docs = text_splitter.split_documents([doc])
        """

    @timed()
    def summarize_stuff(self, chars_max = None) -> str:

        prompt = SummarizeDocumentPrompt().get_prompt_template()
        self.stuff_chain = load_summarize_chain(
            self.llm, 
            prompt = prompt,
            chain_type = "stuff"
        )

        if not chars_max:
            chars_max = self.llm_max_tokens * 2

        docs = self.get_documents(chunk_size = chars_max, max_chunks = 1)
        # Only use first doc for stuff chain - we're assuming we've rechunked this
        #  to be about the max token length for the model.
        docs = [docs[0]]
        text = docs[0].page_content.strip()

        if len(text) == 0:
            log_info(f"summarize_stuff: Document length is zero for '{self.file}' - nothing to do")
            # TODO: replace with Status:Failed summary document
            return SummaryNoContent

        if len(text) > chars_max:
            log_warning(f"summarize_stuff: Document length exceeds max: {len(text)} for '{self.file}'")
            text = text[:chars_max]

        log_info(f"Running stuff summary for '{self.file}': input len: {len(text)}, tokens: {get_num_tokens(text)}")

        summary:str = self.stuff_chain.run(docs)

        if not summary:
            log_warning(f"summarize_stuff: Summary length is zero for '{self.file}'")
            # TODO: replace with Status:Failed summary document
            return SummaryGeneratedEmpty

        log_info(f"Created stuff summary for '{self.file}': summary len: {len(summary)}, tokens: {get_num_tokens(summary)}")

        return summary

    # Note: MapReduce is not done in parallel and takes a long time to run.
    # https://github.com/langchain-ai/langchain/discussions/17045 
    # There's probably litttle reason to use this in practice for summarization
    #   as most of the pertinent information is likely to be in the first few pages.
    # MapReduce also creates shorter and more awakard summaries.

    @timed()
    def summarize_map_reduce(self, tokens_max:int = 30000):

        log_info("Attempting summarize with mapreduce chain")

        # Map phase
        # Take a group of doc chunks less than the token limit and summarize them
        # TODO: "{docs}" seems like the wrong var
        map_template = """Summarize the paragraphs below.
                        Be sure to include any information about the following topics:
                        Manufacturer, Model, Serial Number, Part Number, and any other relevant details about the asset.
                        Documents:
                        ======================
                        {docs}
                        Summary:"""  

        map_chain = LLMChain(
            llm = self.llm, 
            prompt = PromptTemplate.from_template(map_template),
            #verbose = True, Debug = True
        )

        # Reduce phase 
        # Take previously summarized documents and reduce them into a single summary
        reduce_template = """The following is set of summaries:
                {docs}
                ==============
                Take the summaries above and distill them into a final, consolidated summary.
                Be sure to include any information about the following topics:
                Manufacturer, Model, Serial Number, Part Number, and any other relevant details about the asset.
                Helpful Answer:
        """
        reduce_prompt = PromptTemplate.from_template(reduce_template)
        reduce_chain = LLMChain(
            llm = self.llm, 
            prompt = reduce_prompt,
            #verbose = True, Debug = True
        )
        # Takes a list of documents, combines them into a single string, and passes this to an LLMChain
        combine_documents_chain = StuffDocumentsChain(
            llm_chain = reduce_chain, 
            document_variable_name = "docs",
            #verbose = True, Debug = True
        )
        # Combines and iteratively reduces the mapped documents
        reduce_documents_chain = ReduceDocumentsChain(
            # This is final chain that is called.
            combine_documents_chain = combine_documents_chain,
            # If documents exceed context for `StuffDocumentsChain`
            collapse_documents_chain = combine_documents_chain,
            # The maximum number of tokens to group documents into.
            token_max = tokens_max,
            #verbose = True, Debug = True
        )

        map_reduce_chain = MapReduceDocumentsChain(
            # Map chain
            llm_chain = map_chain,
            # Reduce chain
            reduce_documents_chain = reduce_documents_chain,
            # The variable name in the llm_chain to put the documents in
            document_variable_name = "docs",
            # Return the results of the map steps in the output
            return_intermediate_steps = True,
            #verbose = True, Debug = True
        )

        docs = self.get_documents()
        summary = map_reduce_chain(docs)

        #return summary # when return_intermediate_steps = False
        summary_text = summary['output_text']
        log_info(f"Created mapreduce summary: len: {len(summary_text)}, tokens: {get_num_tokens(summary_text)}")

        return summary_text

    # TODO: Refine chain - will also be slow (and must be serial and unbatched), but should give better results than mapreduce



if __name__ == "__main__":

    from shared.indexing.Indexer import PyPdfIndexProcessor

    file = "3086855_MPCETNM6S_SC_IO_EN Hussman ref case installation and operations manual.pdf"

    ip = PyPdfIndexProcessor(remove_extra_whitespace = True)
    chunks, all_text = ip.get_chunks_from_pdf_blob(file, 4000, 40)

    all_text = all_text * 3
    summarizer = DocumentSummarizer(text = all_text)
    #summarizer = DocumentSummarizer(chunks = chunks)

    #all_text = all_text[:5000]

    summary_s = summarizer.summarize_stuff()
    summary_mr = summarizer.summarize_map_reduce()
    pass