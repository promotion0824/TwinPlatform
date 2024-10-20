
import os
import sys
from toolz.itertoolz import groupby

sys.path.append( os.getcwd())
from shared.ServicesWrapper import ServicesWrapper

def print_doc(doc):
    try: chunk_index = int(doc.metadata['ChunkId'].split('_')[3])
    except: chunk_index = -1
    print(f"Title: {doc.metadata['Title']}, Chunk: {chunk_index}, Score: {round( doc.metadata['@search.score'], 4)}")

def all_doc_titles(docs):
    set([doc.metadata['Title'] for doc in docs])

def grouped_docs(docs):
    #return {k: list(g) for k, g in groupby(docs, lambda doc: doc.metadata['Title'])}
    return groupby(lambda doc: doc.metadata['Title'], docs)

text = "how repair the ke2?"
K = 10
SearchType = "hybrid"
Filter = "Title eq 'KE2-EvapOEM.pdf'"

#Source docs: ['KE2-EvapOEM.pdf', 'MK_0000323_BR_Krack_DOE_Evaporators_EN.pdf', 'troubleshooting-guide.pdf', '3086855_MPCETNM6S_SC_IO_EN Hussman ref case installation and operations manual.pdf']

def doc_info(docs):
    print("====================")
    print(len(docs))
    for doc in docs:
        print_doc(doc)  
    g = grouped_docs(docs)

def test_vector_stores():
    s = ServicesWrapper()
    vs = s.get_vector_store()

    docs1 = vs.similarity_search(
        query = text, 
        k = K, 
        search_type = "similarity",
        #filters = Filter
    )
    doc_info(docs1)

    docs2 = vs.similarity_search(
        query = text, 
        k = K, 
        search_type = "hybrid",
        #filters = Filter
    )
    doc_info(docs2)

def test_as_retriever():
    s = ServicesWrapper()
    vs = s.get_vector_store()
    ret = vs.as_retriever(
        search_type = "similarity",
        search_kwargs = {
            "k": 8,
            "search_type": "hybrid",
            "filters": "Title eq 'KE2-EvapOEM.pdf'"
        }
    )
    docs = ret.get_relevant_documents("air")
    doc_info(docs)
    
             
if __name__ == '__main__':

    #test_vector_stores()
    test_as_retriever()

