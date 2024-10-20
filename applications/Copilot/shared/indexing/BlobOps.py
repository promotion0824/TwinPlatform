import io
import os
import urllib.parse

from azure.identity import DefaultAzureCredential
from azure.storage.blob import BlobServiceClient, BlobClient
from azure.storage.blob._container_client import ContainerClient
from azure.storage.blob._models import BlobProperties

from shared.OpenTelemetry import log_info, log_debug, log_error, log_exception, log_warning
from shared.Utils import timed, path_join, file_looks_like

# https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-download-python


class BlobOps:

    default_credential = DefaultAzureCredential()
    account_name = os.environ["BlobStorage__AccountName"]
    container_name = os.environ["BlobStorage__ContainerName"]
    account_url = f"https://{account_name}.blob.core.windows.net/"
    file_prefix = path_join(account_url, container_name)

    def __init__(self, file:str = None, container:str|None = None) -> None:
        self.blob_name = file
        self.blob_client = None
        self.blob_service_client = None
        self.container_client = None
        if container:
            self.container_name = container
        if self.blob_name:
            self.blob_client = self.get_blob_client()
        log_debug(f"BlobOps: blob_name:{self.blob_name} account_url: {self.account_url} container_name: {self.container_name}")

    @staticmethod
    def get_nameOrUri_filename(nameOrUri:str) -> str:
        if nameOrUri.strip().lower().startswith("http"):
            #return nameOrUri.split('/')[-1]
            # Better in case we ever use virtual directories
            return nameOrUri.replace(BlobOps.file_prefix, "").strip("/")
        else:
            return nameOrUri

    @classmethod
    def get_nameOrUri_uri(cls, nameOrUri:str) -> str:
        if nameOrUri.strip().lower().startswith("http"):
            return nameOrUri
        else:
            return path_join(cls.file_prefix, nameOrUri)

    @classmethod
    def get_nameOrUri_encodedUri(cls, nameOrUri:str) -> str:
        file = cls.get_nameOrUri_filename(nameOrUri)
        return f"{BlobOps.file_prefix}/{urllib.parse.quote(file)}"

    @classmethod
    def get_nameOrUri_unencodedUri(cls, nameOrUri:str) -> str:
        file = cls.get_nameOrUri_filename(nameOrUri)
        return f"{BlobOps.file_prefix}/{urllib.parse.unquote(file)}"

    def get_blob_uri_urlencoded(self):
        return self.get_blob_client().url

    def get_blob_uri_unencoded(self):
        return f"{BlobOps.file_prefix}/{self.get_blob_name()}"

    @timed()
    def get_blob_stream( self) -> io.BytesIO:
        blob_client = self.get_blob_client()
        stream = io.BytesIO()
        # TODO: Check stream close semantics
        num_bytes = blob_client.download_blob().readinto(stream)
        log_debug(f"Blob stream len: {num_bytes} bytes")
        return stream

    @timed()
    def get_blob_bytes( self) -> io.BytesIO:
        blob_client = self.get_blob_client()
        downloader = blob_client.download_blob( 
            # Return as str, otherwise bytes
            #encoding='UTF-8', 
        )
        # Internally this just calls .getvalue() on the stream w/optional decode call
        bytes = downloader.readall()
        log_debug(f"Blob data len: {len(bytes)} bytes")
        return bytes

    def get_service_client(self) -> BlobServiceClient:
        self.blob_service_client = self.blob_service_client or BlobServiceClient(
            self.account_url, 
            credential = self.default_credential
        )
        return self.blob_service_client

    def get_container_client(self) -> ContainerClient:
        self.container_client = self.container_client or \
                    self.get_service_client().get_container_client(self.container_name)
        return self.container_client

    def get_blob_client( self) -> BlobClient:
        if not self.blob_name:
            raise Exception("BlobOps get_blob_client: must init with blob file")
        self.blob_client = self.blob_client or self.get_service_client().get_blob_client(
            container = self.container_name, 
            blob = self.blob_name
        )
        return self.blob_client

    def get_blob_properties( self) -> BlobProperties:
        blob_client = self.get_blob_client()
        return blob_client.get_blob_properties()

    def get_blob_name(self):
        return self.get_blob_client().blob_name
        #return self.get_nameOrUri_filename( self.blob_name)
        #return self.blob_name

    def write_all(self, text:str):
        blob_client = self.get_blob_client()
        blob_client.upload_blob(text, overwrite = True)

    def read_all(self) -> str|None:
        blob_client = self.get_blob_client()
        # check if exists
        if not blob_client.exists():
            return None
        return blob_client.download_blob().readall().decode('utf-8')
        

    def get_blob_file_extension(self):
        # "splitExt" returns tuple (base, ext) or '' if no extension
        return os.path.splitext(self.get_blob_name())[1].lower()

    #@timed
    def get_blob_list(self) -> list[BlobProperties]:
        c_client = self.get_container_client()
        blob_generator = c_client.list_blobs()
        return [b for b in blob_generator]

    def file_looks_like(self, extension = "pdf") -> bool:
        file_type = self.get_blob_properties().content_settings['content_type']
        if file_type == f'application/{extension}':
            return True
        file = self.get_blob_name()
        return file_looks_like(file, extension)

if __name__ == '__main__':
    
    if False:
        uri = BlobOps.get_nameOrUri_uri("foo.pdf")
        file = BlobOps.get_nameOrUri_filename(uri)
        pass

    if False:
        bops = BlobOps("foo.txt", container="copilot")
        bops.write_all("Hello, world")
    if True:
        bops = BlobOps("foo.txt", container="copilot")
        text = bops.read_all()
        print(text)

    if False:
        #sys.path.append(os.path.dirname(os.path.abspath(__file__ + '/../../')))
        print("Retrieving blob list")
        bo = BlobOps()
        blobs = bo.get_blob_list()
        for (i, blob) in enumerate(blobs):
            print(f"{i:3}: {int(blob.size/1024):>8}K : {blob.name}")
            bof = BlobOps(blob.name)
            metadata = bof.get_blob_properties().metadata
            if False and len(metadata) > 0:
                print(f"\tMetadata: {metadata}")
        print("Done")