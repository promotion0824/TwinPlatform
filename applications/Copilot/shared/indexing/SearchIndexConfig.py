
import os
from azure.search.documents.indexes.models import (
    SearchableField, SearchField,
    SearchFieldDataType, SimpleField,

    VectorSearch, VectorSearchProfile, HnswAlgorithmConfiguration,
    AzureOpenAIVectorizer, 
    AzureOpenAIParameters
)
from pydantic import Field

ItemType_DocumentChunk = "docChunk" 
ItemType_DocumentWhole = "docWhole" 
ItemType_DocumentSummary = "docSummary" 

FieldName_Id = "Id"
FieldName_Content = "Content"
FieldName_ItemType = "ItemType"
FieldName_GroupId = "GroupId"
FieldName_CopilotEnabled = "CopilotEnabled"
FieldName_Title = "Title"
FieldName_Uri = "Uri"
FieldName_IndexUpdateTime = "IndexUpdateTime"
FieldName_DocLastUpdateTime = "DocLastUpdateTime"
FieldName_PageNumber = "PageNumber"
FieldName_ParsePath = "ParsePath"
FieldName_Vector = "ContentVector"
FieldName_RequestChunkSize = "RequestedChunkSize"
FieldName_TotalDocumentNumChunks = "TotalDocumentNumChunks"
FieldName_TotalDocumentLength = "TotalDocumentLength"
FieldName_DocUnstructuredMetadata = "DocUnstructuredMetadata"
FieldName_ContentLength = "ContentLength"
FieldName_ContentTokenCount = "ContentTokenCount"
FieldName_IndexerSource = "IndexerSource"
FieldName_ProcessingParameters = "ProcessingParameters"
FieldName_FilterTags = "FilterTags"

IndexFields = [

    SearchableField(
        name = FieldName_Content,
        type = SearchFieldDataType.String,
    ),
    SimpleField(
        name = FieldName_ItemType,
        type = SearchFieldDataType.String,
        filterable = True,
    ),
    SimpleField(
        name = FieldName_Id,
        type = SearchFieldDataType.String,
        key = True,
        searchable = False,
        filterable = True,
    ),
    SimpleField(
        name = FieldName_GroupId,
        type = SearchFieldDataType.String,
        searchable = False,
        filterable = True,
    ),
    SimpleField(
        name = FieldName_CopilotEnabled,
        type = SearchFieldDataType.Boolean,
        searchable = False,
        filterable = True,
    ),
    SearchableField(
        name = FieldName_Title,
        type = SearchFieldDataType.String,
        filterable = True
    ),
    SimpleField(
        name = FieldName_Uri,
        type = SearchFieldDataType.String,
        searchable = False,
        filterable = True,
    ),
    SimpleField(
        name = FieldName_IndexUpdateTime,
        type = SearchFieldDataType.DateTimeOffset,
        searchable = False,
        filterable = True,
    ),
    SimpleField(
        name = FieldName_DocLastUpdateTime,
        type = SearchFieldDataType.DateTimeOffset,
        searchable = False,
        filterable = True,
    ),
    SimpleField(
        name = FieldName_PageNumber,
        type = SearchFieldDataType.Int32,
        searchable = False,
    ),
    SimpleField(
        name = FieldName_ParsePath,
        type = SearchFieldDataType.String,
        searchable = False, # maybe true if sections/header/footer etc
    ),
    SearchField(
        name = FieldName_Vector,
        type = SearchFieldDataType.Collection(SearchFieldDataType.Single),
        vector_search_dimensions = 1536,
        vector_search_profile_name = "Document-Vector-Profile",
        # This is what we want to set: retrievable=True (which apparently is a no-op)
        hidden = False
    ),
    SimpleField(
        name = FieldName_RequestChunkSize,
        type = SearchFieldDataType.Int32,
        searchable = False,
    ),
    SimpleField(
        name = FieldName_TotalDocumentNumChunks,
        type = SearchFieldDataType.Int32,
        searchable = False,
    ),
    SimpleField(
        name = FieldName_TotalDocumentLength,
        type = SearchFieldDataType.Int64,
        searchable = False,
    ),
    SimpleField(
        name = FieldName_ContentLength,
        type = SearchFieldDataType.Int64,
        searchable = False,
    ),
    SimpleField(
        name = FieldName_ContentTokenCount,
        type = SearchFieldDataType.Int32,
        searchable = False,
    ),
    SimpleField(
        name = FieldName_IndexerSource,
        type = SearchFieldDataType.String,
        searchable = False,
        filterable = True,
    ),
    SearchableField(
        name = FieldName_DocUnstructuredMetadata,
        type = SearchFieldDataType.String,
        filterable = True,
        # there's no searchAnalyzer/indexAnalyzer for JSON to get only values
        # https://learn.microsoft.com/en-us/azure/search/index-add-custom-analyzers
    ),
    SimpleField(
        name = FieldName_ProcessingParameters,
        type = SearchFieldDataType.String,
        searchable = False,
    ),
    SimpleField(
        name = FieldName_FilterTags,
        type = SearchFieldDataType.Collection(SearchFieldDataType.String),
        searchable = False,
        filterable = True,
    ),
]


# https://learn.microsoft.com/en-us/azure/search/vector-search-how-to-configure-vectorizer
# Note that the snake_cases get converted to camelCases by the Azure Python SDK

def get_vector_search_config() -> VectorSearch:
    
    vs = VectorSearch(

        algorithms = [
            HnswAlgorithmConfiguration(
                kind = "azureOpenAI",
                name = "documents-vec-algorithm"
            )
        ],
        profiles=[
            VectorSearchProfile(
                name = "Document-Vector-Profile",
                vectorizer = "documents-vectorizer", 
                algorithm_configuration_name = "documents-vec-algorithm"
            )
        ],
        vectorizers = [
            AzureOpenAIVectorizer(
                kind = "azureOpenAI",
                name = "documents-vectorizer",
                azure_open_ai_parameters = AzureOpenAIParameters(
                    model_name = "text-embedding-ada-002",
                    resource_uri = os.environ["AZURE_OPENAI_ENDPOINT"],
                    deployment_id = os.environ["VECTOR_EMBEDDINGS_DEPLOYMENT_NAME"],
                    api_key = os.environ["AZURE_OPENAI_API_KEY"]
                )
            )
        ]
    )
    return vs

# semantic_config = SemanticConfiguration(
#     name="my-semantic-config",
#     prioritized_fields=SemanticPrioritizedFields(
#         title_field=SemanticField(field_name="title"),
#         keywords_fields=[SemanticField(field_name="category")],
#         content_fields=[SemanticField(field_name="content")]))
# semantic_search = SemanticSearch(configurations=[semantic_config])

if __name__ == '__main__':
    vsc = get_vector_search_config()
    pass