# MobileXL

## How to run it locally

- Start the service

```sh
dotnet run
```

A profile has also been set up to start against UAT:

```sh
dotnet run --launch-profile "UAT"
```

- Verify the service is running. Open your favorite browser and navigate to these two links

```
https://localhost:5001/swagger
https://localhost:5001/healthcheck
```
