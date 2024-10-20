# pylint: disable=broad-exception-caught.

import os
import sys
import multiprocessing
from threading import Thread
import datetime

# Get access to shared libs one level up (See dockfile)
if __name__ == '__main__': 
    sys.path.append(os.path.dirname(os.path.abspath(__file__ + '/../')))

from CopilotPythonHost.libs.models import ChatResponse
from CopilotPythonHost.libs.models.GetIndexDocumentInfo import GetIndexDocumentInfoRequest, GetIndexDocumentInfoResponse
from shared.indexing.Indexer import PyPdfIndexProcessor
from shared.indexing.IndexOps import AzureAiIndexOps

# Add access to shared packages from Copilot project root
sys.path.append(os.path.dirname(os.path.abspath(__file__ + '/../')))

from typing import Dict, Any, Tuple, List

from flask import Flask, jsonify, make_response, request
from flask_restful import Resource, Api
from flasgger import Swagger, swag_from
from langsmith import Client

from libs.openai.RAG import RAG
from shared.ServicesWrapper import ServicesWrapper
from shared.HealthCheck import CopilotHealthChecks
from shared.Utils import fix_request_date_timestamp, timed, default, DebugMode, fix_request_date
from libs.models.ChatRequest import ChatRequest
from libs.models.ChatResponse import ChatResponse
from libs.models.IndexDocumentRequest import IndexDocumentRequest
from libs.models.IndexDocumentResponse import IndexDocumentResponse
from libs.models.IndexRebuldRequest import IndexRebuildRequest
from libs.models.DeleteDocumentRequest import DeleteDocumentRequest
from libs.models.FindIndexDocuments import FindIndexDocumentsRequest, FindIndexDocumentsResponse, FindIndexDocumentsDocument
from CopilotPythonHost.libs.models.IndexRebuildResponse import IndexRebuildResponse
from libs.models.DeleteDocumentResponse import DeleteDocumentResponse

from shared.OpenTelemetry import (
    log_critical, log_info, log_exception, log_debug, log_warning, 
    MetricInitalized, MetricStartup, MetricFindIndexDocuments
)

from shared.indexing.SearchIndexConfig import (
    FieldName_DocUnstructuredMetadata,
    FieldName_TotalDocumentLength,
    FieldName_Uri,
    FieldName_Id, FieldName_IndexUpdateTime,
    FieldName_ItemType, FieldName_Content,
)

LangsmithEnabled = True
LangsmithClient = None
DefaultHost = '0.0.0.0'
DefaultPort = 8080

os.environ["COPILOT_SERVER_HOST"] = "0.0.0.0"
os.environ["OPENAI_API_VERSION"] = "2023-05-15"
image_version = os.environ.get("Image") or "local"
deployment_time = os.environ.get("DeploymentTime") or "n/a"
# os.environ["VECTOR_INDEX_NAME"] = 

# TODO: Encorporate Gunicorn or other as production web server
app = Flask(__name__)

if DebugMode:
    app.json.sort_keys = False
    app.config['SWAGGER'] = {
        'swagger_version': '2.0',
        'title': 'Copilot API'
    }
    Swagger(app=app)


# TODO: Move from env vars to passing in index config

if __name__ == '__main__':

    # These need to be set *before* the langchain module is imported to take effect -
    #   the others above can be set anytime before the dependencies are used.
    # (Shouldn't be a problem now that we're not setting os.environ at runtime)
    # https://github.com/langchain-ai/langchain/issues/7813
    # TODO: pass the schema into the c'tor instead of using env vars
    # "AZURESEARCH_FIELDS_ID", "AZURESEARCH_FIELDS_CONTENT",
    # "AZURESEARCH_FIELDS_CONTENT_VECTOR"

    def print_env():
        for key in os.environ.keys():
            log_debug(f"{key} = '{os.environ.get(key)}'")


# TODO: Comprare flask-resful (used here) w/plain flask and flask-restx (which also does swagger but differently)

