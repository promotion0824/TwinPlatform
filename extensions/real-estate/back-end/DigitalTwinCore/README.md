# DigitalTwinCore

## How to run it locally
* Make sure Docker has been installed and running
* Run the command to start a SQL server container
```cmd
docker run -d --rm --name test-mssql -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Password01!" -p 61234:1433 mcr.microsoft.com/mssql/server:2019-latest
```
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
docker build --build-arg FEED_ACCESSTOKEN=$env:FEED_ACCESSTOKEN -t digitaltwincore:latest -f .\src\DigitalTwinCore\Dockerfile.local .
```