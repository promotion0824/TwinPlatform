from typing import Dict, Any
import logging
import os
import platform

from opentelemetry import trace
from opentelemetry import metrics
from opentelemetry.sdk._logs import (LoggerProvider, LoggingHandler)
from azure.monitor.opentelemetry.exporter import AzureMonitorLogExporter, AzureMonitorMetricExporter
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk._logs.export import BatchLogRecordProcessor
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.metrics.view import View
from opentelemetry.sdk.metrics.export import PeriodicExportingMetricReader
from regex import B
#from azure.monitor.opentelemetry import configure_azure_monitor
#from opentelemetry._logs import set_logger_provider

DebugMode = os.environ.get("COPILOT_DEBUG", "").lower() == "true"

EnableLocalCloudLogs = os.environ.get("COPILOT_DEBUG_ENABLE_LOCAL_APPINSIGHTS", "").lower() == "true"
EnableLocalCloudLogs = True

# TODO: change to INFO after development
log_level_name = os.environ.get("COPILOT_LOG_LEVEL") or "DEBUG"
LogLevel = logging.getLevelName(log_level_name) # operates in either direction

ConnectionString = os.environ.get("ApplicationInsights__ConnectionString")
default_env = "local"
CopilotAppRoleName = "copilot"
CopilotLoggerName = "copilot"

# "dev"
env_short_name = os.environ.get("WillowContext__EnvironmentConfiguration__ShortName") \
                or platform.node() or default_env
# "eus"
env_region = os.environ.get("WillowContext__RegionConfiguration__ShortName") or default_env
"01"
env_stamp = os.environ.get("WillowContext__StampConfiguration__Name") or default_env
# "wil-in1"
env_cust = os.environ.get("WillowContext__CustomerInstanceConfiguration__CustomerInstanceName") or default_env

image_version = os.environ.get("Image") or ""
deployment_time = os.environ.get("DeploymentTime") or ""

# "prd:eus2:15:wmr-uat"
def get_full_instance():
    return f"{env_short_name}:{env_region}:{env_stamp}:{env_cust}"

# dev:eus:01:wil-in1:Willow.AzureDigitalTwins.Api:adt-api--bhv9394-789cddc68c-hdxzm
def get_full_role_instance():
    replica_name = os.environ.get("CONTAINER_APP_REPLICA_NAME") or default_env
    return f"{get_full_instance()}:{replica_name}"

custom_dimensions = {
    "AppRoleName": CopilotAppRoleName,
    "AppVersion": image_version or "copilot:local",
    "AppRoleInstance": get_full_role_instance(),
    "FullCustomerInstanceName": get_full_instance(),
    "CustomerInstanceName": env_cust
}

MerticCounterInfo = [
    ("Copilot.StartupCount", "Count of container startups"),
    ("Copilot.InitalizedCount", "Count of container initializations complete"),
    ("Copilot.ChatRequestCount", "Count of succesful chat requests"),
    ("Copilot.IndexDocumentCount", "Count of documents indexed"),
    ("Copilot.DeleteDocumentCount", "Count of documents deleted"),
    ("Copilot.GetDocumentInfoCount", "Count of document info requests"),
    ("Copilot.FindIndexDocumentsCount", "Count of find index document requests"),
    ("Copilot.IndexRebuildRequestCount", "Count of index rebuild requests"),
    ("Copilot.CreateSummaryCount", "Count of summary creation")
]

MetricHistogramInfo = [
    ("Copilot.ChatRequestDuration", "Duration of succesful chat request", "ms"),
    ("Copilot.IndexDocumentDuration", "Duration of document indexing", "ms"),
    ("Copilot.IndexRebuildDuration", "Duration of index rebuild", "ms"),
    ("Copilot.CreateSummaryDuration", "Duration of summary creation", "ms")
]

copilot_meter = None