class ChatResponseApi(Resource):
    # TODO: figure out how to create swagger from marshmallow schema
    #   generated from dataclass to avoid manually writing this yaml

    @swag_from('swagger/ChatRequest.yml')
    def post(self):
        """
        Endpoint to process chat requests
        """
        try:
            # json_data = request.get_json() # force=True) # force application/json
            ChatRequest.Schema().validate(request.get_json()) # pylint: disable=no-member.
            schema_violations: Dict = ChatRequest.Schema().validate(request.get_json()) # pylint: disable=no-member.
            if len(schema_violations.keys()) > 0:
                return f"Invalid request: {schema_violations}", 500

            # log_debug(f"ChatRequest: {request.get_data(as_text=True)}")

            chat_request = ChatRequest.from_json( request.get_data() ) # pylint: disable=no-member.
            # TODO: Can combine sessionID w/headers[REMOTE_ADDR] or userinfo
            log_info(f"ChatRequest: {chat_request}")

            if not chat_request.user_input:
                # Respond to "" with ""/204 for quick chat connectivity check 
                return make_response(
                    ChatResponse( responseText="", response_format="text"), 
                    204)

            EnableLangSmith(chat_request.get_options().is_langsmith_enabled())

            rag = RAG(chat_request)
            response = rag.get_chat_response()

        except Exception as ex:
            log_exception(ex)
            return f"Error processing chat request: {ex}", 500

        return jsonify(response)

class HealthzApi(Resource):

    @swag_from('swagger/HealthZ.yml')
    def get(self):
        """
        Return health status of Copilot and its dependencies
        """
        status = CopilotHealthChecks.get_root()
        return status.get_json()

# @app.route('/hellopage') def hello(): return "Hello from copilot"

def run_in_background_if_needed(func, id, run_in_background: bool) -> tuple[str, str|None, Any]:
    if run_in_background:
        thread = Thread(target = func, name = id, args=[])
        # At this point assume that the appsettings are correct
        thread.start()
        log_info(f"Started background thread {thread.native_id} for {id}")
        # TODO: use twinsapi SQL-based jobs api when avail and return job id
        job_id = f"{id}-{thread.native_id}"
        return "Started", job_id, None
    else:
        log_debug(f"Running synchronously: {id}")
        return "OK", None, func()

class IndexDocumentApi(Resource):

    @swag_from('swagger/IndexDocument.yml')
    def post(self):
        """
        Index a single document from using the provided blob file name
          which is assumed to be in the documents container for the customer
        """
        try:
            schema_violations: Dict = IndexDocumentRequest.Schema().validate(request.get_json()) # pylint: disable=no-member.
            if len(schema_violations.keys()) > 0:
                return f"Invalid request: {schema_violations}", 500

            index_doc_request: IndexDocumentRequest = IndexDocumentRequest.from_json( request.get_data() )
            log_info(f"IndexRequest: {index_doc_request}")

            index_name = index_doc_request.index_name or ServicesWrapper.get_default_create_index_name()
            # Create index lazily on index request if not already created - fairly quick operation
            # index_ops = AzureAiIndexOps(index_name)
            # index_ops.create_or_update_index()

            indexer = PyPdfIndexProcessor(index_name_override = index_name)

            status, job_id = None, None
            def exec():
                # Telemetry metric emitted in add_pdf_file_to_index
                n_chunks = indexer.add_pdf_file_to_index(
                    file = index_doc_request.blob_file,
                    generate_summaries_mode = index_doc_request.generate_summaries_mode,
                    generate_index_mode = index_doc_request.generate_index_mode,
                    chunk_size = index_doc_request.chunk_size,
                    chunk_overlap = index_doc_request.chunk_overlap,
                    include_chunkdocs_for_copilot_disabled_files = index_doc_request.include_chunkdocs_for_copilot_disabled_files,
                    indexing_strategy = index_doc_request.indexing_strategy
                )
                return n_chunks

            status, job_id, _n_chunks = run_in_background_if_needed(exec, 
                                            f"IndexDocument-{index_doc_request.blob_file}",
                                            # Make sure to set to True if omitted/None
                                            default(index_doc_request.run_in_background, True))

            response = IndexDocumentResponse(
                status = status,
                job_id = job_id
            )
            return make_response(jsonify(response), 202 if job_id else 200)

        except Exception as ex:
            log_exception(ex)
            return f"Error indexing document '{index_doc_request.blob_file}' : {ex}", 500

