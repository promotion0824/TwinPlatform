# platform-web

Website for Willow Platform.

# Build Docker image

Dockerfile copies the published output of web

```
npm install
npm run build
docker build -t web:latest -f packages/platform/deployment/Dockerfile packages/platform/
```