def _configure_cloud_logger(base_logger):

    role_instance = get_full_role_instance()

    resource = Resource.create({
        # maps to cloud_RoleName
        "service.name": "copilot", 
        # maps to cloud_RoleInstance (dev:eus:01:wil-lt:Willow.AzureDigitalTwins.Api:adt-api--bvfuduw-5b5f555b66-qwpkn)
        "service.instance.id": role_instance 
    })

    trace_provider = TracerProvider(resource = resource)
    trace.set_tracer_provider(trace_provider)
    log_provider = LoggerProvider(resource=resource)
    log_exporter = AzureMonitorLogExporter(connection_string = ConnectionString)

    #set_logger_provider(logProvider) # sets global logger provider - this will output logs from all imported libs that log
    log_provider.add_log_record_processor(BatchLogRecordProcessor(log_exporter))
    log_handler = LoggingHandler(logger_provider=log_provider)
    base_logger.addHandler(log_handler)

    metrics_exporter = AzureMonitorMetricExporter(connection_string=ConnectionString)
    reader = PeriodicExportingMetricReader(metrics_exporter)

    #  By default the python OT SDK will collapse all the metric names to lower case,
    #    so we need to pass in a list of views to the MeterProvider which needs to be
    #    created before the actual instruments are.
    views = [
            View(instrument_name=info[0].lower(), name=info[0]) 
        for info in MerticCounterInfo + MetricHistogramInfo
    ]
    metrics.set_meter_provider(MeterProvider(
        metric_readers=[reader], 
        resource=resource,
        views = views ))  
    global copilot_meter
    copilot_meter = metrics.get_meter_provider().get_meter("copilot")


def configure_cloud_logger(base_logger):
    try:
        return _configure_cloud_logger(base_logger)
    except Exception as ex:
        # Use print here when logging not available - check container console logs
        # We could also decide to do this for all logs when initializing appinsights fails
        print(f"Error configuring cloud logger: {ex}")

base_logger = logging.getLogger(CopilotLoggerName)
base_logger.setLevel(LogLevel)

if DebugMode:
    print("logger debug mode")
    # Add stdout handler as well as outputting to Azure 
    console_handler = logging.StreamHandler()
    console_handler.setFormatter(logging.Formatter('%(asctime)s: %(message)s'))
    base_logger.addHandler(console_handler)

logger = base_logger

if ConnectionString and (not DebugMode or EnableLocalCloudLogs):
    print(f"Configuring cloud logger with key ending: {ConnectionString[-5:-1]}")
    configure_cloud_logger(logger)
    # Add our extra dimensions to the logger by default, otherwise we would have to add them to every log call
    logger = logging.LoggerAdapter(base_logger, custom_dimensions)
    print("Cloud logger configured")

# TODO: Figure out how to set the OperationId

logger.setLevel(LogLevel)
# Now the logger is set up and this var can be imported and utlized -
#  however, we provide wrappers in case we decide to change the logger implementation to use the tracer directly.

# Needed to reach up from callstack in logger into the fn that's calling these to get correct file/line info (don't know why it's not 2)
# It doesn't seem to be easy to remove this information completely from the logrecord, which ends up in the custom dimensions.
# TODO: need to check if called by timed() wrapper
_stack_level = 4 
def log_debug(*args, **kwargs): logger.debug(*args, stacklevel=_stack_level, **kwargs)
def log_info(*args, **kwargs): logger.info(*args, stacklevel=_stack_level, **kwargs)
def log_warning(*args, **kwargs): logger.warning(*args, stacklevel=_stack_level, **kwargs)
def log_error(*args, **kwargs): logger.error(*args, stacklevel=_stack_level, **kwargs)
def log_exception(*args, **kwargs): logger.exception(*args, stacklevel=_stack_level, **kwargs)
def log_critical(*args, **kwargs): logger.critical(*args, stacklevel=_stack_level, **kwargs)

## Define telemetry metrics and call wrappers
# Note metrics are collapsed to lower case
# See: https://github.com/Azure/azure-sdk-for-python/issues/34465

#dict_counters = {}
#dict_histograms = {}
#def make_counter(name, description, unit): dict_counters[name] = (name, description, unit)
#def make_histogram(name, description, unit): dict_histograms[name] = (name, description, unit)

TeleCounters = {}
TeleHistograms = {}

if copilot_meter:

    # Create open telemetry instruments from name,desc,(unit) tuples
    TeleCounters = {
            name: copilot_meter.create_counter(name=name, 
                                            description=desc, 
                                            unit="count") 
        for name, desc in MerticCounterInfo
    }
    TeleHistograms = {
            name: copilot_meter.create_histogram(name=name, 
                                                description=desc, 
                                                unit=unit) 
        for name, desc, unit in MetricHistogramInfo
    }
    StatusOK = {"Status": "OK"}
    StatusFailed = {"Status": "Failed"}