class DeleteDocumentApi(Resource):

    @swag_from('swagger/DeleteDocument.yml')
    def post(self):
        """
        Remove all the index document chunks for the given blob file.
        """
        try:
            schema_violations: Dict = DeleteDocumentRequest.Schema().validate(request.get_json()) # pylint: disable=no-member.
            if len(schema_violations.keys()) > 0:
                return f"Invalid request: {schema_violations}", 500
            delete_docs_request = DeleteDocumentRequest.from_json( request.get_data() )
            log_info(f"DeleteDocsRequest: {delete_docs_request}")

            # For now create index lazily on first index request - can move to startup later
            index_name = delete_docs_request.index_name or ServicesWrapper.get_default_create_index_name()
            index_ops = AzureAiIndexOps(index_name)
            # We're not passing a document last update time here, so deletion will always take place
            # Telemetry metric emitted in delete_docs_for_blob_uri_if_needed
            n_chunks_deleted = index_ops.delete_docs_for_blob_uri_if_needed(delete_docs_request.blob_file)
            # Idempotent - if no chunks found, return 200

            response = DeleteDocumentResponse(
                # Idepotent - if no chunks found, return 200 - or 404?
                status = "OK" if n_chunks_deleted > 0 else "NotFound",
                num_index_docs_deleted = n_chunks_deleted
            )
            return jsonify(response)

        except Exception as ex:
            log_exception(ex)
            return f"Error deleting index documents for '{delete_docs_request}' : {ex}", 500

class GetIndexDocumentInfoApi(Resource):

    @swag_from('swagger/GetIndexDocumentInfo.yml')
    def post(self):
        """
        Return information about a document in the index
        """
        try:
            schema_violations: Dict = GetIndexDocumentInfoRequest.Schema().validate(request.get_json()) # pylint: disable=no-member.
            if len(schema_violations.keys()) > 0:
                return f"Invalid request: {schema_violations}", 500
            getinfo_request: GetIndexDocumentInfoRequest = GetIndexDocumentInfoRequest.from_json( request.get_data() )
            log_info(f"GetInfoRequest: {getinfo_request}")

            index_name = getinfo_request.index_name or ServicesWrapper.get_default_read_index_name()
            index_ops = AzureAiIndexOps(index_name)
            response = []
            for blob_file in getinfo_request.blob_files:
                # Telemetry metric emitted in get_document_info
                doc_info = index_ops.get_document_info(blob_file)
                if not doc_info:
                    msg = f"GetIndexDocumentInfo: '{blob_file}' not found in index" 
                    log_warning(msg)
                response.append(doc_info)

            return jsonify(response)

        except Exception as ex:
            log_exception(ex)
            return f"Error getting document info '{getinfo_request.blob_file}' : {ex}", 500

