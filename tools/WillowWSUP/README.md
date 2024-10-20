This is a Willow Admin application, it consists of a front-end Vite React App and a backend BFF C# App.

You will need to configure various appsettings.

To run:

build the vite app: `cd ClientApp` / `npm run build`

launch the dotnet app: `dotnet run --project AdminApp/`

launch the vite app: `cd ClientApp` / `npm run dev`



To generate the typescript http client for the backend:
---

`npx openapi-typescript-codegen -i http://localhost:5000/swagger/v1/swagger.json -o ./src/generated`

To build Docker container and publish it
---
`dotnet publish`

`cd ClientApp`
`npm run build`


`docker buildx build --platform linux/amd64 . -t crwilsbxshared01.azurecr.io/wsup`

`az acr login --name crwilsbxshared01`
`docker push crwilsbxshared01.azurecr.io/wsup`



Preamble
---
`az login`
`az extension add --name containerapp --upgrade`

`az provider register --namespace Microsoft.App`

az provider register --namespace Microsoft.OperationalInsights

RESOURCE_GROUP="rulesengine-poc"
LOCATION="eu2"
CONTAINERAPPS_ENVIRONMENT="wsup"