# Create convenience fns for adding metadata to the instruments.
# Note we could append "OK" or "Failed" to the name if we wanted to have separate metrics 
#   for success/failure, but for now we're using a single metric with a Status.

def Status(ok): 
    return StatusOK if ok else StatusFailed

def BumpCounter(name, extra_info:Dict[str,Any] = {}, value = 1): 
    if not copilot_meter: return
    TeleCounters[name].add(value, {**custom_dimensions, **extra_info})
def BumpCounterStatus(name, ok=True): BumpCounter(name, Status(ok))

def RecordHistogram(name, value, extra_info: Dict[str,Any] = {}): 
    if not copilot_meter: return
    TeleHistograms[name].record(value, {**custom_dimensions, **extra_info})
def RecordHistogramStatus(name, value, ok = True): 
    RecordHistogram(name, value, Status(ok))

# Define wrappers for individual metrics

def MetricInitalized(ok = True): 
    BumpCounterStatus("Copilot.InitalizedCount", ok)
def MetricStartup(): 
    BumpCounterStatus("Copilot.StartupCount")

def MetricChat(duration, ok = True): 
    BumpCounterStatus("Copilot.ChatRequestCount", ok)
    RecordHistogramStatus("Copilot.ChatRequestDuration", duration, ok)

def MetricIndexDocument(duration, ok = True): 
    BumpCounterStatus("Copilot.IndexDocumentCount", ok)
    RecordHistogramStatus("Copilot.IndexDocumentDuration", duration, ok)

def MetricDeleteDocument(ok = True, nDeleted = 0): 
    BumpCounter("Copilot.DeleteDocumentCount", 
                    { **Status(ok), "IndexDocsDeletedCount": nDeleted })

def MetricGetDocumentInfo(ok = True, fileFound = True): 
    BumpCounter("Copilot.GetDocumentInfoCount", { 
                    **Status(ok), "FileFound": fileFound 
                })

def MetricFindIndexDocuments( ok = True, nTotal = 0, 
                             nPage = 0, includeMeta = False, includeContent = False ): 
    BumpCounter("Copilot.FindIndexDocumentsCount", { 
                    **Status(ok), 
                    "TotalIndexDocsFoundCount": nTotal, "PageIndexDocsFoundCount": nPage,
                    "IncludeMetadata": includeMeta, "IncludeContent": includeContent
                 })

def MetricIndexRebuildRequest(ok:bool, delete_and_recreate:bool,
                              summary_mode:str, index_mode:str): 
    BumpCounter("Copilot.IndexRebuildRequestCount", {
                    **Status(ok),
                    "DeleteAndRecreate": delete_and_recreate,
                    "SummaryMode": summary_mode,
                    "IndexMode": index_mode 
                })

def MetricIndexRebuildComplete(
                duration:int, ok:bool, delete_and_recreate:bool,
                summary_mode:str, index_mode:str): 
    RecordHistogram("Copilot.IndexRebuildDuration", duration,
                    { **Status(ok),
                      "DeleteAndRecreate": delete_and_recreate,
                      "SummaryMode": summary_mode,
                      "IndexMode": index_mode 
                    })

def MetricCreateSummary(duration:int, ok:bool, summary_mode:str, from_cache:bool): 
    BumpCounter("Copilot.CreateSummaryCount", { **Status(ok), 
                                               "SummaryMode": summary_mode 
                                               })
    RecordHistogram("Copilot.CreateSummaryDuration", 
                    duration, { **Status(ok), 
                                "SummaryMode": summary_mode,
                                "FromCache": from_cache
                               })



"""
# TODO: finish nested trace/context support
class OpenTelemetryLogging:
    def trace(self, message):             

        tracer = trace.get_tracer(__name__)

        with tracer.start_as_current_span("main_request"):

            logger = logging.getLogger(__name__)
            logger.info(message, extra=custom_dimensions)                      

            # Create a counter instrument
            #counter = meter.create_counter(
            #    name="hello_counter",
            #    description="Counts how many times hello has been called",
            #    unit="counts",
            #)
            #counter.add(1)

            current_span = trace.get_current_span()
            current_span.add_event("Span message", {"message": message})
"""

def main():
    import time
    if True:
        message = f"Copilot Logtest: {time.asctime()}"
        log_debug(message)
        log_info(message)
        log_warning(message)
        log_error(message)
        log_exception(message)
        log_critical(message)

if __name__ == "__main__":
    main()