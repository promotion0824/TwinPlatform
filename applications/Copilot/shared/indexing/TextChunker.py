
from calendar import c
import re
from dataclasses_json import dataclass_json
from marshmallow_dataclass import dataclass

from shared.OpenTelemetry import log_error, log_exception, log_info, log_debug
from shared.indexing.BlobOps import BlobOps

"""
    from langchain.text_splitter import RecursiveCharacterTextSplitter
    def simple_splitter(self, text) -> List[str]:
        splitter = RecursiveCharacterTextSplitter(
            chunk_size = 2000, chunk_overlap = 200
        )
        chunks = splitter.split_text(text)
        return chunks
"""

@dataclass_json()
@dataclass
class TextChunk:
    Content: str
    Page: int

# TODO:
# collect multiple page refs if chunk spans multiple pages
# Line# for txt files?
# use recursive character splitter internally?
# Can use nltk to remove stop words

class TextChunker:

    def __init__(self, chunk_size: int, overlap: int, remove_extra_whitespace = True) -> None:
        self.spans = []
        self.chunk_size = chunk_size
        self.overlap = overlap
        self.remove_extra_whitespace = remove_extra_whitespace

    def add_text(self, page_num, text) -> None:
        if not text: return

        if self.remove_extra_whitespace:
            # Remove extra spaces, but keep \n's and \t's
            # PDF chunks are about 50% whitespace - this gives better utilization of 
            #  chunks, but it's possible that this could change the interpretation of text by the LLM
            trimmed_text = re.sub("[ ][ ]+" , " ", text)
            if len(text) != len(trimmed_text):
                diff = len(text) - len(trimmed_text)
                percent = int(diff * 100 / len(text))
                log_debug(f"TextChunker trimmed {diff} characters ({percent}%) from page {page_num}")
        else:
            trimmed_text = text

        if len(trimmed_text) < 1:
            return

        self.spans.append((page_num, trimmed_text))


    def __iter__(self):
        return self

    def __next__(self) -> TextChunk:
        chunk = self.get_next_chunk()
        if chunk is None:
            raise StopIteration
        return chunk
        
    def get_next_chunk(self, chunk_size: int = None, chunk_overlap:int = None) -> None | TextChunk:

        if len(self.spans) == 0: 
            return None

        chunk_text = ''
        page_num = None
        chunk_size = chunk_size or self.chunk_size
        chunk_overlap = chunk_overlap or self.overlap

        if not chunk_size or chunk_size < 1:
            raise ValueError("chunk_size must be a positive integer")
        if not chunk_overlap or chunk_overlap < 1 or chunk_overlap >= chunk_size:
            raise ValueError("chunk_overlap must be a positive integer less than chunk_size")

        while len(chunk_text) < chunk_size and len(self.spans) > 0:

            (page, text) = self.spans.pop(0)

            try:
                if page_num is None: 
                    # We could return a page range that make up a chunk
                    page_num = page
                n_remaining = chunk_size - len(chunk_text)
                if len(text) <= n_remaining:
                    chunk_text += text
                else:
                    chunk_text += text[:n_remaining]

                remainder_text = text[n_remaining - chunk_overlap:]
                if len(remainder_text) > 0:
                    new_span = (page, remainder_text)
                    self.spans.insert(0, new_span)
                    return TextChunk(chunk_text, page_num)

            except Exception as e:
                log_exception(f"Error in chunk processing: {e}")
                return None

        log_debug(f"Returning chunk of {len(chunk_text)} characters from page {page_num}")
        return TextChunk(chunk_text, page_num)
        

