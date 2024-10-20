
dotnet publish

cd ClientApp
npm run build
cd ..


docker buildx build --platform linux/amd64 . -t crwilsbxshared01.azurecr.io/wsup

az acr login --name crwilsbxshared01
docker push crwilsbxshared01.azurecr.io/wsup

echo Complete