class FindIndexDocumentsApi(Resource):
    
    @swag_from('swagger/FindIndexDocuments.yml')
    def post(self):
        """
        Find documents in the index given the optional type of index document 
          and optional indexed time.  (not content-based search)
        """
        try:
            schema_violations: Dict = FindIndexDocumentsRequest.Schema().validate(request.get_json()) # pylint: disable=no-member.
            if len(schema_violations.keys()) > 0:
                return f"Invalid request: {schema_violations}", 500
            request_dict = request.get_json()
            fix_request_date_timestamp(request_dict, "last_updated_time")
            find_request: FindIndexDocumentsRequest = FindIndexDocumentsRequest.from_dict(request_dict)   
            log_info(f"FindIndexDocumentsRequest: {find_request}")

            index_name = find_request.index_name or ServicesWrapper.get_default_read_index_name()
            index_ops = AzureAiIndexOps(index_name)
            # Hack: Polymorphic return: return_total_count:true will return (count,docs) - all other callers don't need this and will return docs only
            total_count, docs = index_ops.find_index_docs(
                uri = None,
                copilot_enabled_only = False,
                update_time = find_request.last_updated_time,
                doc_type = find_request.document_type,
                page_size = find_request.page_size,
                page_number = find_request.page_number,
                return_total_count = True
            )
            # TODO: Move/add metric to find_index_docs which would track internally usages as well?
            MetricFindIndexDocuments(True, nTotal=total_count, nPage=len(docs), 
                                     includeMeta=find_request.include_metadata, 
                                     includeContent=find_request.include_content) 

            response = FindIndexDocumentsResponse(
                total_count = total_count,
                index_documents = [
                    FindIndexDocumentsDocument(
                        id = doc[FieldName_Id],
                        uri = doc[FieldName_Uri],
                        doc_type = doc[FieldName_ItemType],
                        document_size = int(doc[FieldName_TotalDocumentLength]),
                        content = doc[FieldName_Content] if find_request.include_content else None,
                        indexed_time = doc[FieldName_IndexUpdateTime],
                        metadata_json = doc[FieldName_DocUnstructuredMetadata] if find_request.include_metadata else None
                    ) for doc in docs
                ]
            )
            return jsonify(response)

        except Exception as ex:
            MetricFindIndexDocuments(False)
            log_exception(ex)
            return f"Error finding documents in index: {ex}", 500


class IndexRebuildApi(Resource):

    @swag_from('swagger/IndexRebuild.yml')
    def post(self):
        """
        Endpoint to rebuild the index.
        Currently has separate flags for deleting and recreating the index,
           and reindexing all documents. If the latter is omitted, then documents
           won't get indexed until TLM calls the index document endpoint.
        """
        try:
            schema_violations: Dict = IndexRebuildRequest.Schema().validate(request.get_json()) # pylint: disable=no-member.
            if len(schema_violations.keys()) > 0:
                return f"Invalid request: {schema_violations}", 500
            index_rebuild_request:IndexRebuildRequest = IndexRebuildRequest.from_json( request.get_data() )
            log_info(f"IndexRebuildRequest: {index_rebuild_request}")


            index_name = index_rebuild_request.index_name or ServicesWrapper.get_default_create_index_name() 

            indexer = PyPdfIndexProcessor(index_name_override = index_name)

            if indexer.is_rebuild_in_progress():
                response = IndexRebuildResponse(
                    status = "AlreadyInProgress",
                    job_id = "" 
                )
                return make_response(jsonify(response), 202)

            def exec():
                # Telemetry metric emitted in rebuild_index
                indexer.rebuild_index(
                    delete_and_recreate_index = index_rebuild_request.delete_and_recreate_index, 
                    generate_summaries_mode = index_rebuild_request.generate_summaries_mode,
                    generate_index_mode = index_rebuild_request.generate_index_mode,
                    num_threads = index_rebuild_request.num_threads,
                    chunk_size = index_rebuild_request.chunk_size,
                    chunk_overlap = index_rebuild_request.chunk_overlap,
                    include_chunkdocs_for_copilot_disabled_files = index_rebuild_request.include_chunkdocs_for_copilot_disabled_files,
                    indexing_strategy = index_rebuild_request.indexing_strategy
                )

            status, job_id, _n_docs_processed = run_in_background_if_needed(exec, 
                                            "RebuildIndex",
                                            default(index_rebuild_request.run_in_background, True))

            response = IndexRebuildResponse(
                status = status,
                job_id = job_id
            )
            return make_response(jsonify(response), 202 if job_id else 200)

        except Exception as ex:
            log_exception(ex)
            return f"Error processing index rebuild: {ex}", 500

# TODO: Manage with new twinsapi jobs api
def create_index_and_start_background_indexing(should_index:bool = True):
    
    indexer = PyPdfIndexProcessor(index_name_override = ServicesWrapper.get_default_create_index_name())

    def exec():
        indexer.rebuild_index(
            delete_and_recreate_index = False, 
            generate_summaries_mode = "ifNewer" if should_index else "off",
            generate_index_mode = "ifNewer" if should_index else "off",
            include_chunkdocs_for_copilot_disabled_files = False
        )

    run_in_background_if_needed( 
        exec, 
        "RebuildIndexOnStartup", 
        run_in_background = True
    )


