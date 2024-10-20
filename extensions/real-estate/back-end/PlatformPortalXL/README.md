# PlatformPortalXL

## How to run it locally
* Start the service
```
dotnet run
```
* Verify the service is running. Open your favorite browser and navigate to these two links
```
https://localhost:5001/swagger
https://localhost:5001/healthcheck
```



# Build Docker
Building docker image requires a AzureDevops pat token with permission to read artifact feeds

```powershell
docker build --build-arg FEED_ACCESSTOKEN=$env:FEED_ACCESSTOKEN -t portalxl:latest -f .\src\PlatformPortalXL\Dockerfile.local .
```