class TestTextChunker:

    def __init__(self) -> None:
        self.size = None
        self.overlap = None
        self.clear(0,0)

    def clear(self, size:int = None, overlap:int = None):
        self.ch = TextChunker(self.size, self.overlap)

    def add(self, page, text):
        self.ch.add_text(page, text)

    def check(self, size, overlap, expected_text, expected_page):
        chunk = self.ch.get_next_chunk(size, overlap)
        if chunk is None and expected_text is not None:
            print(f"Expected: {expected_page}, '{expected_text}'")
            print(f"Actual: None\n")
            return False
        elif chunk is None and expected_text is None:
            return False

        (p, t) = (chunk.Page, chunk.Content)
        if (expected_page, expected_text) != (p, t):
            print(f"Expected: {expected_page}, '{expected_text}'")
            print(f"Actual: {p}, '{t}'\n")
            return False
        else: 
            print (f"=== {p}, '{t}'")
        return True

    def add_text(self):
        self.clear()
        self.add(1, "This       is a test")
        self.add(2, " of the            text chunker")
        self.add(3, " class")
        print("======================")

    def add_ntext(self):
        self.clear()
        self.add(1, "123")
        self.add(2, "4567890")
        self.add(3, "ABCD")
        print("======================")

    def test(self):

        self.add_ntext()
        self.check(4, 1, "1234", 1)
        self.check(4, 1, "4567", 2)
        self.check(4, 1, "7890", 2)
        self.check(4, 1, "0ABC", 3)
        self.check(4, 1, "CD", 3)

        self.add_text()
        self.check(1000, 1, "This is a test of the text chunker class", 1)

        self.add_text()
        self.check(6, 1, "This i", 1)
        self.check(6, 1, "is a t", 1)
        self.check(6, 1, "test o", 1)
        self.check(6, 1, "of the", 2)
        self.check(6, 1, "e text", 2)
        self.check(6, 1, "t chun", 2)
        self.check(6, 1, "nker c", 2)
        self.check(6, 1, "class", 3)


if __name__ == '__main__':  

    from pathlib import Path
    import os
    from shared.indexing.Indexer import PyPdfIndexProcessor

    if False:
        t = TestTextChunker()
        t.test()
        pass

    def extract_and_download_pdf_text(file: str, dir: str) -> str:
        try:
            _extract_and_download_pdf_text(file, dir)
        except Exception as e:
            print(f"\n************* Error: {e}\n")

    def _extract_and_download_pdf_text(file: str, dir: str):
        home = Path.home()
        dir = os.path.join(home, dir, file)
        os.makedirs(dir, exist_ok=True)

        pp = PyPdfIndexProcessor()
        cs, co = 4000, 200
        bops = BlobOps()
        bops.blob_name = file

        stream = bops.get_blob_stream()
        try:
            with open(os.path.join(dir, file), "wb") as f:
                file_bytes = stream.getvalue()
                f.write(file_bytes)
        except Exception as e:
            print(f"Error writing pdf: {e}")
        stream.seek(0)

        chunks, all_text = pp.get_chunks_from_pages_simple_text_overlap(stream, cs, co)

        encoding='utf-8'
        all_text_after_chunking = "".join([c.Content for c in chunks])
        try: 
            with open(os.path.join(dir, "all_text_pre_chunk"), "w", encoding = encoding) as f:
                f.write(all_text)
        except Exception as e:
            print(f"Error writing all_text_pre_chunk: {e}")

        all_text_after_chunking = "".join([c.Content for c in chunks])
        try:
            with open(os.path.join(dir, "all_text_post_chunk"), "w", encoding=encoding) as f:
                f.write(all_text_after_chunking)
        except Exception as e:
            print(f"Error writing all_text_post_chunk: {e}")

        def sep(i, chunk:TextChunk): 
            return f"\n\n========= Page:{chunk.Page} Chunk:{i} ==========\n{chunk.Content}\n\n"

        all_chunks = ''.join( sep(i, c) for (i, c) in enumerate(chunks))
        try:
            with open(os.path.join(dir, "all_chunks"), "w", encoding=encoding) as f:
                f.write(all_chunks)
        except Exception as e:
            print(f"Error writing all_chunks: {e}")
            
        return chunks, all_text

    if False:
        file = "ll97of2019.pdf"
        extract_and_download_pdf_text(file, "copilot-chunker-test-output")

    if True:
        bo = BlobOps()
        blobs = bo.get_blob_list()
        blobs = sorted(blobs, key = lambda b: b.size)
        for (i, blob) in enumerate(blobs):
            print(f"================== {i:3}: {int(blob.size/1024):>8}K : {blob.name}")
            extract_and_download_pdf_text(blob.name, "copilot-chunker-test-output")
            print(f"Completed {blob.name}")