def EnableLangSmith(enable: bool) -> bool:

    global LangsmithClient
    if not LangsmithEnabled:
        log_info("Langsmith not enabled")
        return False
    
    langchain_api_key = os.environ.get("LANGCHAIN_API_KEY")
    if not langchain_api_key:
        if enable: log_info("Langsmith no key set")
        return False
        
    # Langsmith langchain integration automatically logs everything if the env vars are set
    #  so this seems to be is the only means of control -- not good for mutithread/request
    #  processing, but good enoough for on-demand debugging. 
    #  Worst case is that we also trace a request that we didn't want to trace.
    # Note this won't be enabled in production anyway (no key set) unless we
    #  decide to pay for an enterprise account.
    if enable:
        os.environ["LANGCHAIN_TRACING_V2"] = "true"
        os.environ["LANGCHAIN_PROJECT"] = "willow copilot 0.1"
        os.environ["LANGCHAIN_ENDPOINT"] = "https://api.smith.langchain.com"
        LangsmithClient = Client()
        log_info(f"Langsmith enabled (key ending {langchain_api_key[-5:-1]})")
        return True
    else:
        log_info("Langsmith disabled")
        if LangsmithClient:
            LangsmithClient = None
        os.environ["LANGCHAIN_TRACING_V2"] = "false"
        return False

@app.errorhandler(404)
def page_not_found(e):
    return jsonify({"error": "Not found"}), 404

if __name__ == '__main__':

    try:
        MetricStartup()

        log_info(f"Copilot main app init: Version {image_version} deployed at {deployment_time}")
        log_info(f"cpu cores: {multiprocessing.cpu_count()} ")
        print_env()

        api = Api(app)
        # Make the WSGI interface available at the top level so wfastcgi can get it.
        wsgi_app = app.wsgi_app

        api.add_resource(HealthzApi,       '/healthz')
        api.add_resource(ChatResponseApi,  '/chat')
        api.add_resource(IndexRebuildApi,  '/index/rebuild')
        api.add_resource(IndexDocumentApi, '/index/add-doc')
        api.add_resource(GetIndexDocumentInfoApi, '/index/doc-info')
        api.add_resource(FindIndexDocumentsApi, '/index/find-index-docs')
        api.add_resource(DeleteDocumentApi, '/index/delete-doc')

    except Exception as ex:
        log_critical(f"Copilot startup failure: Error in route setup: {ex}", exc_info=True)

    try:
        # Create index if needed and start re-indexing on startup 
        index_on_startup = os.environ.get("INDEX_ON_STARTUP", "false").lower() == "true"
        log_info(f"Index on startup: {index_on_startup}")
        if not DebugMode and index_on_startup:
            create_index_and_start_background_indexing(should_index = not DebugMode)
        else:
            # If we're not reindex, still call create_or_update_index to make any incremental schema changes
            index_ops = AzureAiIndexOps()
            index_ops.create_or_update_index()

        if DebugMode:
            # See: https://stackoverflow.com/questions/1435415/python-memory-leaks
            #   for many options for debugging memory issues
            #gc.set_debug(gc.DEBUG_LEAK)
            pass

        MetricInitalized()

        # Must set Debug=False for debugger to work...
        # https://stackoverflow.com/questions/22711087/flask-importerror-no-module-named-app
        # app.run(HOST, PORT, debug=DebugMode)
        app.run(
            DefaultHost, DefaultPort, 
            debug=False, 
            threaded = True, 
            # Can also use process pool with processes=n - can't be used w/threaded
            #  however if we need more scalability, should use gunicorn or similar
            #  as per: https://flask.palletsprojects.com/en/2.3.x/deploying/
        )
        log_warning("Exiting web server app.run")
        # Should never get here

    except Exception as ex:
        MetricInitalized(False)
        log_critical(f"Copilot startup failure: Error in app run: {ex}", exc_info=True)
