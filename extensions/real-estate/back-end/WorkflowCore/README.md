# WorkflowCore



# Build Docker
Building docker image requires a AzureDevops pat token with permission to read artifact feeds

```powershell
docker build --build-arg FEED_ACCESSTOKEN=$env:FEED_ACCESSTOKEN -t workflowcore:latest -f .\src\WorkflowCore\Dockerfile.local .
```