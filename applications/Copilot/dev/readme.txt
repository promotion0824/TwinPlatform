
Notes:
    - Must Remove from any requirements.txt of any new python virtua env 
       - not needed and won't run in linux image
       - pywin-32, portallocker
    - AzureIdentity requires mssal-extensions which requires pywin32 - but not loaded a runtime when running in linux container
    - Can remove Pylint and pyLance from requirements.txt for deployment 
            - need local only to override globally installed version
            (supposedly can config vscode's global version to find all libs - but no luck)

    - Remove version from opentelemetry-semantic-conventions
        opentelemetry-instrumentation-asgi 0.43b0 depends on 
         opentelemetry-semantic-conventions==0.43b0