import os
from typing import Dict
# TODO: After python 3.11, this is defined in typing
from typing_extensions import Self

from .Utils import recurse_remove_empty_elements
from .OpenTelemetry import log_info, log_debug

# TODO: Support TotalDuration if useful

image_version = os.environ.get("Image") or "copilot:local"

class HealthCheckStatus:

    Status_Unhealthy = 0
    Status_Degraded = 1
    Status_Healthy = 2

    Desc_Healthy = "Healthy"
    Desc_Starting = "Starting"
    Desc_FailingCalls = "Failing Calls"
    Desc_Degraed = "Degraded"

    def __init__(self, key:str, description:str|None = None, version:str|None = None):
        self.key = key
        self.last_status = None
        self.entries : Dict[str, Self] = {}
        self.version = version
        self.set_healthy(HealthCheckStatus.Desc_Starting)

    def output_log(self):
        # ouput to log only if health status has changed
        if self.current_status != self.last_status:
            self.last_status = self.current_status
            log_info(self)

    def add_dependency(self, dependency: Self):
        self.entries[dependency.key] = dependency

    def set_healthy(self, description:str|None = None):
        self.current_status = HealthCheckStatus.Status_Healthy
        self.description = description or HealthCheckStatus.Desc_Healthy
        self.output_log()

    # This is currently unused - need heuristics 
    def set_degraded(self, description:str|None = None):
        self.current_status = HealthCheckStatus.Status_Degraded
        self.description = description or HealthCheckStatus.Desc_Degraed
        self.output_log()
    
    def set_failing(self, description:str|None = None):
        self.current_status = HealthCheckStatus.Status_Unhealthy
        self.description = description or HealthCheckStatus.Desc_FailingCalls
        self.output_log()

    def status_str(self) -> str:
        return "Healthy" if self.current_status == HealthCheckStatus.Status_Healthy \
            else "Degraded" if self.current_status == HealthCheckStatus.Status_Degraded \
            else "Unhealthy"

    def __str__(self) -> str:
        return f"HealthCheckStatus {self.key}: {self.status_str()} '{self.description or 'unknown'}'"

    def get_json(self) -> Dict:
        # skip "version": null, etc.
        return recurse_remove_empty_elements( self._get_json() )

    # TODO: Need to propagate status - currently dependent services can be unhealthy 
    #  and copilot can still be healthy
    def _get_json(self) -> Dict:
        json = {
            "Key": self.key,
            "Status": self.current_status,
            "Description": self.description or self.status_str(),
            "Version": self.version,
            "Entries": {k: v.get_json() for k, v in self.entries.items()}
        }
        if not self.entries: del json["Entries"] 
        return json

class HealthChecks:

    def __init__(self):

        self.healthcheck_copilot = HealthCheckStatus(
                    "copilot", 
                    version = image_version
                )

        self.healthcheck_azure_ai_search = HealthCheckStatus(
                "azure-search",
            )

        self.healthcheck_openai = HealthCheckStatus(
                "azure-openai",
            )

        self.healthcheck_copilot.add_dependency(self.healthcheck_azure_ai_search)
        self.healthcheck_copilot.add_dependency(self.healthcheck_openai)

    def get_root(self) -> HealthCheckStatus:
        return self.healthcheck_copilot

# This is module level-variable, therefore a signleton
CopilotHealthChecks = HealthChecks()
